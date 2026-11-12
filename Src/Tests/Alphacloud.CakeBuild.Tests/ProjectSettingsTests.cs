using System.Text.RegularExpressions;

namespace Alphacloud.CakeBuild.Tests;

public class ProjectSettingsTests
{
    [Theory]
    [InlineData("", "repo", "sln")]
    [InlineData("owner", " ", "sln")]
    [InlineData("owner", "repo", "")]
    public void Constructor_RejectsEmptyValues(string owner, string repo, string solution)
    {
        Should.Throw<ArgumentException>(() => new ProjectSettings(owner, repo, solution));
    }

    [Fact]
    public void Defaults_FollowConventions()
    {
        var settings = new ProjectSettings("owner", "repo", "My.Lib");

        settings.Conventions.SrcDir.ShouldBe("Src");
        settings.Conventions.LibDir.ShouldBe("Lib");
        settings.Conventions.TestsDir.ShouldBe("Tests");
        settings.Conventions.SamplesDir.ShouldBe("Samples");
        settings.Conventions.CommonDir.ShouldBe("Common");
        settings.Conventions.ArtifactsDir.ShouldBe("artifacts");
        settings.Tests.RunReleaseTests.ShouldBeTrue();
        settings.Tests.ExcludedCategories.ShouldBe(["ManualTests"]);
        settings.ReleaseTargetBranch.ShouldBe("master");
    }

    [Theory]
    [InlineData(@"C:\repo\Src\Lib\My.Lib\bin\Debug\net10.0\My.Lib.dll", true)]
    [InlineData("/repo/Src/Lib/My.Lib.Extensions/bin/Debug/net10.0/My.Lib.Extensions.dll", true)]
    [InlineData(@"C:\repo\bin\MyXLib.dll", false)]
    [InlineData(@"C:\repo\bin\Other.dll", false)]
    public void DefaultIncludeModulePath_MatchesSolutionAssemblies(string modulePath, bool expected)
    {
        var settings = new ProjectSettings("owner", "repo", "My.Lib");

        var pattern = settings.CodeCoverage.IncludeModulePaths.ShouldHaveSingleItem();
        Regex.IsMatch(modulePath, pattern, RegexOptions.IgnoreCase).ShouldBe(expected);
    }

    [Theory]
    [InlineData("My.Lib.Tests.dll", true)]
    [InlineData("My.Lib.Tests.Integration.dll", true)]
    [InlineData("My.Lib.dll", false)]
    public void DefaultExcludeModulePaths_MatchTestAssemblies(string modulePath, bool expected)
    {
        var settings = new ProjectSettings("owner", "repo", "My.Lib");

        settings.CodeCoverage.ExcludeModulePaths.Any(p => Regex.IsMatch(modulePath, p)).ShouldBe(expected);
    }
}
