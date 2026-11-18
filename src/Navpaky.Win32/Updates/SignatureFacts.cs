namespace Navpaky.Win32.Updates {
  // A file's signature as Windows judged it, with the signer's subject and the thumbprint of the
  // CA that issued the signer's certificate (empty when unsigned).
  internal sealed record SignatureFacts(TrustResult Trust, string Subject, string Issuer);
}
