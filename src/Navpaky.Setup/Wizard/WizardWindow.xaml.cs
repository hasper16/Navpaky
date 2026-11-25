using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Navpaky.Setup.Install;
using Navpaky.Ui;
using Wpf.Ui.Appearance;
using ImageIcon = Wpf.Ui.Controls.ImageIcon;

namespace Navpaky.Setup.Wizard {
  // The install wizard's window (spec install-wizard): one page at a time, texts in the chosen
  // language, and no closing while a setup runs.
  internal sealed partial class WizardWindow {
    // A design decision: keeps the Folder page within the window at 1024x768.
    private const int _extrasShown = 8;

    private readonly WizardViewModel _model;

    public WizardWindow(WizardViewModel model) {
      _model = model;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      Mark.Source = BrandMark.Large();
      TitleBar.Icon = new ImageIcon { Source = BrandMark.Large(), Width = 16, Height = 16 };
      Licence.Text = WizardViewModel.LicenceText;
      model.PropertyChanged += OnModelChanged;
      model.Finished += (_, _) => Close();
      Localizer.Follow(this, Refresh);
      Back.Click += (_, _) => model.Back();
      Next.Click += (_, _) => _ = model.Next();
      Install.Click += (_, _) => _ = model.Next();
      Retry.Click += (_, _) => _ = model.Retry();
      Finish.Click += (_, _) => model.Finish();
      CloseWizard.Click += (_, _) => Close();
      FreshCancel.Click += (_, _) => model.CancelFresh();
      FreshDelete.Click += (_, _) => model.ConfirmFresh();
      Browse.Click += (_, _) => PickFolder();
      Loaded += (_, _) => Next.Focus();
    }

    // Brings the window forward when a second start of the wizard asked for it.
    public void BringToFront() {
      if (WindowState == WindowState.Minimized) {
        WindowState = WindowState.Normal;
      }

      Activate();
    }

    protected override void OnClosing(CancelEventArgs e) {
      e.Cancel = _model.Page == WizardPage.Installing && !_model.Failed;
      base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e) {
      _model.PropertyChanged -= OnModelChanged;
      base.OnClosed(e);
    }

    private static string Text(string key, params object[] values) {
      return string.Format(Localizer.Instance.Culture, Localizer.Instance[key], values);
    }

    private static string Size(long bytes) {
      return bytes >= 1_000_000_000
          ? Text("SizeGigabytes", bytes / 1e9)
          : Text("SizeMegabytes", Math.Ceiling(bytes / 1e6));
    }

    private static TextBlock ExtraLine(string text) {
      return new TextBlock {
        Text = text,
        TextTrimming = TextTrimming.CharacterEllipsis,
        Focusable = true,
      };
    }

    private static Visibility Shown(bool shown) {
      return shown ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string? Was(bool? before) {
      if (before is not { } was) {
        return null;
      }

      return Text(was ? "WasOn" : "WasOff");
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e) {
      WizardPage shown = (WizardPage?)Scroller.Tag ?? WizardPage.Language;
      Refresh();
      if (shown != _model.Page) {
        Scroller.ScrollToTop();
        Next.Focus();
      }
    }

    private void Refresh() {
      WizardPage page = _model.Page;
      Scroller.Tag = page;
      LanguagePage.Visibility = Shown(page == WizardPage.Language);
      LicencePage.Visibility = Shown(page == WizardPage.Licence);
      WelcomeBackPage.Visibility = Shown(page == WizardPage.WelcomeBack);
      FolderPage.Visibility = Shown(page == WizardPage.Folder);
      OptionsPage.Visibility = Shown(page == WizardPage.Options);
      InstallingPage.Visibility = Shown(page == WizardPage.Installing);
      DonePage.Visibility = Shown(page == WizardPage.Done);
      ShowSteps();
      ShowButtons(page);
      ShowWelcomeBack();
      ShowFolder();
      string signInHint = string.Join(" ", new[] {
          _model.ForAllUsers ? Text("OptionSignInAllUsers") : null,
          Was(_model.SignInBefore),
      }.OfType<string>());
      SignInRow.Hint = signInHint.Length > 0 ? signInHint : null;
      ShortcutRow.Hint = string.Join(
          " ", new[] { Text("OptionShortcutHint"), Was(_model.ShortcutBefore) }.OfType<string>());
      ShowInstalling();
      DoneNext.Text = Text(_model.Kept is not null && _model.KeepsData ? "DoneKeep" : "DoneNew");
    }

    private void ShowSteps() {
      if (StepBars.Children.Count != _model.StepCount) {
        StepBars.Children.Clear();
        for (int bar = 0; bar < _model.StepCount; bar++) {
          StepBars.Children.Add(new Border { Style = (Style)FindResource("StepBar") });
        }
      }

      for (int bar = 0; bar < StepBars.Children.Count; bar++) {
        ((Border)StepBars.Children[bar]).SetResourceReference(
            Border.BackgroundProperty,
            bar < _model.StepNumber ? "AccentFillColorDefaultBrush" : "BrandLine");
      }

      StepText.Text = Text("SetupStep", _model.StepNumber, _model.StepCount);
    }

    private void ShowButtons(WizardPage page) {
      Back.Visibility = _model.CanGoBack ? Visibility.Visible : Visibility.Hidden;
      Next.Visibility = Shown(page is not (WizardPage.Options or WizardPage.Installing
          or WizardPage.Done));
      Next.IsEnabled = _model.CanGoNext;
      Install.Visibility = Shown(_model.IsInstallStep);
      Finish.Visibility = Shown(page == WizardPage.Done);
      Retry.Visibility = Shown(page == WizardPage.Installing && _model.CanRetry);
      CloseWizard.Visibility = Shown(page == WizardPage.Installing && _model.Failed);
    }

    private void ShowWelcomeBack() {
      DataChoice.Visibility = Shown(!_model.ConfirmingFresh);
      FreshConfirm.Visibility = Shown(_model.ConfirmingFresh);
      if (_model.Kept is not { } kept) {
        return;
      }

      var lines = new List<string>();
      if (kept.Settings is { } settings) {
        lines.Add(Text("KeptHotkey", Text($"Hotkey{settings.Hotkey.Kind}")));
        lines.Add(Text("KeptApps", settings.ExcludedApps.Entries.Count));
      } else if (kept.SettingsUnreadable) {
        lines.Add(Text("KeptNoDetails"));
      }

      if (kept.LearnedWords is { } words) {
        lines.Add(Text("KeptWords", words));
      }

      if (kept.Statistics) {
        lines.Add(Text("KeptStatistics"));
      }

      KeptList.Text = string.Join(Environment.NewLine, lines.Select(line => "• " + line));
    }

    private void ShowFolder() {
      FolderCheck folder = _model.Folder;
      FolderPath.Text = folder.Folder;
      Browse.IsEnabled = !folder.Locked;
      FolderModes.IsEnabled = !_model.ModeLocked;
      FolderModeLocked.Text = Text(_model.ForAllUsers ? "FolderModeAll" : "FolderModeMine");
      FolderModeLocked.Visibility = Shown(_model.ModeLocked);
      FolderHint.Visibility = Shown(!_model.ForAllUsers);
      FolderQuiet.Visibility = Shown(!_model.ForAllUsers);
      FolderAllUsers.Visibility = Shown(_model.ForAllUsers);

      // All users: the folder is fixed by the mode, not by an earlier install.
      FolderLocked.Visibility = Shown(_model.ModeLocked);
      FolderRule.Visibility = Shown(!folder.Locked);
      FolderProblem.Text = _model.FolderProblem is { } problem ? Text(problem) : string.Empty;
      FolderProblem.Visibility = Shown(_model.FolderProblem is not null);
      FolderSpace.Text = Text("FolderSpace", Size(_model.NeededBytes), Size(_model.FreeBytes));
      ShowExtras(folder.Extras);
    }

    // The entries a reinstall would delete, bounded so the page keeps its layout (design D3 of
    // warn-reinstall-extra-files).
    private void ShowExtras(IReadOnlyList<string> extras) {
      FolderExtras.Visibility = Shown(extras.Count > 0);
      FolderExtrasList.Children.Clear();
      foreach (string name in extras.Take(_extrasShown)) {
        FolderExtrasList.Children.Add(ExtraLine(name));
      }

      if (extras.Count > _extrasShown) {
        int more = extras.Count - _extrasShown;
        FolderExtrasList.Children.Add(ExtraLine(Text("FolderExtrasMore", more)));
      }
    }

    private void ShowInstalling() {
      PhaseText.Text = Text($"Phase{_model.Phase}");
      InstallingHint.Visibility = Shown(!_model.Failed);
      PhaseText.Visibility = Shown(!_model.Failed);
      Progress.Visibility = Shown(!_model.Failed);
      FailureText.Text = Text(_model.MayStillRun ? "InstallMayStillRun" : "InstallFailed");
      FailureText.Visibility = Shown(_model.Failed);
      LogText.Text = _model.LogPath is { } log ? Text("InstallLog", log) : string.Empty;
      LogText.Visibility = Shown(_model.Failed && _model.LogPath is not null);
    }

    private void PickFolder() {
      var dialog = new OpenFolderDialog { Title = Text("FolderTitle") };
      if (dialog.ShowDialog(this) == true) {
        _model.Pick(dialog.FolderName);
      }
    }
  }
}
