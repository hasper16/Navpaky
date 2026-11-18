namespace Navpaky.Win32.Updates {
  // Design D2 of add-auto-update-and-installer: a checksum that matches the feed proves only
  // that the package is what the feed lists; the publisher pin proves the code is ours.
  internal static class SignaturePolicy {
    public static SignatureVerdict Judge(SignatureFacts facts, PublisherPin pin) {
      return facts.Trust switch {
        TrustResult.NotSigned => SignatureVerdict.NotSigned,
        TrustResult.Untrusted => SignatureVerdict.Untrusted,
        TrustResult.Revoked => SignatureVerdict.Revoked,
        _ when IsPinned(facts, pin) => SignatureVerdict.Valid,
        _ => SignatureVerdict.WrongPublisher,
      };
    }

    private static bool IsPinned(SignatureFacts facts, PublisherPin pin) {
      return pin.Subject.Length > 0
          && string.Equals(facts.Subject, pin.Subject, StringComparison.Ordinal)
          && pin.IssuerThumbprints.Contains(facts.Issuer, StringComparer.OrdinalIgnoreCase);
    }
  }
}
