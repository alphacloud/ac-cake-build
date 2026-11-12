namespace Alphacloud.CakeBuild;

/// <summary>Consumer project configuration, passed to <c>UseAlphacloudCakeBuild</c>.</summary>
public class ProjectSettings
{
    /// <summary>Creates settings.</summary>
    /// <param name="repoOwner">GitHub repository owner/organization.</param>
    /// <param name="repoName">GitHub repository name.</param>
    /// <param name="solutionName">Solution file name without extension (<c>.slnx</c> or <c>.sln</c>).</param>
    public ProjectSettings(string repoOwner, string repoName, string solutionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repoOwner);
        ArgumentException.ThrowIfNullOrWhiteSpace(repoName);
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionName);

        RepoOwner = repoOwner;
        RepoName = repoName;
        SolutionName = solutionName;
        CodeCoverage.IncludeModulePaths.Add($@".*[\\/]{System.Text.RegularExpressions.Regex.Escape(solutionName)}[^\\/]*\.dll$");
    }

    /// <summary>GitHub repository owner/organization.</summary>
    public string RepoOwner { get; }

    /// <summary>GitHub repository name.</summary>
    public string RepoName { get; }

    /// <summary>Solution file name without extension.</summary>
    public string SolutionName { get; }

    /// <summary>Folder layout conventions.</summary>
    public ConventionSettings Conventions { get; } = new();

    /// <summary>Code coverage filters.</summary>
    public CodeCoverageSettings CodeCoverage { get; } = new();

    /// <summary>Test execution options.</summary>
    public TestSettings Tests { get; } = new();

    /// <summary>Branch GitHub releases are created against. Default: <c>master</c>.</summary>
    public string ReleaseTargetBranch { get; set; } = "master";
}
