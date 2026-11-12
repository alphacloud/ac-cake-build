namespace SampleSolution.Calculator.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_ReturnsSum() => Assert.Equal(5, Calculator.Add(2, 3));

    [Fact]
    public void Divide_ByZero_Throws() => Assert.Throws<DivideByZeroException>(() => Calculator.Divide(1, 0));

    [Fact]
    public void Framework_IsReported() => Assert.StartsWith("net", Calculator.Framework());

    [Fact]
    [Trait("Category", "ManualTests")]
    public void ManualTest_IsExcludedByBuild() =>
        Assert.Fail("Category=ManualTests must be filtered out by Alphacloud.CakeBuild.");
}
