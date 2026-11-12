using System.Xml.Linq;

namespace Alphacloud.CakeBuild;

/// <summary>Generates the dotnet-coverage XML settings file (<c>-s</c>) from <see cref="CodeCoverageSettings"/>.</summary>
public static class CoverageSettingsXml
{
    /// <summary>Builds the settings document.</summary>
    public static XDocument Create(CodeCoverageSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var codeCoverage = new XElement("CodeCoverage",
            Section("ModulePaths", "ModulePath", settings.IncludeModulePaths, settings.ExcludeModulePaths),
            Section("Attributes", "Attribute", [], settings.ExcludeAttributes),
            Section("Sources", "Source", [], settings.ExcludeSources),
            new XElement("EnableStaticManagedInstrumentation", "True"),
            new XElement("EnableDynamicManagedInstrumentation", "True"));

        return new XDocument(new XElement("Configuration", codeCoverage));
    }

    /// <summary>Writes the settings document to <paramref name="path"/>.</summary>
    public static void Write(CodeCoverageSettings settings, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        Create(settings).Save(path);
    }

    private static XElement? Section(string name, string item, IReadOnlyCollection<string> include, IReadOnlyCollection<string> exclude)
    {
        if (include.Count == 0 && exclude.Count == 0)
            return null;

        var section = new XElement(name);
        if (include.Count > 0)
            section.Add(new XElement("Include", include.Select(v => new XElement(item, v))));
        if (exclude.Count > 0)
            section.Add(new XElement("Exclude", exclude.Select(v => new XElement(item, v))));
        return section;
    }
}
