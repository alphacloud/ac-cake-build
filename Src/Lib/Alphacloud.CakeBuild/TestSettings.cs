namespace Alphacloud.CakeBuild;

/// <summary>Test execution options (xunit.v3 on Microsoft.Testing.Platform).</summary>
public class TestSettings
{
    /// <summary>Trait name used to categorize tests. Default: <c>Category</c>.</summary>
    public string CategoryTrait { get; set; } = "Category";

    /// <summary>Values of <see cref="CategoryTrait"/> excluded from the run. Default: <c>ManualTests</c>.</summary>
    public List<string> ExcludedCategories { get; set; } = ["ManualTests"];

    /// <summary>
    /// After the Debug run with coverage, run tests again against the Release build (without coverage) when the
    /// build configuration is Release. Default: <c>true</c>.
    /// </summary>
    public bool RunReleaseTests { get; set; } = true;

    /// <summary>Run test modules sequentially (<c>--max-parallel-test-modules 1</c>). Default: <c>true</c>.</summary>
    public bool SequentialTestModules { get; set; } = true;
}
