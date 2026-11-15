using System.ComponentModel;
using System.Windows;
using Rekey.App.Onboarding;
using Rekey.App.Tray;
using Rekey.App.ViewModels;
using Wpf.Ui.Appearance;

namespace Rekey.App.Views {
  // The welcome window (design D1 of add-onboarding). The try-it field is an ordinary text box:
  // Rekey's hook sees it like any other app's, so a Correction there proves the whole pipeline.
  internal sealed partial class WelcomeWindow {
    private readonly WelcomeViewModel _model;

    public WelcomeWindow(
        WelcomeViewModel model, HotkeyPage hotkey, bool dark, Action openLanguageSettings) {
      _model = model;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      Mark.Source = Keycap.Render("EN", TrayState.Active, dark, 64);
      Hotkey.Content = hotkey;
      ShowLayouts();
      model.PropertyChanged += OnModelChanged;
      LanguageSettings.Click += (_, _) => openLanguageSettings();
      Done.Click += (_, _) => Close();
      Loaded += (_, _) => {
        TitleBarButtons.Name(TitleBar);
        TryIt.Focus();
      };
    }

    // Whether the user is typing in the try-it field right now.
    public bool TryItHasFocus => IsActive && TryIt.IsKeyboardFocused;

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
