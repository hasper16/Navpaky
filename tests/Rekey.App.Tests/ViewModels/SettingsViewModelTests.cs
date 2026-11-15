using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Detection;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  public sealed class SettingsViewModelTests : IDisposable {
    private readonly ManualClock _clock = new();
    private readonly List<Settings> _saved = [];
    private readonly List<Settings> _published = [];
    private readonly SettingsHub _hub;
    private readonly SettingsViewModel _model;

    public SettingsViewModelTests() {
      _hub = new SettingsHub(new Settings(), _saved.Add, _clock);
      _hub.Changed += _published.Add;
      _model = Create();
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void ChangingAPropertyPublishesANewSnapshot() {
      _model.Sensitivity = Sensitivity.Cautious;

      Settings published = Assert.Single(_published);
      Assert.Equal(Sensitivity.Cautious, published.Sensitivity);
      Assert.Same(published, _hub.Current);
    }

    [Fact]
    public void DiagnosticsSwitchesChangeTheSettings() {
      _model.StatisticsEnabled = false;
      _model.AnalysisLogEnabled = true;

      Assert.False(_hub.Current.StatisticsEnabled);
      Assert.True(_hub.Current.AnalysisLogEnabled);
    }

    [Fact]
    public void SettingTheSameValueAgainPublishesNothing() {
      _model.AutoCorrectionEnabled = true;

      Assert.Empty(_published);
    }

    [Fact]
    public void RapidChangesAreSavedOnceAfterTheyStop() {
      _model.AutoCorrectionEnabled = false;
      _clock.Advance(TimeSpan.FromMilliseconds(300));
      _model.PlaySwitchSound = true;
      _clock.Advance(TimeSpan.FromMilliseconds(300));
      _model.TriggerOnTab = true;

      _clock.Advance(TimeSpan.FromMilliseconds(499));
      Assert.Empty(_saved);

      _clock.Advance(TimeSpan.FromMilliseconds(1));
      Settings saved = Assert.Single(_saved);
      Assert.Equal(
          new Settings(
              AutoCorrectionEnabled: false,
              TriggerKeys: TriggerKeys.Space | TriggerKeys.Enter | TriggerKeys.Tab,
              PlaySwitchSound: true),
          saved);
    }

    [Fact]
    public void FlushSavesAPendingChangeAtOnce() {
      _model.ShowCursorIndicator = false;

      _hub.Flush();

      Assert.False(Assert.Single(_saved).ShowCursorIndicator);
      _clock.Advance(TimeSpan.FromSeconds(1));
      Assert.Single(_saved);
    }

    [Fact]
    public void TurningATriggerKeyOffKeepsTheOthers() {
      _model.TriggerOnEnter = false;

      Assert.Equal(TriggerKeys.Space, _hub.Current.TriggerKeys);
      Assert.True(_model.TriggerOnSpace);
      Assert.False(_model.TriggerOnEnter);
    }

    [Fact]
    public void AChangeFromElsewhereIsAnnouncedToTheView() {
      var announced = new List<string?>();
      _model.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

      _hub.Update(settings => settings with { AutoCorrectionEnabled = false });

      Assert.False(_model.AutoCorrectionEnabled);
      Assert.Contains(string.Empty, announced);
    }

    [Fact]
    public void ADisposedModelNoLongerFollowsTheHub() {
      using SettingsViewModel model = Create();
      var announced = new List<string?>();
      model.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

      model.Dispose();
      _hub.Update(settings => settings with { AutoCorrectionEnabled = false });

      Assert.Empty(announced);
    }

    private SettingsViewModel Create() {
      return new SettingsViewModel(_hub);
    }
  }
}
