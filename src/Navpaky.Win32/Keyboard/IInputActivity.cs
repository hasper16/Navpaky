namespace Navpaky.Win32.Keyboard {
  /// <summary>
  /// Tells when the system and Navpaky's hook last saw input, in milliseconds since system start.
  /// </summary>
  internal interface IInputActivity {
    /// <summary>
    /// Gets the tick of the last input anywhere in the session.
    /// </summary>
    long SystemLastInputTick { get; }

    /// <summary>
    /// Gets the tick of the last input Navpaky's hooks saw.
    /// </summary>
    long HookLastInputTick { get; }

    /// <summary>
    /// Gets the number of times the hooks were installed again successfully.
    /// </summary>
    long Reattachments { get; }
  }
}
