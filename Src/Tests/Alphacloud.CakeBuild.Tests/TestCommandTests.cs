namespace Alphacloud.CakeBuild.Tests;

public class TestCommandTests
{
    [Fact]
    public void Arguments_UseMicrosoftTestingPlatformSwitches()
    {
        var args = TestCommand.Arguments("Src/S.slnx", "Debug", "artifacts/test-results", new TestSettings());

        args.ShouldBe(
        [
            "test", "--solution", "Src/S.slnx", "--configuration", "Debug", "--no-build", "--no-restore",
            "--results-directory", "artifacts/test-results", "--report-xunit-trx", "--ignore-exit-code", "8",
            "--max-parallel-test-modules", "1", "--filter-not-trait", "Category=ManualTests",
        ]);
    }

    [Fact]
    public void Arguments_EmitOneFilterPerExcludedCategoryAndHonourSettings()
    {
        var settings = new TestSettings
        {
            CategoryTrait = "Kind",
            ExcludedCategories = ["Slow", "", "Integration"],
            SequentialTestModules = false,
        };

        var args = TestCommand.Arguments("S.sln", "Release", "out", settings);

        args.ShouldNotContain("--max-parallel-test-modules");
        args.Count(a => a == "--filter-not-trait").ShouldBe(2);
        args.ShouldContain("Kind=Slow");
        args.ShouldContain("Kind=Integration");
    }

    [Fact]
    public void ToCommandLine_QuotesValuesWithSpaces()
    {
        TestCommand.ToCommandLine("dotnet", ["test", "--solution", @"C:\My Repo\S.slnx"])
            .ShouldBe("dotnet test --solution \"C:\\My Repo\\S.slnx\"");
    }
}
