using Rekey.Core;

namespace Rekey.Win32.Keyboard {
  /// <summary>
  /// Holds what a freeze check asks the host to do.
  /// </summary>
  /// <param name="Restart">
  /// <see langword="true" /> to restart Rekey; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="Incident">The Incident to record, if any.</param>
  /// <param name="Stalled">
  /// <see langword="true" /> to report a freeze of the worker, once per stall; otherwise,
  /// <see langword="false" />.
  /// </param>
  internal sealed record FreezeOutcome(bool Restart, Incident? Incident, bool Stalled = false) {
    /// <summary>Gets the outcome that asks for nothing.</summary>
    public static FreezeOutcome None { get; } = new(false, null);

    /// <summary>Gets the outcome that only reports a stall.</summary>
    public static FreezeOutcome Stall { get; } = new(false, null, Stalled: true);
  }
}
