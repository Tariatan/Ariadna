using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ariadna.Tests;

[TestClass]
public sealed class HeadlessPolicyTests
{
    [TestMethod]
    public void TestSources_DesktopInteraction_IsForbidden()
    {
        // Arrange: embedded sources also catch a window shown and closed within one test.
        var assembly = typeof(HeadlessPolicyTests).Assembly;
        var resources = assembly.GetManifestResourceNames().Where(name => name.EndsWith(".cs", StringComparison.Ordinal)).ToArray();
        var forbidden = new Regex(@"\.\s*(Show|ShowDialog|Activate|ActivateCatalog|Focus|ConfirmRemoval|ShowInformation|ShowError)\s*\(|\b(Clipboard|Process|Mouse)\s*\.\s*(GetText|GetDataObject|SetText|SetDataObject|Clear|Start|Capture)\s*\(");

        // Act / Assert
        Assert.IsNotEmpty(resources, "The test project must embed its sources for the headless policy check.");
        foreach (var name in resources)
        {
            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new System.IO.StreamReader(stream);
            Assert.IsFalse(forbidden.IsMatch(reader.ReadToEnd()), $"Desktop interaction is forbidden in {name}. Use detached layout or a model/action contract.");
        }
    }
}
