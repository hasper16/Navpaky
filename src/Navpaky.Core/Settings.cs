using Navpaky.Core.Detection;

namespace Navpaky.Core {
  /// <summary>
  /// Holds the user's settings as an immutable snapshot; a change produces a new snapshot.
  /// </summary>
  /// <param name="AutoCorrectionEnabled">
  /// <see langword="true" /> to let Navpaky correct wrong-layout Words on its own; otherwise,
  /// <see langword="false" />. The default is <see langword="true" />.
  /// </param>
  /// <param name="TriggerKeys">
  /// A bitwise combination of the enumeration values that specifies the Trigger Keys. The
  /// default is Space and Enter; Tab is left out because it drives autocomplete and focus moves
  /// (design decision).
  /// </param>
  /// <param name="ExcludedApps">The Excluded Apps. The default is none.</param>
  /// <param name="PausedUntil">
  /// The time the user's Pause ends, or <see langword="null" /> when Auto Correction is not
  /// paused. The default is <see langword="null" />.
  /// </param>
  /// <param name="SkipAfterManualLayoutSwitch">
  /// <see langword="true" /> to leave a Word alone when the user switched Layout themselves
  /// while typing it; otherwise, <see langword="false" />. The default is
  /// <see langword="true" />.
  /// </param>
  /// <param name="Sensitivity">
  /// One of the enumeration values that specifies how much evidence Navpaky needs before
  /// correcting. The default is <see cref="Detection.Sensitivity.Balanced" />.
  /// </param>
  /// <param name="KeyboardWatchdogEnabled">
  /// <see langword="true" /> to let Navpaky notice when Windows stops delivering Keystrokes and
  /// reattach; otherwise, <see langword="false" />. The default is <see langword="true" />.
  /// </param>
  /// <param name="RestartAfterFreezeEnabled">
  /// <see langword="true" /> to let Navpaky restart itself after its input processing froze a
  /// second time within an hour; otherwise, <see langword="false" />. The default is
  /// <see langword="true" />.
  /// </param>
  /// <param name="VerifiedReplacement">
  /// <see langword="true" /> to check that the text before the caret is the Word before
  /// replacing it, where the focused control exposes its text; otherwise,
  /// <see langword="false" />. The default is <see langword="true" />.
  /// </param>
  /// <param name="Hotkey">
  /// The Hotkey that asks for a Manual Correction. The default is
  /// <see cref="Core.Hotkey.DoubleShift" />.
  /// </param>
  /// <param name="ShowCursorIndicator">
  /// <see langword="true" /> to show the new Language next to the caret after Navpaky switched
  /// Layout; otherwise, <see langword="false" />. The default is <see langword="true" />.
  /// </param>
  /// <param name="PlaySwitchSound">
  /// <see langword="true" /> to play <paramref name="SwitchSound" /> after each Correction;
  /// otherwise, <see langword="false" />. The default is <see langword="false" />.
  /// </param>
  /// <param name="SwitchSound">
  /// One of the enumeration values that specifies the switch sound. The default is
  /// <see cref="Core.SwitchSound.Navpaky" />.
  /// </param>
  /// <param name="LaunchAtSignIn">
  /// <see langword="true" /> to start Navpaky when the user signs in to Windows; otherwise,
  /// <see langword="false" />. The default is <see langword="false" />; on start the app reads
  /// the real state from Windows, which the user may change in Task Manager.
  /// </param>
  /// <param name="StatisticsEnabled">
  /// <see langword="true" /> to count Words analysed, Corrections, Undos and Decisions per day;
  /// otherwise, <see langword="false" />, and stored counters are deleted. The default is
  /// <see langword="true" />.
  /// </param>
  /// <param name="AnalysisLogEnabled">
  /// <see langword="true" /> to keep the word-analysis log, which records typed Words;
  /// otherwise, <see langword="false" />, and the log is deleted. The default is
  /// <see langword="false" />.
  /// </param>
  /// <param name="AutoCheckForUpdates">
  /// <see langword="true" /> to check for updates at start and every 24 hours; otherwise,
  /// <see langword="false" />. The default is <see langword="true" />.
  /// </param>
  /// <param name="WelcomeCompleted">
  /// <see langword="true" /> once the user completed or closed the welcome window, so it no
  /// longer opens at start; otherwise, <see langword="false" />. The default is
  /// <see langword="false" />.
  /// </param>
  /// <param name="CorrectionTipShown">
  /// <see langword="true" /> once Navpaky told the user after their first Correction how to undo
  /// it; otherwise, <see langword="false" />. The default is <see langword="false" />.
  /// </param>
  /// <param name="DiagnosticsConsent">
  /// What the user agreed Navpaky may send about crashes and usage. The default agrees to nothing.
  /// </param>
  /// <param name="LastDigestDay">
  /// The local day Navpaky last tried to send a Usage Digest, sent or not, or
  /// <see langword="null" /> when it never tried. The default is <see langword="null" />.
  /// </param>
  /// <param name="SetupSignalSent">
  /// <see langword="true" /> once Navpaky sent the one-time setup signal; otherwise,
  /// <see langword="false" />. The default is <see langword="false" />.
  /// </param>
  /// <param name="UiLanguage">
  /// One of the enumeration values that specifies the language of Navpaky's interface. The default
  /// is <see cref="Core.UiLanguage.System" />.
  /// </param>
  /// <param name="CursorIndicatorStyle">
  /// One of the enumeration values that specifies how the cursor indicator looks. The default is
  /// <see cref="Core.CursorIndicatorStyle.Pill" />.
  /// </param>
  // Each setting is a constructor parameter with a default: since .NET 10, source-generated JSON
  // gives an init-only property that the file omits its type default, not its initializer.
  public sealed record Settings(
      bool AutoCorrectionEnabled = true,
      TriggerKeys TriggerKeys = TriggerKeys.Space | TriggerKeys.Enter,
      ExcludedAppList ExcludedApps = default,
      DateTimeOffset? PausedUntil = null,
      bool SkipAfterManualLayoutSwitch = true,
      Sensitivity Sensitivity = Sensitivity.Balanced,
      bool KeyboardWatchdogEnabled = true,
      bool RestartAfterFreezeEnabled = true,
      bool VerifiedReplacement = true,
      Hotkey Hotkey = default,
      bool ShowCursorIndicator = true,
      bool PlaySwitchSound = false,
      SwitchSound SwitchSound = SwitchSound.Navpaky,
      bool LaunchAtSignIn = false,
      bool StatisticsEnabled = true,
      bool AnalysisLogEnabled = false,
      bool AutoCheckForUpdates = true,
      bool WelcomeCompleted = false,
      bool CorrectionTipShown = false,
      DiagnosticsConsent DiagnosticsConsent = default,
      DateOnly? LastDigestDay = null,
      bool SetupSignalSent = false,
      UiLanguage UiLanguage = UiLanguage.System,
      CursorIndicatorStyle CursorIndicatorStyle = CursorIndicatorStyle.Pill) {
    /// <summary>
    /// The <see cref="PausedUntil" /> value of a Pause that lasts until Navpaky restarts.
    /// </summary>
    public static readonly DateTimeOffset UntilRestart = DateTimeOffset.MaxValue;

    /// <summary>
    /// Gets the schema version of the settings.
    /// </summary>
    /// <value>The schema version. The default is 1.</value>
    public int Version { get; } = 1;

    /// <summary>
    /// Returns the settings a newly started Navpaky uses: a Pause until restart has ended.
    /// </summary>
    /// <returns>The settings for the new session.</returns>
    public Settings ForNewSession() {
      return PausedUntil == UntilRestart ? this with { PausedUntil = null } : this;
    }
  }
}
