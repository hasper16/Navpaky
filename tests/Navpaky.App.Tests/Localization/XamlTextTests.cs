using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Navpaky.App.ViewModels;
using Navpaky.Core.Languages;
using Xunit;

namespace Navpaky.App.Tests.Localization {
  // XAML shows text only through {l:Loc Key}, so every visible string follows the interface
  // language; the product name is the one literal.
  public sealed class XamlTextTests {
    private static readonly HashSet<string> _textAttributes = [
        "Text", "Content", "Header", "Title", "ToolTip", "ToolTipText", "PlaceholderText",
        "AutomationProperties.Name", "AutomationProperties.HelpText"];

    [Fact]
    public void NoLiteralOrStaticTextRemains() {
      List<string> found = [.. Attributes()
          .Where(attribute => attribute.Value.Contains("Strings.", StringComparison.Ordinal)
              || (_textAttributes.Contains(attribute.Name.LocalName)
                  && attribute.Value != Product.Name
                  && !attribute.Value.StartsWith("{l:Loc ", StringComparison.Ordinal)
                  && !attribute.Value.StartsWith("{Binding", StringComparison.Ordinal)
                  && !attribute.Value.StartsWith("{TemplateBinding", StringComparison.Ordinal)))
          .Select(Describe)];

      Assert.Empty(found);
    }

    [Fact]
    public void EveryLocKeyExists() {
      List<string> missing = [.. Attributes()
          .Where(attribute => attribute.Value.StartsWith("{l:Loc ", StringComparison.Ordinal))
          .Where(attribute => Strings.ResourceManager.GetString(
              attribute.Value[7..^1], CultureInfo.InvariantCulture) is null)
          .Select(Describe)];

      Assert.Empty(missing);
    }

    [Fact]
    public void EveryWordLanguageHasAName() {
      List<string> missing = [.. Enum.GetValues<Language>()
          .Select(language => new LearnedEntry("слово", language).LanguageKey)
          .Where(key => Strings.ResourceManager.GetString(
              key, CultureInfo.InvariantCulture) is null)];

      Assert.Empty(missing);
    }

    private static IEnumerable<XAttribute> Attributes() {
      string app = Path.Join(RepositoryRoot(), "src", "Navpaky.App");
      string obj = Path.Join(app, "obj");
      return Directory.EnumerateFiles(app, "*.xaml", SearchOption.AllDirectories)
          .Where(file => !file.StartsWith(obj, StringComparison.OrdinalIgnoreCase))
          .SelectMany(file => XDocument.Load(file, LoadOptions.SetBaseUri).Descendants())
          .SelectMany(element => element.Attributes());
    }

    private static string Describe(XAttribute attribute) {
      return $"{Path.GetFileName(attribute.BaseUri)} {attribute.Parent!.Name.LocalName}"
          + $".{attribute.Name.LocalName}={attribute.Value}";
    }

    private static string RepositoryRoot() {
      DirectoryInfo? folder = new(AppContext.BaseDirectory);
      while (folder is not null && !File.Exists(Path.Join(folder.FullName, "Navpaky.slnx"))) {
        folder = folder.Parent;
      }

      return folder?.FullName ?? throw new InvalidOperationException("No Navpaky.slnx above.");
    }
  }
}
