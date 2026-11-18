namespace Navpaky.Core {
  /// <summary>
  /// Specifies why Navpaky switched the Layout.
  /// </summary>
  public enum SwitchCause {
    /// <summary>
    /// A Correction replaced text and switched to the target Layout.
    /// </summary>
    Correction,

    /// <summary>
    /// An Undo restored the typed text and switched back.
    /// </summary>
    Undo,

    /// <summary>
    /// The Hotkey found nothing to correct and switched to the next Layout.
    /// </summary>
    LayoutSwitch,
  }
}
