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
    private static readonly Color[] _light = [
        Rgb(0xC8102E), Rgb(0xB00E28), Rgb(0x8F0B20), Rgb(0x6E0818)];

    private static readonly Color[] _dark = [
        Rgb(0x4C86F0), Rgb(0x6B9CF7), Rgb(0x8AB2FA), Rgb(0xA9C7FC)];

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
      var second = new SolidColorBrush(Secondary(dark));
      second.Freeze();
      UiApplication.Current.Resources["BrandSecondaryBrush"] = second;
    }

    private static Color Rgb(int rgb) {
      return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }
  }
}
