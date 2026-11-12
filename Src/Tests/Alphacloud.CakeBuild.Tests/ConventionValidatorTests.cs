namespace Alphacloud.CakeBuild.Tests;

public sealed class ConventionValidatorTests : IDisposable
{
    private const string ValidProps =
        """<Project><PropertyGroup Label="Package"><PackageReleaseNotes>x</PackageReleaseNotes></PropertyGroup></Project>""";

    private readonly string _root = Path.Combine(Path.GetTempPath(), "acb-" + Guid.NewGuid().ToString("N"));
    private readonly ProjectSettings _settings = new("owner", "repo", "Sample");

    public ConventionValidatorTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "Src", "Lib"));
        Directory.CreateDirectory(Path.Combine(_root, "Src", "Tests"));
        File.WriteAllText(Path.Combine(_root, "GitVersion.yml"), "mode: ContinuousDelivery");
        File.WriteAllText(Path.Combine(_root, "Src", "Sample.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(_root, "Src", "Directory.Build.props"), ValidProps);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Validate_ValidLayout_ReturnsNoErrors()
    {
        ConventionValidator.Validate(new Paths(_root, _settings), _settings).ShouldBeEmpty();
    }

    [Fact]
    public void Validate_MissingSrcFolder_ReportsOnlyThatAndSuggestsSetting()
    {
        _settings.Conventions.SrcDir = "Source";

        var error = ConventionValidator.Validate(new Paths(_root, _settings), _settings).ShouldHaveSingleItem();

        error.ShouldContain("Conventions.SrcDir");
    }

    [Fact]
    public void Validate_FolderCaseMismatch_IsReported()
    {
        _settings.Conventions.SrcDir = "src";

        ConventionValidator.Validate(new Paths(_root, _settings), _settings).ShouldHaveSingleItem();
    }

    [Fact]
    public void Validate_ReportsEveryMissingItem()
    {
        Directory.Delete(Path.Combine(_root, "Src", "Lib"));
        Directory.Delete(Path.Combine(_root, "Src", "Tests"));
        File.Delete(Path.Combine(_root, "GitVersion.yml"));
        File.Delete(Path.Combine(_root, "Src", "Sample.slnx"));
        File.WriteAllText(Path.Combine(_root, "Src", "Directory.Build.props"), "<Project />");

        var errors = ConventionValidator.Validate(new Paths(_root, _settings), _settings);

        errors.Count.ShouldBe(5);
        errors.ShouldContain(e => e.Contains("PackageReleaseNotes"));
    }

    [Fact]
    public void Paths_PreferSlnxAndFallBackToSln()
    {
        File.Delete(Path.Combine(_root, "Src", "Sample.slnx"));
        File.WriteAllText(Path.Combine(_root, "Src", "Sample.sln"), "");

        new Paths(_root, _settings).SolutionFile.ShouldEndWith("Sample.sln");
        ConventionValidator.Validate(new Paths(_root, _settings), _settings).ShouldBeEmpty();
    }

    [Fact]
    public void Paths_ResolveFromConventions()
    {
        _settings.Conventions.ArtifactsDir = "out";

        var paths = new Paths(_root, _settings);

        paths.CommonAssemblyVersionFile.ShouldBe(Path.Combine(_root, "Src", "Common", "AssemblyVersion.cs"));
        paths.CoverageFile.ShouldBe(Path.Combine(_root, "out", "coverage", "coverage.cobertura.xml"));
        paths.PackagesDir.ShouldBe(Path.Combine(_root, "out", "packages"));
    }
}
