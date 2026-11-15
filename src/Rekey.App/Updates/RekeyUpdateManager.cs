using System.IO;
using Rekey.Win32.Updates;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Rekey.App.Updates {
  // Velopack's update manager with Rekey's own checks: no release that is not newer than the
  // running one, and no package whose code the Rekey publisher did not sign (spec auto-update
  // "No downgrades" and "Only authentic updates are applied").
  internal class RekeyUpdateManager : UpdateManager {
    private readonly Func<string, SignatureVerdict> _verifySignature;

    // Deltas are rebuilt into a full package that Velopack does not pass to
    // VerifyPackageChecksumAsync, so only full packages are downloaded.
    public RekeyUpdateManager(
        IUpdateSource source,
        Func<string, SignatureVerdict> verifySignature,
        IVelopackLocator? locator = null)
        : base(source, new UpdateOptions { MaximumDeltasBeforeFallback = -1 }, locator) {
      _verifySignature = verifySignature;
    }

    public override async Task<UpdateInfo?> CheckForUpdatesAsync() {
      UpdateInfo? update = await base.CheckForUpdatesAsync().ConfigureAwait(false);
      if (update is null || !VersionGate.Allows(CurrentVersion, update.TargetFullRelease.Version)) {
        return null;
      }

      return update;
    }

    // Velopack calls this on each downloaded package before it keeps it; a throw leaves the
    // running version in place.
    protected override async Task VerifyPackageChecksumAsync(
        VelopackAsset release, string? filePathOverride = null) {
      string package = filePathOverride ?? Path.Join(Locator.PackagesDir, release.FileName);
      try {
        await base.VerifyPackageChecksumAsync(release, package).ConfigureAwait(false);
        SignatureVerdict verdict = _verifySignature(package);
        if (verdict != SignatureVerdict.Valid) {
          throw new UpdateSignatureException(verdict);
        }
      } catch {
        File.Delete(package);
        throw;
      }
    }
  }
}
