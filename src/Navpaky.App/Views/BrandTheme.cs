using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The brand palette follows the app theme: red and black in light, blue and gold in dark
  // (design D3). WPF UI resets the accent to the system one on every theme change, so the
  // brand colours are applied again after it, into the same resources WPF UI uses.
  internal static class BrandTheme {
    public static void Follow() {
      Apply(ApplicationThemeManager.GetAppTheme());
      ApplicationThemeManager.Changed += (theme, _) => Apply(theme);
    }

    public static Color Accent(bool dark) {
      return dark ? Color.FromRgb(0x4C, 0x86, 0xF0) : Color.FromRgb(0xC8, 0x10, 0x2E);
    }

    public static Color Secondary(bool dark) {
      return dark ? Color.FromRgb(0xFA, 0xCC, 0x15) : Color.FromRgb(0x17, 0x17, 0x1C);
    }

    public static void Apply(ApplicationTheme theme) {
      bool dark = theme == ApplicationTheme.Dark;
      ApplicationAccentColorManager.Apply(Accent(dark), theme);
      var second = new SolidColorBrush(Secondary(dark));
      second.Freeze();
      UiApplication.Current.Resources["BrandSecondaryBrush"] = second;
    }
  }
}
