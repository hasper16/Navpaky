using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using Navpaky.App.Localization;
using Navpaky.App.Onboarding;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The welcome window (design D1 of add-onboarding). The try-it field is an ordinary text box:
  // Navpaky's hook sees it like any other app's, so a Correction there proves the whole pipeline.
  internal sealed partial class WelcomeWindow {
    private readonly WelcomeViewModel _model;

    // Written on the UI thread only, read on the worker thread when a Switch happens.
    private volatile bool _tryItFocused;
    private nint _handle;

    public WelcomeWindow(
        WelcomeViewModel model, HotkeyPage hotkey, bool dark, Action openLanguageSettings) {
      _model = model;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      Mark.Source = Keycap.Render("EN", TrayState.Active, dark, 64);
      Hotkey.Content = hotkey;
      OuterWheelScroll.Attach(Scroller);
      Localizer.Follow(this, ShowLayouts);
      model.PropertyChanged += OnModelChanged;
      LanguageSettings.Click += (_, _) => openLanguageSettings();
      Done.Click += (_, _) => {
        model.NoteDone();
        Close();
      };
      TryIt.GotKeyboardFocus += (_, _) => _tryItFocused = true;
      TryIt.LostKeyboardFocus += (_, _) => _tryItFocused = false;
      SourceInitialized += (_, _) => Interlocked.Exchange(
          ref _handle, new WindowInteropHelper(this).Handle);
      Loaded += (_, _) => {
        TitleBarButtons.Name(TitleBar);
        TryIt.Focus();
      };
    }

    // Whether typing goes to the try-it field while the given window is in front; safe to call
    // from any thread, so the worker can ask when the Switch happens.
    public bool IsTryItIn(nint foreground) {
      return foreground != 0 && foreground == Volatile.Read(ref _handle) && _tryItFocused;
    }

    protected override void OnClosed(EventArgs e) {
      _model.PropertyChanged -= OnModelChanged;
      base.OnClosed(e);
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof(WelcomeViewModel.LayoutNames)) {
        ShowLayouts();
      }
    }

    private void ShowLayouts() {
      TryItHint.Text = UiText.Format(Strings.WelcomeTryIt, _model.LayoutNames);
    }
  }
}
