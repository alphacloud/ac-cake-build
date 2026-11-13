using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.Tools.GitVersion;
using Cake.Core;
using Cake.Frosting;

namespace Alphacloud.CakeBuild;

/// <summary>Build state shared by all tasks. Resolved once per run.</summary>
public class BuildContext : FrostingContext
{
    /// <summary>Creates the context; called by the Frosting DI container.</summary>
    public BuildContext(ICakeContext context, ProjectSettings settings)
        : base(context)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Settings = settings;

        Target = context.Argument("target", "Default");
        Config = context.Argument("buildConfig", "Release");
        IsRelease = string.Equals(Config, "Release", StringComparison.OrdinalIgnoreCase);

        var buildSystem = context.BuildSystem();
        IsLocal = buildSystem.IsLocalBuild;
        IsAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor;

        Paths = new Paths(context.Environment.WorkingDirectory.FullPath, settings);

        var env = buildSystem.AppVeyor.Environment;
        Repository = IsAppVeyor
            ? RepositoryInfo.Create(settings, env.Repository.Name, env.Repository.Branch, env.PullRequest.IsPullRequest, env.Repository.Tag.IsTag)
            : RepositoryInfo.Create(settings, null, null, isPullRequest: false, isTagged: false);

        Version = Repository.IsPullRequest
            ? BuildVersion.ForPullRequest(env.PullRequest.Number, env.Build.Number, env.Repository.Commit.Id, env.PullRequest.Title)
            : CalculateGitVersion(context);

        GitHubToken = context.EnvironmentVariable("GITHUB_TOKEN");

        context.Information(
            "Building {0} {1} (config: {2}, local: {3}, tagged: {4}, release branch: {5}, PR: {6})",
            settings.SolutionName, Version.NuGet, Config, IsLocal, Repository.IsTagged, Repository.IsReleaseBranch, Repository.IsPullRequest);
    }

    /// <summary>Project settings supplied by the consumer.</summary>
    public ProjectSettings Settings { get; }

    /// <summary>Requested target (<c>--target</c>, default <c>Default</c>).</summary>
    /// <remarks>Not used by built-in tasks; kept for consumer-defined tasks and logging.</remarks>
    public string Target { get; }

    /// <summary>Build configuration (<c>--buildConfig</c>, default <c>Release</c>).</summary>
    public string Config { get; }

    /// <summary>Configuration is Release.</summary>
    public bool IsRelease { get; }

    /// <summary>Running outside of CI.</summary>
    public bool IsLocal { get; }

    /// <summary>Running on AppVeyor.</summary>
    public bool IsAppVeyor { get; }

    /// <summary>Repository / branch state.</summary>
    public RepositoryInfo Repository { get; }

    /// <summary>Calculated version.</summary>
    public BuildVersion Version { get; }

    /// <summary>GitHub token (<c>GITHUB_TOKEN</c>), required by release tasks.</summary>
    public string? GitHubToken { get; }

    /// <summary>Resolved paths.</summary>
    public Paths Paths { get; }

    /// <summary>Deferred test failure, re-thrown by <c>RunUnitTests</c> after coverage is reported.</summary>
    public Exception? TestFailure { get; internal set; }

    private static BuildVersion CalculateGitVersion(ICakeContext context)
    {
        // Force the pinned GitVersion.Tool (dotnet-gitversion) so a stale legacy GitVersion.exe in the tools
        // cache is never picked up. GitVersion 6 removed NuGetVersion; SemVer is its replacement.
        var gitVersion = context.GitVersion(new GitVersionSettings
        {
            OutputType = GitVersionOutput.Json,
            ToolPath = ToolResolver.Resolve(context, "dotnet-gitversion"),
        });

        return new BuildVersion(
            gitVersion.SemVer,
            gitVersion.FullBuildMetaData,
            gitVersion.InformationalVersion,
            $"{gitVersion.Major + 1}.0.0",
            gitVersion.Sha,
            gitVersion.MajorMinorPatch);
    }
}
