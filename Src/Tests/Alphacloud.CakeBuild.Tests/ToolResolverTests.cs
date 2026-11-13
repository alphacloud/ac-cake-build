namespace Alphacloud.CakeBuild.Tests;

public class ToolResolverTests
{
    [Theory]
    [InlineData("dotnet-coverage", true, "dotnet-coverage.exe")]
    [InlineData("dotnet-coverage", false, "dotnet-coverage")]
    [InlineData("coveralls-windows.exe", true, "coveralls-windows.exe")]
    [InlineData("tool.EXE", true, "tool.EXE")]
    public void ExecutableName_AppendsExeOnWindowsOnly(string name, bool isWindows, string expected)
    {
        ToolResolver.ExecutableName(name, isWindows).ShouldBe(expected);
    }

    [Fact]
    public void ExecutableName_DefaultsToCurrentPlatform()
    {
        ToolResolver.ExecutableName("x").ShouldBe(OperatingSystem.IsWindows() ? "x.exe" : "x");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ExecutableName_RejectsEmptyName(string name)
    {
        Should.Throw<ArgumentException>(() => ToolResolver.ExecutableName(name));
    }
}
