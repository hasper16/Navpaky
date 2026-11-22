using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Navpaky.App.ViewModels;
using Navpaky.Win32;

namespace Navpaky.App.Tray {
  // The quick panel above the tray (design D5 of redesign-visual-identity). It remembers the
  // window the user typed in, so the Layout key can hand the focus back and switch that
  // window's Layout. It closes on Esc and when it loses the focus.
  internal sealed partial class QuickPanel {
    private readonly QuickPanelViewModel _model;
    private readonly nint _typedIn;
    private bool _closing;

    public QuickPanel(QuickPanelViewModel model, QuickPanelActions actions) {
      _model = model;
      _typedIn = ScreenFacts.Foreground();
      InitializeComponent();
      DataContext = model;
      LayoutKey.Click += (_, _) => {
        CloseNow();
        ScreenFacts.Activate(_typedIn);
        actions.SwitchLayout();
      };
      Pause15.Click += (_, _) => Pause(PausePreset.FifteenMinutes, actions);
      Pause30.Click += (_, _) => Pause(PausePreset.ThirtyMinutes, actions);
      Pause60.Click += (_, _) => Pause(PausePreset.OneHour, actions);
      Pause180.Click += (_, _) => Pause(PausePreset.ThreeHours, actions);
      PauseRestart.Click += (_, _) => Pause(PausePreset.UntilRestart, actions);
      Stats.Click += (_, _) => Then(actions.OpenStatistics);
      Settings.Click += (_, _) => Then(actions.OpenSettings);
      FirstSteps.Click += (_, _) => Then(actions.OpenWelcome);
      Quit.Click += (_, _) => Then(actions.Quit);
      model.PropertyChanged += (_, _) => actions.Changed();
      Deactivated += (_, _) => CloseNow();
      PreviewKeyDown += (_, e) => {
        if (e.Key == Key.Escape) {
          CloseNow();
        }
      };
    }

    // Opens the panel above the tray, in the work area of the screen under the pointer.
    public void ShowAtPointer() {
      Show();
      double scale = VisualTreeHelper.GetDpi(this).DpiScaleX;
      (int x, int y) = ScreenFacts.Mouse();
      (int left, int top, int right, int bottom) = ScreenFacts.WorkAreaAt(x, y);
      double width = ActualWidth * scale;
      double height = ActualHeight * scale;
      double atX = Math.Clamp(x - (width / 2), left, right - width);

      // A taskbar at the top puts the panel under it; otherwise the panel sits above the tray.
      double atY = y <= top ? top : bottom - height;
      Left = atX / scale;
      Top = atY / scale;
      Activate();
    }

    private void Pause(PausePreset preset, QuickPanelActions actions) {
      _model.ChoosePause(preset);
      actions.Changed();
    }

    private void Then(Action action) {
      CloseNow();
      action();
    }

    private void CloseNow() {
      if (!_closing) {
        _closing = true;
        Close();
      }
    }
  }
}
