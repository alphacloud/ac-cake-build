namespace Alphacloud.CakeBuild;

/// <summary>Git/CI repository state used to gate release tasks (GitFlow).</summary>
public sealed class RepositoryInfo
{
    private static readonly string[] ReleaseBranchPrefixes = ["release/", "releases/", "hotfix/", "hotfixes/"];

    /// <summary>Build was triggered by a pull request.</summary>
    public bool IsPullRequest { get; init; }

    /// <summary>Build runs in the upstream repository <c>owner/name</c> (not a fork).</summary>
    public bool IsMain { get; init; }

    /// <summary>Branch is <c>develop</c>.</summary>
    /// <remarks>Not used by built-in tasks; kept for consumer-defined tasks (e.g. pre-release-only steps).</remarks>
    public bool IsDevelopBranch { get; init; }

    /// <summary>
    /// Branch is a GitFlow release or hotfix branch: its name <b>starts with</b> <c>release/</c>, <c>releases/</c>,
    /// <c>hotfix/</c> or <c>hotfixes/</c> (case-insensitive). Nested names such as <c>feature/release/x</c> do not match.
    /// </summary>
    public bool IsReleaseBranch { get; init; }

    /// <summary>Build was triggered by a tag.</summary>
    public bool IsTagged { get; init; }

    /// <summary>Branch name reported by CI (empty for local builds).</summary>
    public string Branch { get; init; } = string.Empty;

    /// <summary>Creates repository info from raw CI values.</summary>
    public static RepositoryInfo Create(
        ProjectSettings settings,
        string? repositoryName,
        string? branch,
        bool isPullRequest,
        bool isTagged)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var branchName = branch ?? string.Empty;
        return new RepositoryInfo
        {
            Branch = branchName,
            IsPullRequest = isPullRequest,
            IsTagged = isTagged,
            IsDevelopBranch = string.Equals(branchName, "develop", StringComparison.OrdinalIgnoreCase),
            IsReleaseBranch = ReleaseBranchPrefixes.Any(p => branchName.StartsWith(p, StringComparison.OrdinalIgnoreCase)),
            IsMain = string.Equals($"{settings.RepoOwner}/{settings.RepoName}", repositoryName, StringComparison.OrdinalIgnoreCase),
        };
    }
}
