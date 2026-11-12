namespace Alphacloud.CakeBuild;

/// <summary>Version numbers used by the build.</summary>
/// <param name="NuGet">Package version (GitVersion <c>SemVer</c>).</param>
/// <param name="Full">Full build metadata, used as the AppVeyor build number.</param>
/// <param name="Informational">Assembly informational version.</param>
/// <param name="NextMajor">Next major version, e.g. <c>3.0.0</c>.</param>
/// <param name="CommitHash">Commit SHA.</param>
/// <param name="Milestone">Major.Minor.Patch, used as assembly version and GitHub milestone name.</param>
public sealed record BuildVersion(
    string NuGet,
    string Full,
    string Informational,
    string NextMajor,
    string CommitHash,
    string Milestone)
{
    /// <summary>
    /// Synthesizes a <c>0.&lt;PR&gt;.&lt;build&gt;</c> version for pull-request builds, where GitVersion fails.
    /// </summary>
    public static BuildVersion ForPullRequest(int pullRequestNumber, int buildNumber, string? commitHash, string? pullRequestTitle)
    {
        var version = $"0.{pullRequestNumber}.{buildNumber}";
        var hash = commitHash ?? string.Empty;
        return new BuildVersion(
            version,
            $"{version}/{hash}-PR-{pullRequestTitle}",
            version,
            $"{version}.0",
            hash,
            version);
    }
}
