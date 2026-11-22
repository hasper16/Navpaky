using Navpaky.App.ViewModels;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;
using Navpaky.Core.Tests;
using Xunit;

namespace Navpaky.App.Tests.ViewModels {
  public sealed class QuickPanelViewModelTests : IDisposable {
    private static readonly DateOnly _today = new(2026, 11, 22);
    private static readonly CurrentApp _telegram =
        new(new AppId(@"C:\Apps\Telegram\Telegram.exe"), "Telegram");

    private static readonly LayoutId _english = new(0x00000409);
    private static readonly LayoutId _ukrainian = new(0x00000422);
    private static readonly LayoutId _russian = new(0x00000419);

    private readonly ManualClock _clock = new();
    private readonly SettingsHub _hub;
    private readonly TrayViewModel _tray;
    private StatsDocument _stats = StatsDocument.Empty;

    public QuickPanelViewModelTests() {
      _hub = new SettingsHub(new Settings(), _ => { }, _clock);
      _tray = new TrayViewModel(_hub, _clock, () => { });
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void TurningAutoCorrectionOffShowsOff() {
      QuickPanelViewModel panel = Panel();

      panel.AutoCorrection = false;

      Assert.False(_hub.Current.AutoCorrectionEnabled);
      Assert.Equal(TrayState.Off, panel.State);
    }

    [Fact]
    public void APresetPausesAndTheSamePresetResumes() {
      QuickPanelViewModel panel = Panel();

      panel.ChoosePause(PausePreset.ThirtyMinutes);
      Assert.Equal(TrayState.Paused, panel.State);
      Assert.Equal(PausePreset.ThirtyMinutes, panel.ActivePause);

      panel.ChoosePause(PausePreset.ThirtyMinutes);
      Assert.Equal(TrayState.Active, panel.State);
      Assert.Null(panel.ActivePause);
    }

    [Fact]
    public void TheCurrentAppCanBeTurnedOff() {
      _tray.CurrentApp = _telegram;
      QuickPanelViewModel panel = Panel();

      Assert.Equal("Telegram", panel.CurrentAppName);
      panel.CurrentAppEnabled = false;

      Assert.True(_hub.Current.ExcludedApps.Contains(_telegram.Id));
      Assert.False(panel.CurrentAppEnabled);
    }

    [Fact]
    public void TodayShowsCorrectionsUndosAndTheShareKept() {
      DayStats day = DayStats.Empty(_today);
      for (int i = 0; i < 4; i++) {
        day = day.Add(StatCounter.AutoCorrections);
      }

      _stats = new StatsDocument([day.Add(StatCounter.Undos)]);
      QuickPanelViewModel panel = Panel();

      Assert.True(panel.ShowStatistics);
      Assert.Equal((4L, 1L, 75), (panel.CorrectionsToday, panel.UndosToday, panel.KeptPercent));
    }

    [Fact]
    public void NoCorrectionsTodayHasNoShare() {
      Assert.Null(Panel().KeptPercent);
    }

    [Fact]
    public void StatisticsOffHidesTheStrip() {
      _hub.Update(settings => settings with { StatisticsEnabled = false });

      Assert.False(Panel().ShowStatistics);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 0)]
    public void NextLayoutFollowsTheInstalledOrder(int current, int next) {
      LayoutId[] installed = [_english, _ukrainian, _russian];

      Assert.Equal(installed[next], QuickPanelViewModel.NextLayout(installed, installed[current]));
    }

    private QuickPanelViewModel Panel() {
      return new QuickPanelViewModel(_hub, _tray, () => _stats, () => _today);
    }
  }
}
