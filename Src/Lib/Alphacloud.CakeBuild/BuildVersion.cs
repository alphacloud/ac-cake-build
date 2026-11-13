namespace Alphacloud.CakeBuild;

/// <summary>
/// Version numbers calculated once per build and exposed through <see cref="BuildContext.Version"/>.
/// Members not consumed by the built-in tasks (<see cref="NextMajor"/>, <see cref="CommitHash"/>) are public API
/// kept for consumer-defined tasks, e.g. version-range package references or commit-stamped artifacts.
/// </summary>
/// <param name="NuGet">Package version (GitVersion <c>SemVer</c>); used by <c>CreateNugetPackages</c>.</param>
/// <param name="Full">Full build metadata; used as the AppVeyor build number.</param>
/// <param name="Informational">Assembly informational version; used by <c>SetVersion</c>.</param>
/// <param name="NextMajor">Next major version, e.g. <c>3.0.0</c>. Not used by built-in tasks; for consumer tasks.</param>
/// <param name="CommitHash">Commit SHA. Not used by built-in tasks; for consumer tasks.</param>
/// <param name="Milestone">
/// <c>Major.Minor.Patch</c>; used as assembly/file version, GitHub milestone name and release tag.
/// </param>
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
