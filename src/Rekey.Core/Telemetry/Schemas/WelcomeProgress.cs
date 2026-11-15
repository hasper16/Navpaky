namespace Rekey.Core.Telemetry.Schemas {
  /// <summary>
  /// Holds how far the user got in the welcome window.
  /// </summary>
  /// <param name="TriedCorrection">
  /// <see langword="true" /> if Rekey made a Correction in the try-it field; otherwise,
  /// <see langword="false" />.
  /// </param>
  /// <param name="ClosedWithDone">
  /// <see langword="true" /> if the user closed the window with its Done button; otherwise,
  /// <see langword="false" />.
  /// </param>
  public sealed record WelcomeProgress(bool TriedCorrection, bool ClosedWithDone);
}
