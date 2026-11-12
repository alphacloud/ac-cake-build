using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.GitReleaseManager;
using Cake.Common.Tools.GitReleaseManager.Create;
using Cake.Core;
using Cake.Frosting;

namespace Alphacloud.CakeBuild.Tasks;

/// <summary>Packs the solution's packable projects into <c>artifacts/packages</c>.</summary>
[TaskName("CreateNugetPackages")]
public sealed class CreateNugetPackagesTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override void Run(BuildContext context) =>
        context.DotNetPack(context.Paths.SolutionFile, new DotNetPackSettings
        {
            Configuration = context.Config,
            NoRestore = true,
            NoBuild = true,
            OutputDirectory = context.Paths.PackagesDir,
            ArgumentCustomization = args => args.Append($"-p:Version={context.Version.NuGet}"),
        });
}

/// <summary>Creates a draft GitHub release from the milestone (upstream release/hotfix branches only).</summary>
[TaskName("CreateRelease")]
public sealed class CreateReleaseTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) =>
        context.Repository is { IsMain: true, IsReleaseBranch: true, IsPullRequest: false };

    /// <inheritdoc />
    public override void Run(BuildContext context) =>
        context.GitReleaseManagerCreate(
            ReleaseGuard.RequireToken(context),
            context.Settings.RepoOwner,
            context.Settings.RepoName,
            new GitReleaseManagerCreateSettings
            {
                Milestone = context.Version.Milestone,
                TargetCommitish = context.Settings.ReleaseTargetBranch,
            });
}

/// <summary>Closes the GitHub milestone (upstream tagged builds only).</summary>
[TaskName("CloseMilestone")]
public sealed class CloseMilestoneTask : FrostingTask<BuildContext>
{
    /// <inheritdoc />
    public override bool ShouldRun(BuildContext context) =>
        context.Repository is { IsMain: true, IsTagged: true, IsPullRequest: false };

    /// <inheritdoc />
    public override void Run(BuildContext context) =>
        context.GitReleaseManagerClose(
            ReleaseGuard.RequireToken(context),
            context.Settings.RepoOwner,
            context.Settings.RepoName,
            context.Version.Milestone);
}

internal static class ReleaseGuard
{
    public static string RequireToken(BuildContext context) =>
        string.IsNullOrEmpty(context.GitHubToken)
            ? throw new CakeException("GITHUB_TOKEN environment variable is required for GitHub release tasks.")
            : context.GitHubToken;
}

/// <summary>Full pipeline.</summary>
[TaskName("Default")]
[IsDependentOn(typeof(UpdateAppVeyorBuildNumberTask))]
[IsDependentOn(typeof(BuildTask))]
[IsDependentOn(typeof(RunUnitTestsTask))]
[IsDependentOn(typeof(CreateNugetPackagesTask))]
[IsDependentOn(typeof(CreateReleaseTask))]
[IsDependentOn(typeof(CloseMilestoneTask))]
public sealed class DefaultTask : FrostingTask<BuildContext>
{
}
