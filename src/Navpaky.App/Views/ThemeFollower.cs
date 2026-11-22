using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // Follows the Windows light/dark setting while Navpaky runs (design D1 of
  // fix-redesign-findings). WPF UI follows it only through open, watched windows, and a tray app
  // usually has none, so a change would wait until the next window and look inverted.
  internal sealed class ThemeFollower : IDisposable {
    private readonly System.Windows.Threading.Dispatcher _dispatcher;
    private readonly Action _changed;

    public ThemeFollower(System.Windows.Threading.Dispatcher dispatcher, Action changed) {
      _dispatcher = dispatcher;
      _changed = changed;
      SystemEvents.UserPreferenceChanged += OnPreferenceChanged;
    }

    // Applies the given Windows theme to the app and the brand accent to match.
    public static void Refresh(ApplicationTheme systemTheme) {
      if (systemTheme != ApplicationThemeManager.GetAppTheme()) {
        ApplicationThemeManager.Apply(systemTheme, updateAccent: false);
      }

      BrandTheme.Apply(systemTheme);
    }

    public void Dispose() {
      SystemEvents.UserPreferenceChanged -= OnPreferenceChanged;
    }

    // The app theme WPF UI would pick for the current Windows setting.
    private static ApplicationTheme SystemAppTheme() {
      SystemThemeManager.UpdateSystemThemeCache();
      return ApplicationThemeManager.GetSystemTheme() switch {
        SystemTheme.Dark or SystemTheme.CapturedMotion or SystemTheme.Glow => ApplicationTheme.Dark,
        SystemTheme.HC1 or SystemTheme.HC2 or SystemTheme.HCBlack or SystemTheme.HCWhite =>
            ApplicationTheme.HighContrast,
        _ => ApplicationTheme.Light,
      };
    }

    private void OnPreferenceChanged(object sender, UserPreferenceChangedEventArgs e) {
      if (e.Category is UserPreferenceCategory.General or UserPreferenceCategory.Color
          or UserPreferenceCategory.VisualStyle) {
        _dispatcher.BeginInvoke(() => {
          Refresh(SystemAppTheme());
          _changed();
        });
      }
    }
  }
}
