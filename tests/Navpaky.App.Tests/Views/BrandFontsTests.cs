using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Navpaky.App.Tests.Views {
  [Collection(nameof(Sta))]
  public sealed class BrandFontsTests {
    [Theory]
    [InlineData("ContentControlThemeFontFamily", "Inter")]
    [InlineData("HeadingFontFamily", "Grunt Grotesk")]
    public void ResourceLoadsTheShippedFont(string key, string family) {
      List<string> names = [];
      Sta.Run(() => {
        if (Application.Current is null) {
          _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        }

        var resources = new ResourceDictionary {
          Source = new Uri("pack://application:,,,/Navpaky.App;component/Resources.xaml"),
        };
        var font = (FontFamily)resources[key];
        foreach (Typeface face in font.GetTypefaces()) {
          if (face.TryGetGlyphTypeface(out GlyphTypeface glyphs)) {
            names.Add(glyphs.Win32FamilyNames[CultureInfo.GetCultureInfo("en-US")]);
          }
        }
      });

      Assert.Contains(names, name => name.StartsWith(family, StringComparison.Ordinal));
    }
  }
}
