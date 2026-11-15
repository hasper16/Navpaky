namespace Rekey.Win32.Updates {
  // What WinVerifyTrust said about a file.
  internal enum TrustResult {
    Trusted,
    NotSigned,
    Untrusted,
    Revoked,
  }
}
