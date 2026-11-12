using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Build.AppVeyor;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Path = System.IO.Path;

namespace Alphacloud.CakeBuild.Tasks;

/// <summary>Removes coverage, TRX and report output of previous runs.</summary>
[TaskName("CleanPreviousTestResults")]
public sealed class CleanPreviousTestResultsTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        foreach (var dir in new[] { context.Paths.TestResultsDir, context.Paths.CoverageDir, context.Paths.CoverageReportDir })
        {
            if (context.DirectoryExists(dir))
                context.DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true, Force = true });
        }
    }
}

/// <summary>
/// Runs tests (xunit.v3 on Microsoft.Testing.Platform) for all projects and target frameworks of the solution.
/// The Debug run is wrapped by <c>dotnet-coverage collect</c>, producing one merged Cobertura report.
/// Test failures are deferred: <c>RunUnitTests</c> re-throws them after coverage is reported.
/// </summary>
[TaskName("RunXunitTests")]
public sealed class RunXunitTestsTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var paths = context.Paths;
        var tests = context.Settings.Tests;
        context.EnsureDirectoryExists(paths.TestResultsDir);
        CoverageSettingsXml.Write(context.Settings.CodeCoverage, paths.CoverageSettingsFile);

        var failures = new List<string>();

        context.Information("Running Debug tests with code coverage for {0} ...", context.Settings.SolutionName);
        var debugArgs = TestCommand.Arguments(paths.SolutionFile, "Debug", Path.Combine(paths.TestResultsDir, "Debug"), tests);
        var coverageArgs = new ProcessArgumentBuilder()
            .Append("collect")
            .AppendSwitchQuoted("--settings", paths.CoverageSettingsFile)
            .AppendSwitch("--output-format", "cobertura")
            .AppendSwitchQuoted("--output", paths.CoverageFile)
            .Append("--nologo")
            .AppendQuoted(TestCommand.ToCommandLine("dotnet", debugArgs));
        Execute(context, ToolResolver.Resolve(context, "dotnet-coverage"), coverageArgs, "Debug tests", failures);

        if (tests.RunReleaseTests && context.IsRelease)
        {
            context.Information("Running Release tests for {0} ...", context.Settings.SolutionName);
            var releaseArgs = new ProcessArgumentBuilder();
            foreach (var arg in TestCommand.Arguments(paths.SolutionFile, context.Config, Path.Combine(paths.TestResultsDir, "Release"), tests))
                releaseArgs.AppendQuoted(arg);
            Execute(context, "dotnet", releaseArgs, "Release tests", failures);
        }

        UploadTestResults(context);

        if (failures.Count > 0)
            context.TestFailure = new CakeException("Test run failed: " + string.Join("; ", failures));
    }

    private static void Execute(BuildContext context, FilePath tool, ProcessArgumentBuilder args, string name, List<string> failures)
    {
        var exitCode = context.StartProcess(tool, new ProcessSettings { Arguments = args });
        if (exitCode == 0)
            return;
        context.Error("{0} failed with exit code {1}", name, exitCode);
        failures.Add($"{name} (exit code {exitCode})");
    }

    private static void UploadTestResults(BuildContext context)
    {
        if (!context.IsAppVeyor)
            return;
        var appVeyor = context.BuildSystem().AppVeyor;
        foreach (var trx in context.GetFiles($"{context.Paths.TestResultsDir}/**/*.trx"))
        {
            try
            {
                appVeyor.UploadTestResults(trx, AppVeyorTestResultsType.MSTest);
            }
            catch (Exception ex)
            {
                context.Warning("Could not upload test results {0}: {1}", trx.GetFilename(), ex.Message);
            }
        }
    }
}

/// <summary>Test pipeline; re-throws deferred test failures after coverage reporting.</summary>
[TaskName("RunUnitTests")]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(CleanPreviousTestResultsTask))]
[IsDependentOn(typeof(RunXunitTestsTask))]
[IsDependentOn(typeof(GenerateCoverageReportTask))]
[IsDependentOn(typeof(UploadCoverageTask))]
public sealed class RunUnitTestsTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        if (context.TestFailure is not null)
            throw context.TestFailure;
        context.Information("Done Test");
    }
}

internal static class ToolResolver
{
    public static FilePath Resolve(ICakeContext context, string name) =>
        context.Tools.Resolve(OperatingSystem.IsWindows() ? name + ".exe" : name)
        ?? throw new CakeException($"Tool '{name}' was not found. Is it installed via UseAlphacloudCakeBuild()?");
}
