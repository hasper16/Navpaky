using Rekey.Win32.Updates;

namespace Rekey.App.Updates {
  // The publisher whose signed code Rekey applies as an update (design D2 of
  // add-auto-update-and-installer).
  internal static class RekeyPublisher {
    public const string MainExe = "Rekey.App.exe";

    // No code-signing certificate is chosen yet (design, open question), so the pin matches no
    // one and every update is refused. Set the certificate's subject and its issuing CA
    // thumbprints here once the certificate exists; docs/release.md says how to read them.
    public static readonly PublisherPin Pin = PublisherPin.None;

    public static SignatureVerdict Verify(string package) {
      return PackageSignature.Verify(package, MainExe, Pin);
    }
  }
}
