using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Navpaky.App.ViewModels;
using Navpaky.Core.Detection;
using Navpaky.Win32;

namespace Navpaky.App.Tray {
  // The quick panel above the tray (design D5 of redesign-visual-identity). Its Layout key
  // switches the Layout of the app the user typed in and the panel stays open (design D4 of
  // fix-redesign-findings). It closes on Esc and when it loses the focus.
  internal sealed partial class QuickPanel {
    private readonly QuickPanelViewModel _model;
    private bool _closing;

    public QuickPanel(QuickPanelViewModel model, QuickPanelActions actions) {
      _model = model;
      InitializeComponent();
      DataContext = model;

      // The panel stays open; the app's Layout and the Language shown change in place.
      LayoutKey.Click += (_, _) => actions.SwitchLayout();

      // One-way bindings with a click each, so a click on the active choice keeps it shown.
      SensCautious.Click += (_, _) => model.ChooseSensitivity(Sensitivity.Cautious);
      SensBalanced.Click += (_, _) => model.ChooseSensitivity(Sensitivity.Balanced);
      SensAggressive.Click += (_, _) => model.ChooseSensitivity(Sensitivity.Aggressive);
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

      // Centred on the pointer, kept inside the work area; a work area narrower than the panel
      // gets it at its left edge.
      double atX = Math.Max(left, Math.Min(x - (width / 2), right - width));

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
