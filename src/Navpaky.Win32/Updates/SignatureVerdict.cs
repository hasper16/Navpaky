namespace Navpaky.Win32.Updates {
  /// <summary>
  /// Specifies the outcome of an Authenticode check against the pinned publisher.
  /// </summary>
  public enum SignatureVerdict {
    /// <summary>
    /// Windows trusts the signature and it names the pinned publisher.
    /// </summary>
    Valid,

    /// <summary>
    /// The file carries no Authenticode signature, or the package has no main executable.
    /// </summary>
    NotSigned,

    /// <summary>
    /// The signature is broken, expired or chains to a root Windows does not trust, or its
    /// revocation could not be checked.
    /// </summary>
    Untrusted,

    /// <summary>
    /// A certificate in the signature's chain is revoked.
    /// </summary>
    Revoked,

    /// <summary>
    /// Windows trusts the signature but it names another publisher or issuing CA.
    /// </summary>
    WrongPublisher,
  }
}
