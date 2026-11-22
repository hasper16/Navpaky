using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core;
using Navpaky.Core.Detection;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;

namespace Navpaky.App.ViewModels {
  // What the quick panel shows and changes (design D5 of redesign-visual-identity). It reuses
  // the tray's model for state, Pause and the current app, so the panel and the menu agree.
  internal sealed class QuickPanelViewModel : ObservableObject {
    private readonly SettingsHub _hub;
    private readonly TrayViewModel _tray;
    private readonly StatsTotals _today;

    // The preset the user chose here, to show it as active and resume on a second click.
    private PausePreset? _chosenPause;

    public QuickPanelViewModel(
        SettingsHub hub, TrayViewModel tray, Func<StatsDocument> stats, Func<DateOnly> today) {
      _hub = hub;
      _tray = tray;
      _today = StatsTotals.Of(stats(), today(), 1);
    }

    public TrayState State => _tray.State;

    public string StateText => _tray.ToolTip;

    public string? LanguageCode => _tray.LanguageCode;

    public bool AutoCorrection {
      get {
        return _hub.Current.AutoCorrectionEnabled;
      }

      set {
        if (value != AutoCorrection) {
          _tray.ToggleAutoCorrection();
          Changed();
        }
      }
    }

    public string? CurrentAppName => _tray.CurrentApp?.Name;

    public bool CurrentAppEnabled {
      get {
        return _tray.CurrentApp is not { } app || !_hub.Current.ExcludedApps.Contains(app.Id);
      }

      set {
        if (value != CurrentAppEnabled) {
          _tray.ToggleCurrentApp();
          Changed();
        }
      }
    }

    public Sensitivity Sensitivity {
      get => _hub.Current.Sensitivity;
      set => _hub.Update(settings => settings with { Sensitivity = value });
    }

    public PausePreset? ActivePause {
      get {
        if (!_tray.IsPaused) {
          return null;
        }

        return _hub.Current.PausedUntil == Settings.UntilRestart
            ? PausePreset.UntilRestart
            : _chosenPause;
      }
    }

    public bool PlaySwitchSound {
      get => _hub.Current.PlaySwitchSound;
      set => _hub.Update(settings => settings with { PlaySwitchSound = value });
    }

    public bool ShowCursorIndicator {
      get => _hub.Current.ShowCursorIndicator;
      set => _hub.Update(settings => settings with { ShowCursorIndicator = value });
    }

    public bool ShowStatistics => _hub.Current.StatisticsEnabled;

    public long CorrectionsToday => _today.Corrections;

    public long UndosToday => _today.Undos;

    // The share of today's Corrections kept, in whole percent; none without Corrections.
    public int? KeptPercent => _today.UndoShare is { } undone
        ? (int)Math.Round(Math.Max(0, 1 - undone) * 100)
        : null;

    public string HotkeyText => HotkeyViewModel.Describe(_hub.Current.Hotkey);

    // The Layout after the current one in the installed order, wrapping round.
    public static LayoutId? NextLayout(IReadOnlyList<LayoutId> installed, LayoutId? current) {
      if (installed.Count == 0) {
        return null;
      }

      int index = current is { } layout ? IndexOf(installed, layout) : -1;
      return installed[(index + 1) % installed.Count];
    }

    public void ChoosePause(PausePreset preset) {
      if (ActivePause == preset) {
        _chosenPause = null;
        _tray.Resume();
      } else {
        _chosenPause = preset;
        _tray.Pause(preset);
      }

      Changed();
    }

    private static int IndexOf(IReadOnlyList<LayoutId> installed, LayoutId layout) {
      for (int i = 0; i < installed.Count; i++) {
        if (installed[i] == layout) {
          return i;
        }
      }

      return -1;
    }

    private void Changed() {
      OnPropertyChanged(string.Empty);
    }
  }
}
