using System.Xml.Linq;

namespace Alphacloud.CakeBuild;

/// <summary>Checks that a repository follows the folder and naming conventions.</summary>
public static class ConventionValidator
{
    /// <summary>XPath-like location of the release notes element updated on tagged builds.</summary>
    public const string ReleaseNotesLocation = "/Project/PropertyGroup[@Label=\"Package\"]/PackageReleaseNotes";

    /// <summary>Returns a list of convention violations; empty when the repository is valid.</summary>
    public static IReadOnlyList<string> Validate(Paths paths, ProjectSettings settings)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(settings);

        var errors = new List<string>();
        var c = settings.Conventions;

        if (!DirectoryExistsExact(paths.SrcDir))
        {
            errors.Add($"Source folder '{c.SrcDir}' not found at '{paths.SrcDir}'. Set ProjectSettings.Conventions.SrcDir if it is named differently (names are case-sensitive on Linux).");
            return errors;
        }

        if (!File.Exists(paths.SolutionFile))
            errors.Add($"Solution '{settings.SolutionName}.slnx' or '{settings.SolutionName}.sln' not found in '{paths.SrcDir}'.");

        if (!DirectoryExistsExact(paths.LibDir))
            errors.Add($"Library folder '{c.SrcDir}/{c.LibDir}' not found. Set ProjectSettings.Conventions.LibDir if it is named differently.");

        if (!DirectoryExistsExact(paths.TestsDir))
            errors.Add($"Tests folder '{c.SrcDir}/{c.TestsDir}' not found. Set ProjectSettings.Conventions.TestsDir if it is named differently.");

        if (!File.Exists(paths.GitVersionConfigFile))
            errors.Add($"'GitVersion.yml' not found in repository root '{paths.RootDir}'.");

        if (!File.Exists(paths.BuildPropsFile))
            errors.Add($"'Directory.Build.props' not found in '{paths.SrcDir}'.");
        else if (!HasReleaseNotesElement(paths.BuildPropsFile))
            errors.Add($"'{paths.BuildPropsFile}' must contain <PropertyGroup Label=\"Package\"> with a <PackageReleaseNotes> element.");

        return errors;
    }

    /// <summary>Checks for <c>PropertyGroup[@Label='Package']/PackageReleaseNotes</c>.</summary>
    public static bool HasReleaseNotesElement(string buildPropsFile)
    {
        var doc = XDocument.Load(buildPropsFile);
        return doc.Root?
            .Elements().Where(e => e.Name.LocalName == "PropertyGroup" && (string?)e.Attribute("Label") == "Package")
            .Elements().Any(e => e.Name.LocalName == "PackageReleaseNotes") ?? false;
    }

    /// <summary>
    /// Directory existence check that also compares the final segment's case, so a convention mismatch is
    /// reported on Windows/macOS too, instead of only breaking on Linux CI.
    /// </summary>
    internal static bool DirectoryExistsExact(string path)
    {
        if (!Directory.Exists(path))
            return false;
        var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var parent = Path.GetDirectoryName(full);
        if (parent is null)
            return true;
        var name = Path.GetFileName(full);
        return Directory.EnumerateDirectories(parent).Any(d => string.Equals(Path.GetFileName(d), name, StringComparison.Ordinal));
    }
}
