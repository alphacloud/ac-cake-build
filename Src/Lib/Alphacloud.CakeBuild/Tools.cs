namespace Alphacloud.CakeBuild;

/// <summary>Pinned external tools installed by <see cref="CakeHostExtensions.UseAlphacloudCakeBuild"/>.</summary>
public static class Tools
{
    /// <summary>GitVersion (dotnet tool).</summary>
    public static readonly Uri GitVersion = new("dotnet:?package=GitVersion.Tool&version=6.8.2");

    /// <summary>GitReleaseManager (dotnet tool).</summary>
    public static readonly Uri GitReleaseManager = new("dotnet:?package=GitReleaseManager.Tool&version=0.20.0");

    /// <summary>dotnet-coverage (dotnet tool).</summary>
    public static readonly Uri DotNetCoverage = new("dotnet:?package=dotnet-coverage&version=18.12.0");

    /// <summary>ReportGenerator (NuGet tool).</summary>
    public static readonly Uri ReportGenerator = new("nuget:?package=ReportGenerator&version=5.6.0");

    /// <summary>All tools.</summary>
    public static IReadOnlyList<Uri> All { get; } = [GitVersion, GitReleaseManager, DotNetCoverage, ReportGenerator];
}
