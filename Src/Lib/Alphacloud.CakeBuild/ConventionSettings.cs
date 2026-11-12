namespace Alphacloud.CakeBuild;

/// <summary>
/// Folder and naming conventions of a consumer repository. All paths are relative to the repository root
/// (or to <see cref="SrcDir"/> for the source sub-folders). Folder names are case-sensitive on Linux.
/// </summary>
public class ConventionSettings
{
    /// <summary>Source root folder, relative to the repository root. Default: <c>Src</c>.</summary>
    public string SrcDir { get; set; } = "Src";

    /// <summary>Packable library projects folder, relative to <see cref="SrcDir"/>. Default: <c>Lib</c>.</summary>
    public string LibDir { get; set; } = "Lib";

    /// <summary>Test projects folder, relative to <see cref="SrcDir"/>. Default: <c>Tests</c>.</summary>
    public string TestsDir { get; set; } = "Tests";

    /// <summary>Optional samples folder, relative to <see cref="SrcDir"/>. Default: <c>Samples</c>.</summary>
    public string SamplesDir { get; set; } = "Samples";

    /// <summary>Shared files folder (generated <c>AssemblyVersion.cs</c>), relative to <see cref="SrcDir"/>. Default: <c>Common</c>.</summary>
    public string CommonDir { get; set; } = "Common";

    /// <summary>Build output folder, relative to the repository root. Default: <c>artifacts</c>.</summary>
    public string ArtifactsDir { get; set; } = "artifacts";
}
