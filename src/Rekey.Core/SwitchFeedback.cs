using Rekey.Core.Languages;

namespace Rekey.Core {
  /// <summary>
  /// Describes a Layout Switch Rekey made, for the cursor indicator and the switch sound.
  /// </summary>
  /// <param name="Target">The Layout Rekey switched to.</param>
  /// <param name="Cause">One of the enumeration values that specifies why.</param>
  public sealed record SwitchFeedback(LayoutId Target, SwitchCause Cause) {
    /// <summary>
    /// Returns what the engine's answer to an input event means for the user's feedback.
    /// </summary>
    /// <param name="handled">The input event the engine handled.</param>
    /// <param name="actions">The actions the engine returned for it.</param>
    /// <returns>
    /// The feedback, or <see langword="null" /> when the actions switch no Layout.
    /// </returns>
    public static SwitchFeedback? Of(InputEvent handled, IReadOnlyList<EngineAction> actions) {
      if (actions.OfType<SwitchLayout>().FirstOrDefault() is not { } switchLayout) {
        return null;
      }

      if (actions.OfType<UndoPerformed>().Any()) {
        return new SwitchFeedback(switchLayout.Target, SwitchCause.Undo);
      }

      // A Correction switches once its replacement is applied; the Hotkey alone switches at once.
      SwitchCause cause = handled is ReplacementFinished
          ? SwitchCause.Correction
          : SwitchCause.LayoutSwitch;
      return new SwitchFeedback(switchLayout.Target, cause);
    }
  }
}
