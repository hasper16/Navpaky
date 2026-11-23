using System.Globalization;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Navpaky.App.Views {
  // Inter for all text and Grunt Grotesk for headings (design D3 of fix-redesign-findings).
  // WPF UI reads no font resource of its own, so the text font is set on every window, menu and
  // tooltip; their controls inherit it. A font a window sets itself stays.
  internal static class BrandFonts {
    private const string _fallback = ", Segoe UI Variable Text, Segoe UI";
    private static bool _applied;

    public static FontFamily Text => Shipped("Inter");

    public static FontFamily Heading => Shipped("Grunt Grotesk");

    public static void Apply() {
      if (_applied) {
        return;
      }

      _applied = true;

      // A default value does not flow down to child elements, a set one does: so the font is
      // set on each window, menu and tooltip as it appears.
      EventManager.RegisterClassHandler(
          typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(UseText));
      EventManager.RegisterClassHandler(
          typeof(ContextMenu), ContextMenu.OpenedEvent, new RoutedEventHandler(UseText));
      EventManager.RegisterClassHandler(
          typeof(ToolTip), ToolTip.OpenedEvent, new RoutedEventHandler(UseText));
    }

    // Whether both shipped fonts load from the app's resources; `--check-fonts` runs it on the
    // published single-file build in CI.
    public static bool Check() {
      return Loads(Text, "Inter") && Loads(Heading, "Grunt Grotesk");
    }

    private static void UseText(object sender, RoutedEventArgs e) {
      if (sender is Control control
          && DependencyPropertyHelper.GetValueSource(control, Control.FontFamilyProperty)
              .BaseValueSource < BaseValueSource.Local) {
        control.FontFamily = Text;
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1075:URIs should not be hardcoded",
        Justification = "The pack URI of the app's own embedded fonts never changes.")]
    private static FontFamily Shipped(string family) {
      // Reading the scheme registers pack: URIs before an Application exists.
      _ = PackUriHelper.UriSchemePack;
      return new FontFamily(
          new Uri("pack://application:,,,/Navpaky.App;component/"),
          $"./Fonts/#{family}{_fallback}");
    }

    private static bool Loads(FontFamily font, string family) {
      CultureInfo english = CultureInfo.GetCultureInfo("en-US");
      return font.GetTypefaces().Any(face =>
          face.TryGetGlyphTypeface(out GlyphTypeface glyphs)
          && glyphs.Win32FamilyNames[english].StartsWith(family, StringComparison.Ordinal));
    }
  }
}
