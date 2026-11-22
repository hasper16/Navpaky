using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;
using Navpaky.App.Localization;
using Navpaky.App.Onboarding;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;
using Wpf.Ui.Appearance;
using ImageIcon = Wpf.Ui.Controls.ImageIcon;

namespace Navpaky.App.Views {
  // The welcome window (design D1 of add-onboarding). The try-it field is an ordinary text box:
  // Navpaky's hook sees it like any other app's, so a Correction there proves the whole pipeline.
  internal sealed partial class WelcomeWindow {
    private readonly WelcomeViewModel _model;
    private readonly HotkeyViewModel _hotkey;
    private readonly DispatcherTimer _demo = new() { Interval = TimeSpan.FromSeconds(1.6) };

    // Written on the UI thread only, read on the worker thread when a Switch happens.
    private volatile bool _tryItFocused;
    private nint _handle;

    public WelcomeWindow(
        WelcomeViewModel model, HotkeyViewModel hotkey, Action openLanguageSettings) {
      _model = model;
      _hotkey = hotkey;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      Mark.Source = BrandMark.Large();
      TitleBar.Icon = new ImageIcon { Source = BrandMark.Large(), Width = 16, Height = 16 };
      _demo.Tick += (_, _) => FlipDemo();
      ShowDemo(corrected: false);

      // Key names are the same in every language.
      UndoPlus.Text = "+";
      UndoKey.Text = "Z";
      HotkeyShift.Click += (_, _) => hotkey.Choose(Core.Hotkey.DoubleShift);
      HotkeyCtrl.Click += (_, _) => hotkey.Choose(Core.Hotkey.DoubleCtrl);
      hotkey.PropertyChanged += OnHotkeyChanged;
      ShowHotkey();
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
      Next.Click += (_, _) => model.Next();
      Back.Click += (_, _) => model.Back();
      Localizer.Follow(this, ShowStep);
      Localizer.Follow(this, ShowHotkeyKeys);
      Loaded += (_, _) => {
        TitleBarButtons.Name(TitleBar);
        Next.Focus();
      };
    }

    // Whether typing goes to the try-it field while the given window is in front; safe to call
    // from any thread, so the worker can ask when the Switch happens.
    public bool IsTryItIn(nint foreground) {
      return foreground != 0 && foreground == Volatile.Read(ref _handle) && _tryItFocused;
    }

    protected override void OnClosed(EventArgs e) {
      _demo.Stop();
      _model.PropertyChanged -= OnModelChanged;
      _hotkey.PropertyChanged -= OnHotkeyChanged;
      base.OnClosed(e);
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e) {
      if (e.PropertyName == nameof(WelcomeViewModel.LayoutNames)) {
        ShowLayouts();
      } else if (e.PropertyName == nameof(WelcomeViewModel.Step)) {
        ShowStep();
        Scroller.ScrollToTop();
        if (_model.Step == 1) {
          TryIt.Focus();
        }
      }
    }

    private void OnHotkeyChanged(object? sender, PropertyChangedEventArgs e) {
      ShowHotkey();
    }

    // Neither chip is lit when another Hotkey was chosen in Settings.
    private void ShowHotkey() {
      HotkeyShift.IsChecked = _hotkey.Current == Core.Hotkey.DoubleShift;
      HotkeyCtrl.IsChecked = _hotkey.Current == Core.Hotkey.DoubleCtrl;
    }

    private void ShowHotkeyKeys() {
      HotkeyChip.Show(HotkeyShift, Core.Hotkey.DoubleShift);
      HotkeyChip.Show(HotkeyCtrl, Core.Hotkey.DoubleCtrl);
    }

    private void ShowLayouts() {
      TryItHint.Text = UiText.Format(Strings.WelcomeTryIt, _model.LayoutNames);
    }

    private void ShowStep() {
      int step = _model.Step;
      Step0.Visibility = step == 0 ? Visibility.Visible : Visibility.Collapsed;
      Step1.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
      Step2.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
      Back.Visibility = _model.CanGoBack ? Visibility.Visible : Visibility.Hidden;
      Next.Visibility = _model.IsLastStep ? Visibility.Collapsed : Visibility.Visible;
      Done.Visibility = _model.IsLastStep ? Visibility.Visible : Visibility.Collapsed;
      StepText.Text = UiText.Format(
          Strings.WelcomeStep, step + 1, WelcomeViewModel.LastStep + 1);
      for (int bar = 0; bar < StepBars.Children.Count; bar++) {
        var segment = (Border)StepBars.Children[bar];
        segment.SetResourceReference(
            Border.BackgroundProperty,
            bar <= step ? "AccentFillColorDefaultBrush" : "BrandLine");
      }

      // The example on step 1 turns the wrong-layout word into the right one, as in the
      // prototype; without animations it shows the result.
      if (step == 0 && SystemParameters.ClientAreaAnimation) {
        _demo.Start();
      } else {
        _demo.Stop();
        ShowDemo(corrected: true);
      }
    }

    private void FlipDemo() {
      ShowDemo(corrected: DemoKey.Text != "UK");
    }

    private void ShowDemo(bool corrected) {
      DemoWord.Text = corrected ? "привіт" : "ghbdsn";
      DemoKey.Text = corrected ? "UK" : "EN";
    }
  }
}
