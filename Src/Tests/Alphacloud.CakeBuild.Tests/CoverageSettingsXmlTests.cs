using System.Xml.Linq;

namespace Alphacloud.CakeBuild.Tests;

public class CoverageSettingsXmlTests
{
    [Fact]
    public void Create_WritesModuleAttributeAndSourceFilters()
    {
        var settings = new CodeCoverageSettings
        {
            IncludeModulePaths = ["inc1"],
            ExcludeModulePaths = ["exc1", "exc2"],
            ExcludeAttributes = ["attr"],
            ExcludeSources = ["src"],
        };

        var cc = CoverageSettingsXml.Create(settings).Root!.Element("CodeCoverage")!;

        Values(cc, "ModulePaths", "Include", "ModulePath").ShouldBe(["inc1"]);
        Values(cc, "ModulePaths", "Exclude", "ModulePath").ShouldBe(["exc1", "exc2"]);
        Values(cc, "Attributes", "Exclude", "Attribute").ShouldBe(["attr"]);
        Values(cc, "Sources", "Exclude", "Source").ShouldBe(["src"]);
        cc.Element("Attributes")!.Element("Include").ShouldBeNull();
    }

    [Fact]
    public void Create_OmitsEmptySections()
    {
        var settings = new CodeCoverageSettings
        {
            IncludeModulePaths = [], ExcludeModulePaths = [], ExcludeAttributes = [], ExcludeSources = [],
        };

        var cc = CoverageSettingsXml.Create(settings).Root!.Element("CodeCoverage")!;

        cc.Element("ModulePaths").ShouldBeNull();
        cc.Element("Attributes").ShouldBeNull();
        cc.Element("Sources").ShouldBeNull();
    }

    [Fact]
    public void Write_CreatesDirectoryAndFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "coverage", "codecoverage.config");
        try
        {
            CoverageSettingsXml.Write(new CodeCoverageSettings(), path);

            XDocument.Load(path).Root!.Name.LocalName.ShouldBe("Configuration");
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(Path.GetDirectoryName(path))!, recursive: true);
        }
    }

    private static string[] Values(XElement cc, string section, string kind, string item) =>
        cc.Element(section)!.Element(kind)!.Elements(item).Select(e => e.Value).ToArray();
}
