using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core;
using Navpaky.Core.Detection;
using Navpaky.Core.Statistics;

namespace Navpaky.App.ViewModels {
  // What the quick panel shows and changes (design D5 of redesign-visual-identity). It reuses
  // the tray's model for state, Pause and the current app, so the panel and the menu agree.
  internal sealed class QuickPanelViewModel : ObservableObject {
    private readonly SettingsHub _hub;
    private readonly TrayViewModel _tray;
    private readonly StatsTotals _today;

    public QuickPanelViewModel(
        SettingsHub hub, TrayViewModel tray, Func<StatsDocument> stats, Func<DateOnly> today) {
      _hub = hub;
      _tray = tray;
      _today = StatsTotals.Of(stats(), today(), 1);
    }

    public TrayState State => _tray.State;

    public string StateText => _tray.ToolTip;

    public string? LanguageCode => _tray.LanguageCode;

    // The language as its speakers write it, such as "Українська", or the code when unknown.
    public string LanguageName {
      get {
        try {
          string name = CultureInfo.GetCultureInfo(LanguageCode ?? string.Empty).NativeName;
          return name.Length == 0
              ? LanguageCode ?? string.Empty
              : char.ToUpper(name[0], CultureInfo.CurrentCulture) + name[1..];
        } catch (CultureNotFoundException) {
          return LanguageCode ?? string.Empty;
        }
      }
    }

    public string CurrentAppText => CurrentAppName is { } name
        ? UiText.Format(Strings.QuickCurrentApp, name)
        : string.Empty;

    public string HotkeyHint =>
        UiText.Format(Strings.QuickHotkeyHint, HotkeyViewModel.Describe(_hub.Current.Hotkey));

    public string KeptText => KeptPercent is { } kept
        ? kept.ToString("0", CultureInfo.CurrentCulture) + " %"
        : "—";

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

    public bool HasCurrentApp => _tray.CurrentApp is not null;

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

    public Sensitivity Sensitivity => _hub.Current.Sensitivity;

    public PausePreset? ActivePause {
      get {
        if (!_tray.IsPaused) {
          return null;
        }

        return _hub.Current.PausedUntil == Settings.UntilRestart
            ? PausePreset.UntilRestart
            : _tray.ChosenPause;
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

    // Shows a change made outside the panel, such as the app's new Layout.
    public void Refresh() {
      Changed();
    }

    public void ChooseSensitivity(Sensitivity sensitivity) {
      _hub.Update(settings => settings with { Sensitivity = sensitivity });
      Changed();
    }

    public void ChoosePause(PausePreset preset) {
      if (ActivePause == preset) {
        _tray.ChosenPause = null;
        _tray.Resume();
      } else {
        _tray.ChosenPause = preset;
        _tray.Pause(preset);
      }

      Changed();
    }

    private void Changed() {
      OnPropertyChanged(string.Empty);
    }
  }
}
