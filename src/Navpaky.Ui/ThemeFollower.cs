using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Appearance;

namespace Navpaky.Ui {
  // Follows the Windows light/dark setting while Navpaky runs (design D1 of
  // fix-redesign-findings). WPF UI follows it only through open, watched windows, and a tray app
  // usually has none, so a change would wait until the next window and look inverted.
  internal sealed class ThemeFollower : IDisposable {
    private const string _personalizeKey =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private readonly System.Windows.Threading.Dispatcher _dispatcher;
    private readonly Action _changed;

    public ThemeFollower(System.Windows.Threading.Dispatcher dispatcher, Action changed) {
      _dispatcher = dispatcher;
      _changed = changed;
      SystemEvents.UserPreferenceChanged += OnPreferenceChanged;
      ApplicationThemeManager.Changed += OnThemeApplied;
      Refresh(SystemAppTheme());
    }

    // Applies the given Windows theme to the app and the brand accent to match.
    public static void Refresh(ApplicationTheme systemTheme) {
      if (systemTheme != ApplicationThemeManager.GetAppTheme()) {
        ApplicationThemeManager.Apply(systemTheme, updateAccent: false);
      }

      BrandTheme.Apply(systemTheme);
    }

    // The app theme for Windows' "apps mode" setting (AppsUseLightTheme; missing means light) or
    // high contrast. WPF UI reads the desktop theme file first, which can disagree with apps mode.
    public static ApplicationTheme ThemeFor(bool highContrast, int? appsUseLightTheme) {
      if (highContrast) {
        return ApplicationTheme.HighContrast;
      }

      return appsUseLightTheme == 0 ? ApplicationTheme.Dark : ApplicationTheme.Light;
    }

    // The app theme for the Windows setting now.
    public static ApplicationTheme SystemAppTheme() {
      using RegistryKey? personalize = Registry.CurrentUser.OpenSubKey(_personalizeKey);
      return ThemeFor(
          SystemParameters.HighContrast, personalize?.GetValue("AppsUseLightTheme") as int?);
    }

    public void Dispose() {
      SystemEvents.UserPreferenceChanged -= OnPreferenceChanged;
      ApplicationThemeManager.Changed -= OnThemeApplied;
    }

    // WPF UI picks the theme itself when a watched window opens; this keeps it on apps mode.
    private void OnThemeApplied(ApplicationTheme applied, System.Windows.Media.Color accent) {
      ApplicationTheme wanted = SystemAppTheme();
      if (applied != wanted) {
        _dispatcher.BeginInvoke(() => Refresh(wanted));
      }
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
