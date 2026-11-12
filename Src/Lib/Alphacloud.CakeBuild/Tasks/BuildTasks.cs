using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Common.Solution.Project.Properties;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Xml;
using Cake.Core;
using Cake.Frosting;

namespace Alphacloud.CakeBuild.Tasks;

/// <summary>Deletes <c>bin</c>/<c>obj</c> folders under the source root and the artifacts folder content.</summary>
[TaskName("CleanAll")]
public sealed class CleanAllTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        context.CleanDirectories($"{context.Paths.SrcDir}/**/obj");
        context.CleanDirectories($"{context.Paths.SrcDir}/**/bin");
        context.CleanDirectory(context.Paths.ArtifactsDir);
    }
}

/// <summary>Fails fast when the repository does not follow the folder and naming conventions.</summary>
[TaskName("ValidateConventions")]
public sealed class ValidateConventionsTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var errors = ConventionValidator.Validate(context.Paths, context.Settings);
        if (errors.Count == 0)
            return;

        foreach (var error in errors)
            context.Error(error);
        throw new CakeException($"Repository does not follow Alphacloud.CakeBuild conventions ({errors.Count} issue(s), see above).");
    }
}

/// <summary>Generates <c>Common/AssemblyVersion.cs</c>.</summary>
[TaskName("SetVersion")]
public sealed class SetVersionTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists(context.Paths.CommonDir);
        context.CreateAssemblyInfo(context.Paths.CommonAssemblyVersionFile, new AssemblyInfoSettings
        {
            Version = context.Version.Milestone,
            FileVersion = context.Version.Milestone,
            InformationalVersion = context.Version.Informational,
        });
    }
}

/// <summary>Sets the AppVeyor build number to the full version (AppVeyor only; failures are logged, not fatal).</summary>
[TaskName("UpdateAppVeyorBuildNumber")]
public sealed class UpdateAppVeyorBuildNumberTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) => context.IsAppVeyor;

    /// <inheritdoc />
    public override void Run(BuildContext context) =>
        context.BuildSystem().AppVeyor.UpdateBuildVersion(context.Version.Full);

    /// <inheritdoc />
    public override void OnError(Exception exception, BuildContext context) =>
        context.Warning("Could not update AppVeyor build number: {0}", exception.Message);
}

/// <summary>Points <c>PackageReleaseNotes</c> at the GitHub release of the tagged version.</summary>
[TaskName("UpdateReleaseNotesLink")]
public sealed class UpdateReleaseNotesLinkTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) => context.Repository.IsTagged;

    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        var url = $"https://github.com/{context.Settings.RepoOwner}/{context.Settings.RepoName}/releases/tag/{context.Version.Milestone}";
        context.Information("Updating ReleaseNotes URL to '{0}'", url);
        context.XmlPoke(context.Paths.BuildPropsFile, ConventionValidator.ReleaseNotesLocation, url);
    }
}

/// <summary>Restores NuGet packages for the solution.</summary>
[TaskName("Restore")]
public sealed class RestoreTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context) =>
        context.DotNetRestore(context.Paths.SolutionFile, new DotNetRestoreSettings());
}

/// <summary>Builds the solution: Debug (for coverage) and the requested configuration.</summary>
[TaskName("Build")]
[IsDependentOn(typeof(ValidateConventionsTask))]
[IsDependentOn(typeof(SetVersionTask))]
[IsDependentOn(typeof(UpdateAppVeyorBuildNumberTask))]
[IsDependentOn(typeof(UpdateReleaseNotesLinkTask))]
[IsDependentOn(typeof(RestoreTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context)
    {
        if (!string.Equals(context.Config, "Debug", StringComparison.OrdinalIgnoreCase))
        {
            context.Information("Running Debug build (used for code coverage)");
            Build(context, "Debug");
        }

        context.Information("Running {0} build", context.Config);
        Build(context, context.Config);
    }

    private static void Build(BuildContext context, string configuration) =>
        context.DotNetBuild(context.Paths.SolutionFile, new DotNetBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
        });
}
