namespace Navpaky.Core {
  /// <summary>
  /// Specifies the kind of an Incident.
  /// </summary>
  /// <remarks>
  /// Later changes add a value for each Incident they record.
  /// </remarks>
  public enum IncidentType {
    /// <summary>
    /// A Language Profile could not be read or failed validation, so its Language is unavailable.
    /// Details: the <see cref="Languages.Language" /> skipped.
    /// </summary>
    LanguageProfileSkipped,

    /// <summary>
    /// A keyboard or mouse hook callback took longer than 1 ms at least once since the last such
    /// Incident; Windows removes a hook that is too slow. The time includes any wait for the CPU,
    /// as while the system captures the screen, because Windows counts that time too. No
    /// details.
    /// </summary>
    SlowHookCallback,

    /// <summary>
    /// A Layout Switch Navpaky requested did not take effect within 500 ms; Navpaky adopted the
    /// window's real Layout. No details.
    /// </summary>
    SwitchNotLanded,

    /// <summary>
    /// Windows stopped delivering input to Navpaky's hooks while the user typed; Navpaky removed
    /// and installed them again. No details.
    /// </summary>
    HookReinstalled,

    /// <summary>
    /// Keyboard observation failed twice within 10 minutes; Navpaky paused Auto Correction for
    /// 10 minutes. No details.
    /// </summary>
    SelfPaused,

    /// <summary>
    /// Navpaky's input processing stopped for more than 5 seconds with input waiting, and Navpaky
    /// did not restart for it. No details.
    /// </summary>
    InputFrozen,

    /// <summary>
    /// Navpaky's input processing froze a second time within an hour; Navpaky restarted itself.
    /// No details.
    /// </summary>
    FreezeRestart,

    /// <summary>
    /// The text before the caret differed from the Word Navpaky was about to replace, so the
    /// Correction or Undo was cancelled and the text left as it was. No details.
    /// </summary>
    CorrectionCancelled,

    /// <summary>
    /// Navpaky held typed keys back for more than 150 ms while applying a Correction, so the hook
    /// stopped holding them. No details.
    /// </summary>
    HoldTimeout,

    /// <summary>
    /// A downloaded update did not match the checksum in the release feed, so it was discarded.
    /// No details.
    /// </summary>
    UpdateChecksumFailed,

    /// <summary>
    /// A downloaded update matched the feed but its code was unsigned or not signed by the Navpaky
    /// publisher, so it was discarded. Details: the signature verdict.
    /// </summary>
    UpdateSignatureFailed,
  }
}
