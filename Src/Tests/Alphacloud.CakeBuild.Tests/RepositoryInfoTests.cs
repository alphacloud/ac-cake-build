namespace Alphacloud.CakeBuild.Tests;

public class RepositoryInfoTests
{
    private static readonly ProjectSettings Settings = new("alphacloud", "Sample-Repo", "Sample");

    [Theory]
    [InlineData("release/1.2.0", true)]
    [InlineData("releases/1.2.0", true)]
    [InlineData("hotfix/1.2.1", true)]
    [InlineData("Hotfixes/1.2.1", true)]
    [InlineData("develop", false)]
    [InlineData("feature/release-notes", false)]
    [InlineData("feature/release/x", false)]
    [InlineData("bugfix/hotfixes/y", false)]
    [InlineData("user/releases/1.2.0", false)]
    [InlineData("release", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsReleaseBranch_DetectsGitFlowReleaseAndHotfixBranches(string? branch, bool expected)
    {
        RepositoryInfo.Create(Settings, null, branch, false, false).IsReleaseBranch.ShouldBe(expected);
    }

    [Theory]
    [InlineData("develop", true)]
    [InlineData("DEVELOP", true)]
    [InlineData("development", false)]
    public void IsDevelopBranch_MatchesDevelopOnly(string branch, bool expected)
    {
        RepositoryInfo.Create(Settings, null, branch, false, false).IsDevelopBranch.ShouldBe(expected);
    }

    [Theory]
    [InlineData("alphacloud/Sample-Repo", true)]
    [InlineData("ALPHACLOUD/sample-repo", true)]
    [InlineData("fork-owner/Sample-Repo", false)]
    [InlineData(null, false)]
    public void IsMain_IsTrueOnlyForUpstreamRepository(string? repositoryName, bool expected)
    {
        RepositoryInfo.Create(Settings, repositoryName, "master", false, false).IsMain.ShouldBe(expected);
    }

    [Fact]
    public void Create_PassesThroughPullRequestAndTagFlags()
    {
        var info = RepositoryInfo.Create(Settings, null, "master", isPullRequest: true, isTagged: true);

        info.IsPullRequest.ShouldBeTrue();
        info.IsTagged.ShouldBeTrue();
        info.Branch.ShouldBe("master");
    }
}
