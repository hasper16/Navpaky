using System.Windows;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The brand palette follows the app theme: red and black in light, blue and gold in dark
  // (design D3 of redesign-visual-identity). WPF UI resets the accent to the system one on every
  // theme change, so the brand colours are applied again after it, into the same resources WPF UI
  // uses. The shades are fixed (design D2 of fix-redesign-findings): WPF UI's own derivation
  // turns the blue pale in the dark theme.
  internal static class BrandTheme {
    // Brushes WPF UI paints on the accent; the prototype keeps them white in both themes.
    private static readonly string[] _onAccent = [
        "TextOnAccentFillColorPrimaryBrush", "AccentButtonForeground",
        "AccentButtonForegroundPointerOver", "AccentButtonForegroundPressed",
        "ToggleSwitchKnobFillOn", "ToggleSwitchKnobFillOnPointerOver",
        "ToggleSwitchKnobFillOnPressed"];

    private static readonly Color[] _light = [
        Rgb(0xC8102E), Rgb(0xB00E28), Rgb(0x8F0B20), Rgb(0x6E0818)];

    // WPF UI fills controls with the secondary shade in the dark theme: dark enough for white
    // text at 4.5:1.
    private static readonly Color[] _dark = [
        Rgb(0x4C86F0), Rgb(0x6B9CF7), Rgb(0x366FD6), Rgb(0x2F63C8)];

    public static void Follow() {
      Apply(ApplicationThemeManager.GetAppTheme());
      ApplicationThemeManager.Changed += (theme, _) => Apply(theme);
    }

    public static Color Accent(bool dark) {
      return dark ? _dark[0] : _light[0];
    }

    public static Color Secondary(bool dark) {
      return dark ? Rgb(0xFACC15) : Rgb(0x17171C);
    }

    public static void Apply(ApplicationTheme theme) {
      bool dark = theme == ApplicationTheme.Dark;
      Color[] shades = dark ? _dark : _light;
      ApplicationAccentColorManager.Apply(shades[0], shades[1], shades[2], shades[3]);
      Set("BrandSecondaryBrush", Secondary(dark));
      if (theme == ApplicationTheme.HighContrast) {
        foreach (string key in _onAccent) {
          UiApplication.Current.Resources.Remove(key);
        }

        UseSystemSurfaces();
        return;
      }

      foreach (string key in _onAccent) {
        Set(key, Colors.White);
      }

      // The prototype's surfaces: navy in dark, white and blue-grey in light. Hint and caption
      // text reaches 4.5:1 on both.
      Set("BrandWindowBackground", dark ? Argb(0xFF131B2F) : Argb(0xFFFFFFFF));
      Set("BrandSurface", dark ? Argb(0xFF18213A) : Argb(0xFFF5F7FB));
      Set("BrandLine", dark ? Argb(0x2496AAE6) : Argb(0x1F12283A));
      Set("BrandTextSecondary", dark ? Argb(0xB8D2DAF2) : Argb(0xFF4A5468));
      Set("BrandTextTertiary", dark ? Argb(0xB4AFBADE) : Argb(0xFF5F6B80));
    }

    // A high-contrast scheme picks its own colours; the brand surfaces would hide its text.
    private static void UseSystemSurfaces() {
      Set("BrandWindowBackground", SystemColors.WindowColor);
      Set("BrandSurface", SystemColors.WindowColor);
      Set("BrandLine", SystemColors.WindowTextColor);
      Set("BrandTextSecondary", SystemColors.WindowTextColor);
      Set("BrandTextTertiary", SystemColors.WindowTextColor);
    }

    private static void Set(string key, Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      UiApplication.Current.Resources[key] = brush;
    }

    private static Color Argb(uint argb) {
      return Color.FromArgb(
          (byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
    }

    private static Color Rgb(int rgb) {
      return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }
  }
}
