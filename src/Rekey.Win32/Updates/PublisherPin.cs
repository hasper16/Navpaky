namespace Rekey.Win32.Updates {
  /// <summary>
  /// Names the only publisher whose signed code Rekey applies as an update.
  /// </summary>
  /// <param name="Subject">
  /// The exact subject distinguished name of the publisher's code-signing certificate.
  /// </param>
  /// <param name="IssuerThumbprints">
  /// The SHA-1 thumbprints of the CAs allowed to issue that certificate.
  /// </param>
  public sealed record PublisherPin(string Subject, IReadOnlyList<string> IssuerThumbprints) {
    /// <summary>
    /// A pin that matches no publisher, so every update is refused.
    /// </summary>
    public static readonly PublisherPin None = new(string.Empty, []);
  }
}
