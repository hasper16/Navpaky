using System.Text.Json;
using Navpaky.Core.Detection;
using Xunit;

namespace Navpaky.Core.Tests {
  public sealed class SettingsTests {
    [Fact]
    public void SettingsFromBeforeTheWizardLeaveTheInstallFieldsUnknown() {
      const string fromVersion04 = """
          { "version": 1, "welcomeCompleted": true, "launchAtSignIn": true, "uiLanguage": "uk" }
          """;

      Settings? settings = JsonSerializer.Deserialize(
          fromVersion04, SettingsJsonContext.Default.Settings);

      Assert.NotNull(settings);
      Assert.Null(settings.InstallFolder);
      Assert.Null(settings.DesktopShortcut);
      Assert.False(settings.InstallQuestionsAnswered);
      Assert.True(settings.WelcomeCompleted);
    }

    [Fact]
    public void InstallFieldsRoundTripThroughJson() {
      var settings = new Settings(
          InstallFolder: @"D:\Apps\Navpaky",
          DesktopShortcut: true,
          InstallQuestionsAnswered: true);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(settings, restored);
    }

    [Fact]
    public void SensitivityDefaultsToBalanced() {
      Assert.Equal(Sensitivity.Balanced, new Settings().Sensitivity);
    }

    [Fact]
    public void ChangedSensitivityRoundTripsThroughJson() {
      var settings = new Settings(Sensitivity: Sensitivity.Aggressive);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(settings, restored);
    }

    [Fact]
    public void JsonNamesTheSensitivity() {
      var settings = new Settings(Sensitivity: Sensitivity.Cautious);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);

      using JsonDocument document = JsonDocument.Parse(json);
      Assert.Equal("cautious", document.RootElement.GetProperty("sensitivity").GetString());
    }

    [Fact]
    public void FileWithoutSensitivityGetsBalanced() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": false }", SettingsJsonContext.Default.Settings);

      Assert.Equal(Sensitivity.Balanced, restored?.Sensitivity);
    }

    [Fact]
    public void DefaultsRoundTripThroughJsonUnchanged() {
      var defaults = new Settings();

      string json = JsonSerializer.Serialize(defaults, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(defaults, restored);
    }

    [Fact]
    public void VerifiedReplacementDefaultsToOn() {
      Assert.True(new Settings().VerifiedReplacement);
    }

    [Fact]
    public void FileWithoutVerifiedReplacementGetsItOn() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": true }", SettingsJsonContext.Default.Settings);

      Assert.True(restored?.VerifiedReplacement);
    }

    [Fact]
    public void WatchdogsDefaultToOn() {
      var defaults = new Settings();

      Assert.True(defaults.KeyboardWatchdogEnabled);
      Assert.True(defaults.RestartAfterFreezeEnabled);
    }

    [Fact]
    public void TurnedOffWatchdogsRoundTripThroughJson() {
      var settings = new Settings(KeyboardWatchdogEnabled: false, RestartAfterFreezeEnabled: false);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(settings, restored);
    }

    [Fact]
    public void FileWithoutWatchdogSettingsTurnsThemOn() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": false }", SettingsJsonContext.Default.Settings);

      Assert.True(restored?.KeyboardWatchdogEnabled);
      Assert.True(restored?.RestartAfterFreezeEnabled);
    }

    [Fact]
    public void FeedbackSettingsHaveTheirDefaults() {
      var defaults = new Settings();

      Assert.True(defaults.ShowCursorIndicator);
      Assert.False(defaults.PlaySwitchSound);
      Assert.Equal(SwitchSound.Navpaky, defaults.SwitchSound);
      Assert.Equal(CursorIndicatorStyle.Pill, defaults.CursorIndicatorStyle);
      Assert.False(defaults.LaunchAtSignIn);
    }

    [Fact]
    public void ChangedFeedbackSettingsRoundTripThroughJson() {
      var settings = new Settings(
          ShowCursorIndicator: false,
          PlaySwitchSound: true,
          SwitchSound: SwitchSound.Chime,
          LaunchAtSignIn: true);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.Equal(settings, restored);
    }

    [Fact]
    public void JsonNamesTheSwitchSound() {
      var settings = new Settings(SwitchSound: SwitchSound.SoftTick);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);

      using JsonDocument document = JsonDocument.Parse(json);
      Assert.Equal("softTick", document.RootElement.GetProperty("switchSound").GetString());
    }

    [Fact]
    public void FileWithoutFeedbackSettingsGetsTheDefaults() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": false }", SettingsJsonContext.Default.Settings);

      Assert.True(restored?.ShowCursorIndicator);
      Assert.False(restored?.PlaySwitchSound);
      Assert.Equal(SwitchSound.Navpaky, restored?.SwitchSound);
      Assert.Equal(CursorIndicatorStyle.Pill, restored?.CursorIndicatorStyle);
      Assert.False(restored?.LaunchAtSignIn);
    }

    [Fact]
    public void SoundChosenBeforeTheRedesignStaysChosen() {
      Settings? restored = JsonSerializer.Deserialize(
          "{ \"switchSound\": \"chime\" }", SettingsJsonContext.Default.Settings);

      Assert.Equal(SwitchSound.Chime, restored?.SwitchSound);
    }

    [Theory]
    [InlineData(CursorIndicatorStyle.Tag, "tag")]
    [InlineData(CursorIndicatorStyle.Underline, "underline")]
    public void CursorIndicatorStyleRoundTripsByName(CursorIndicatorStyle style, string name) {
      var settings = new Settings(CursorIndicatorStyle: style);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      using JsonDocument document = JsonDocument.Parse(json);
      Assert.Equal(name, document.RootElement.GetProperty("cursorIndicatorStyle").GetString());
      Assert.Equal(style, restored?.CursorIndicatorStyle);
    }

    [Fact]
    public void PauseUntilRestartEndsWithTheSession() {
      var paused = new Settings(PausedUntil: Settings.UntilRestart);

      Assert.Null(paused.ForNewSession().PausedUntil);
    }

    [Fact]
    public void TimedPauseSurvivesANewSession() {
      DateTimeOffset until = new(2026, 11, 14, 12, 30, 0, TimeSpan.Zero);
      var paused = new Settings(PausedUntil: until);

      Assert.Equal(until, paused.ForNewSession().PausedUntil);
    }

    [Fact]
    public void JsonCarriesSchemaVersionOne() {
      string json = JsonSerializer.Serialize(new Settings(), SettingsJsonContext.Default.Settings);

      using JsonDocument document = JsonDocument.Parse(json);
      Assert.Equal(1, document.RootElement.GetProperty("version").GetInt32());
    }

    [Fact]
    public void StatisticsAreOnAndTheAnalysisLogOffByDefault() {
      var defaults = new Settings();

      Assert.True(defaults.StatisticsEnabled);
      Assert.False(defaults.AnalysisLogEnabled);
    }

    [Fact]
    public void FileWithoutDiagnosticsSettingsGetsTheDefaults() {
      Settings? settings = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": true }", SettingsJsonContext.Default.Settings);

      Assert.True(settings!.StatisticsEnabled);
      Assert.False(settings.AnalysisLogEnabled);
    }

    [Fact]
    public void ChangedDiagnosticsSettingsRoundTripThroughJson() {
      var settings = new Settings(StatisticsEnabled: false, AnalysisLogEnabled: true);

      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.False(restored!.StatisticsEnabled);
      Assert.True(restored.AnalysisLogEnabled);
    }

    [Fact]
    public void FileWithoutUpdateSettingGetsAutomaticChecksOn() {
      Settings? settings = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": true }", SettingsJsonContext.Default.Settings);

      Assert.True(settings!.AutoCheckForUpdates);
    }

    [Fact]
    public void AutomaticChecksTurnedOffRoundTripThroughJson() {
      string json = JsonSerializer.Serialize(
          new Settings(AutoCheckForUpdates: false), SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.False(restored!.AutoCheckForUpdates);
    }

    [Fact]
    public void OnboardingIsNotDoneByDefault() {
      var defaults = new Settings();

      Assert.False(defaults.WelcomeCompleted);
      Assert.False(defaults.CorrectionTipShown);
    }

    [Fact]
    public void FileWithoutOnboardingSettingsGetsTheDefaults() {
      Settings? settings = JsonSerializer.Deserialize(
          "{ \"autoCorrectionEnabled\": true }", SettingsJsonContext.Default.Settings);

      Assert.False(settings!.WelcomeCompleted);
      Assert.False(settings.CorrectionTipShown);
    }

    [Fact]
    public void FinishedOnboardingRoundTripsThroughJson() {
      string json = JsonSerializer.Serialize(
          new Settings(WelcomeCompleted: true, CorrectionTipShown: true),
          SettingsJsonContext.Default.Settings);
      Settings? restored = JsonSerializer.Deserialize(json, SettingsJsonContext.Default.Settings);

      Assert.True(restored!.WelcomeCompleted);
      Assert.True(restored.CorrectionTipShown);
    }
  }
}
