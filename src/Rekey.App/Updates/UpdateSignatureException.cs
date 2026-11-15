using Rekey.Win32.Updates;

namespace Rekey.App.Updates {
  /// <summary>
  /// The exception that is thrown when a downloaded update's code is not signed by the Rekey
  /// publisher.
  /// </summary>
  public sealed class UpdateSignatureException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSignatureException" /> class.
    /// </summary>
    /// <param name="verdict">The verdict that refused the package.</param>
    public UpdateSignatureException(SignatureVerdict verdict)
        : base($"The update package failed the signature check: {verdict}.") {
      Verdict = verdict;
    }

    /// <summary>
    /// Gets the verdict that refused the package.
    /// </summary>
    public SignatureVerdict Verdict { get; }
  }
}
