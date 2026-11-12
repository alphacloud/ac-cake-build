namespace Alphacloud.CakeBuild;

/// <summary>
/// Code coverage filters, translated into a dotnet-coverage settings file.
/// All entries are .NET regular expressions, matched as documented at
/// https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage#settings.
/// </summary>
public class CodeCoverageSettings
{
    /// <summary>
    /// Module (assembly file path) regexes to include. Default (set by <see cref="ProjectSettings"/>):
    /// assemblies whose file name starts with the solution name.
    /// </summary>
    public List<string> IncludeModulePaths { get; set; } = [];

    /// <summary>Module regexes to exclude. Default: test assemblies.</summary>
    public List<string> ExcludeModulePaths { get; set; } = [@".*Tests?\.dll$", @".*Tests\..*\.dll$"];

    /// <summary>Full attribute name regexes; members marked with them are excluded.</summary>
    public List<string> ExcludeAttributes { get; set; } =
    [
        @"^System\.Diagnostics\.CodeAnalysis\.ExcludeFromCodeCoverageAttribute$",
        @"^System\.CodeDom\.Compiler\.GeneratedCodeAttribute$",
        @"^System\.Diagnostics\.DebuggerNonUserCodeAttribute$",
    ];

    /// <summary>Source file path regexes to exclude.</summary>
    public List<string> ExcludeSources { get; set; } = [@".*Designer\.cs$", @".*[\\/]obj[\\/].*"];
}
