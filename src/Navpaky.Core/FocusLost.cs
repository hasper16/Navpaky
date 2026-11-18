namespace Navpaky.Core {
  /// <summary>
  /// Reports that Navpaky cannot tell which control has focus, as while the desktop switches.
  /// </summary>
  /// <param name="Timestamp">The time of the report, measured from system start.</param>
  public sealed record FocusLost(TimeSpan Timestamp) : InputEvent(Timestamp);
}
