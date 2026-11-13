using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Tools.ReportGenerator;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Alphacloud.CakeBuild.Tasks;

/// <summary>Generates the HTML coverage report (local builds only).</summary>
[TaskName("GenerateCoverageReport")]
public sealed class GenerateCoverageReportTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) => context.IsLocal;

    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        if (!context.FileExists(context.Paths.CoverageFile))
        {
            context.Warning("Coverage file {0} not found; skipping report.", context.Paths.CoverageFile);
            return;
        }

        context.ReportGenerator(new FilePath(context.Paths.CoverageFile), context.Paths.CoverageReportDir, new ReportGeneratorSettings
        {
            ReportTypes = [ReportGeneratorReportType.Html, ReportGeneratorReportType.TextSummary],
        });
    }
}

/// <summary>
/// Uploads coverage to Coveralls with the coverage-reporter binary (CI only). Its path comes from
/// <c>COVERALLS_REPORTER_EXE</c> (default <c>coveralls-windows.exe</c> / <c>coveralls</c> on PATH);
/// the repo token from <c>COVERALLS_REPO_TOKEN</c>.
/// </summary>
[TaskName("UploadCoverage")]
public sealed class UploadCoverageTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) => !context.IsLocal;

    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        if (!context.FileExists(context.Paths.CoverageFile))
        {
            context.Warning("Coverage file {0} not found; skipping Coveralls upload.", context.Paths.CoverageFile);
            return;
        }

        var reporter = context.EnvironmentVariable("COVERALLS_REPORTER_EXE")
                       ?? ToolResolver.ExecutableName(OperatingSystem.IsWindows() ? "coveralls-windows" : "coveralls");
        var exitCode = context.StartProcess(reporter, new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("report")
                .AppendQuoted(context.Paths.CoverageFile)
                .Append("--format=cobertura")
                .AppendSwitchQuoted("--base-path", "=", context.Paths.RootDir)
                .Append("--no-logo"),
        });

        if (exitCode != 0)
            throw new CakeException($"Coveralls upload failed with exit code {exitCode}.");
    }
}
