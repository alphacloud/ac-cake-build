namespace Alphacloud.CakeBuild;

/// <summary>Builds <c>dotnet test</c> arguments for Microsoft.Testing.Platform mode (xunit.v3).</summary>
public static class TestCommand
{
    /// <summary>MTP exit code returned when a filter leaves a test module with zero tests.</summary>
    public const int ZeroTestsExitCode = 8;

    /// <summary>Creates the argument list (without the leading <c>dotnet</c>).</summary>
    public static IReadOnlyList<string> Arguments(string solutionFile, string configuration, string resultsDirectory, TestSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solutionFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(resultsDirectory);
        ArgumentNullException.ThrowIfNull(settings);

        var args = new List<string>
        {
            "test",
            "--solution", solutionFile,
            "--configuration", configuration,
            "--no-build",
            "--no-restore",
            "--results-directory", resultsDirectory,
            "--report-xunit-trx",
            "--ignore-exit-code", ZeroTestsExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };

        if (settings.SequentialTestModules)
            args.AddRange(["--max-parallel-test-modules", "1"]);

        foreach (var category in settings.ExcludedCategories.Where(c => !string.IsNullOrWhiteSpace(c)))
            args.AddRange(["--filter-not-trait", $"{settings.CategoryTrait}={category}"]);

        return args;
    }

    /// <summary>Joins arguments into one command line, quoting values that contain spaces.</summary>
    public static string ToCommandLine(string executable, IEnumerable<string> arguments) =>
        string.Join(' ', new[] { executable }.Concat(arguments).Select(Quote));

    private static string Quote(string value) =>
        value.Length > 0 && !value.Contains(' ') && !value.Contains('"') ? value : $"\"{value.Replace("\"", "\\\"")}\"";
}
