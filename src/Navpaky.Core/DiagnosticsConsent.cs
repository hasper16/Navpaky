namespace Navpaky.Core {
  /// <summary>
  /// Holds what the user agreed Navpaky may send about crashes and usage; the default value agrees
  /// to nothing.
  /// </summary>
  /// <param name="CrashReports">
  /// <see langword="true" /> to send crash and freeze reports; otherwise,
  /// <see langword="false" />.
  /// </param>
  /// <param name="UsageDigest">
  /// <see langword="true" /> to send the daily Usage Digest and the setup signal; otherwise,
  /// <see langword="false" />.
  /// </param>
  public readonly record struct DiagnosticsConsent(bool CrashReports, bool UsageDigest) {
    /// <summary>
    /// Returns a consent that agrees to every kind of report or to none.
    /// </summary>
    /// <param name="agreed">
    /// <see langword="true" /> to agree to everything; <see langword="false" /> to nothing.
    /// </param>
    /// <returns>The consent.</returns>
    public static DiagnosticsConsent All(bool agreed) {
      return new DiagnosticsConsent(agreed, agreed);
    }
  }
}
