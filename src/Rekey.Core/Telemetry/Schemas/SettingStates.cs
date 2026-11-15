namespace Rekey.Core.Telemetry.Schemas {
  /// <summary>
  /// Holds the on/off states of the main settings that the Usage Digest and the setup signal
  /// report.
  /// </summary>
  /// <param name="AutoCorrection">
  /// <see langword="true" /> if Auto Correction is on; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="LaunchAtSignIn">
  /// <see langword="true" /> if Rekey starts at sign-in; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="CursorIndicator">
  /// <see langword="true" /> if the cursor indicator is on; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="SwitchSound">
  /// <see langword="true" /> if the switch sound is on; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="Statistics">
  /// <see langword="true" /> if statistics are on; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="AutoCheckForUpdates">
  /// <see langword="true" /> if Rekey checks for updates on its own; otherwise,
  /// <see langword="false" />.
  /// </param>
  public sealed record SettingStates(
      bool AutoCorrection,
      bool LaunchAtSignIn,
      bool CursorIndicator,
      bool SwitchSound,
      bool Statistics,
      bool AutoCheckForUpdates) {
    /// <summary>
    /// Returns the states of the given settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The states.</returns>
    public static SettingStates Of(Settings settings) {
      return new SettingStates(
          settings.AutoCorrectionEnabled,
          settings.LaunchAtSignIn,
          settings.ShowCursorIndicator,
          settings.PlaySwitchSound,
          settings.StatisticsEnabled,
          settings.AutoCheckForUpdates);
    }
  }
}
