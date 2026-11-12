namespace Alphacloud.CakeBuild.Tests;

public class BuildVersionTests
{
    [Fact]
    public void ForPullRequest_SynthesizesZeroPrBuildVersion()
    {
        var version = BuildVersion.ForPullRequest(42, 117, "abc123", "Fix things");

        version.NuGet.ShouldBe("0.42.117");
        version.Milestone.ShouldBe("0.42.117");
        version.Informational.ShouldBe("0.42.117");
        version.NextMajor.ShouldBe("0.42.117.0");
        version.CommitHash.ShouldBe("abc123");
        version.Full.ShouldBe("0.42.117/abc123-PR-Fix things");
    }

    [Fact]
    public void ForPullRequest_ToleratesMissingCommitHash()
    {
        BuildVersion.ForPullRequest(1, 2, null, null).CommitHash.ShouldBe(string.Empty);
    }
}
