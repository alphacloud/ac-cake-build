namespace Alphacloud.CakeBuild;

/// <summary>Absolute paths resolved from <see cref="ConventionSettings"/> and the repository root.</summary>
public sealed class Paths
{
    /// <summary>Resolves paths for a repository.</summary>
    public Paths(string rootDir, ProjectSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDir);
        ArgumentNullException.ThrowIfNull(settings);

        var c = settings.Conventions;
        RootDir = Path.GetFullPath(rootDir);
        SrcDir = Path.Combine(RootDir, c.SrcDir);
        LibDir = Path.Combine(SrcDir, c.LibDir);
        TestsDir = Path.Combine(SrcDir, c.TestsDir);
        SamplesDir = Path.Combine(SrcDir, c.SamplesDir);
        CommonDir = Path.Combine(SrcDir, c.CommonDir);
        ArtifactsDir = Path.Combine(RootDir, c.ArtifactsDir);

        BuildPropsFile = Path.Combine(SrcDir, "Directory.Build.props");
        CommonAssemblyVersionFile = Path.Combine(CommonDir, "AssemblyVersion.cs");
        GitVersionConfigFile = Path.Combine(RootDir, "GitVersion.yml");

        PackagesDir = Path.Combine(ArtifactsDir, "packages");
        TestResultsDir = Path.Combine(ArtifactsDir, "test-results");
        CoverageDir = Path.Combine(ArtifactsDir, "coverage");
        CoverageSettingsFile = Path.Combine(CoverageDir, "codecoverage.config");
        CoverageFile = Path.Combine(CoverageDir, "coverage.cobertura.xml");
        CoverageReportDir = Path.Combine(ArtifactsDir, "CodeCoverageReport");

        SolutionFile = ResolveSolution(SrcDir, settings.SolutionName);
    }

    /// <summary>Repository root.</summary>
    public string RootDir { get; }
    /// <summary>Source root (<see cref="ConventionSettings.SrcDir"/>).</summary>
    public string SrcDir { get; }
    /// <summary>Library projects folder.</summary>
    public string LibDir { get; }
    /// <summary>Test projects folder.</summary>
    public string TestsDir { get; }
    /// <summary>Samples folder (optional).</summary>
    public string SamplesDir { get; }
    /// <summary>Common files folder.</summary>
    public string CommonDir { get; }
    /// <summary>Build output root.</summary>
    public string ArtifactsDir { get; }
    /// <summary><c>Directory.Build.props</c> holding the <c>Package</c> property group.</summary>
    public string BuildPropsFile { get; }
    /// <summary>Generated <c>AssemblyVersion.cs</c>.</summary>
    public string CommonAssemblyVersionFile { get; }
    /// <summary><c>GitVersion.yml</c> at the repository root.</summary>
    public string GitVersionConfigFile { get; }
    /// <summary>NuGet package output.</summary>
    public string PackagesDir { get; }
    /// <summary>TRX output.</summary>
    public string TestResultsDir { get; }
    /// <summary>Coverage output folder.</summary>
    public string CoverageDir { get; }
    /// <summary>Generated dotnet-coverage settings file.</summary>
    public string CoverageSettingsFile { get; }
    /// <summary>Merged Cobertura report (all projects and target frameworks).</summary>
    public string CoverageFile { get; }
    /// <summary>HTML coverage report folder.</summary>
    public string CoverageReportDir { get; }
    /// <summary>Solution file; <c>.slnx</c> preferred over <c>.sln</c>. Points to <c>.slnx</c> if neither exists.</summary>
    public string SolutionFile { get; }

    private static string ResolveSolution(string srcDir, string solutionName)
    {
        var slnx = Path.Combine(srcDir, solutionName + ".slnx");
        var sln = Path.Combine(srcDir, solutionName + ".sln");
        return File.Exists(slnx) || !File.Exists(sln) ? slnx : sln;
    }
}
