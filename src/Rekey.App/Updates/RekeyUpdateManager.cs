using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Rekey.App.Updates {
  // Velopack's update manager with Rekey's own checks: no release that is not newer than the
  // running one (spec auto-update "No downgrades").
  internal class RekeyUpdateManager : UpdateManager {
    // Deltas are rebuilt into a full package that Velopack does not pass to
    // VerifyPackageChecksumAsync, so only full packages are downloaded.
    public RekeyUpdateManager(IUpdateSource source, IVelopackLocator? locator = null)
        : base(source, new UpdateOptions { MaximumDeltasBeforeFallback = -1 }, locator) {
    }

    public override async Task<UpdateInfo?> CheckForUpdatesAsync() {
      UpdateInfo? update = await base.CheckForUpdatesAsync().ConfigureAwait(false);
      if (update is null || !VersionGate.Allows(CurrentVersion, update.TargetFullRelease.Version)) {
        return null;
      }

      return update;
    }
  }
}
