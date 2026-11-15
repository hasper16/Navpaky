using System.Diagnostics;
using Velopack;
using Velopack.Sources;

namespace Rekey.App.Updates {
  // Checks, downloads and applies updates through Velopack (design D1 and D3 of
  // add-auto-update-and-installer).
  internal sealed class VelopackUpdates : IUpdateService {
    // The public repository's GitHub Releases over HTTPS, fixed in the app: no access token
    // ships and the user cannot point Rekey elsewhere.
    public const string Repository = "https://github.com/hasper16/key-switcher";

    private readonly RekeyUpdateManager _manager;
    private readonly Action _exit;
    private VelopackAsset? _ready;

    public VelopackUpdates(RekeyUpdateManager manager, Action exit) {
      _manager = manager;
      _exit = exit;
    }

    public bool IsAvailable => _manager.IsInstalled;

    public static VelopackUpdates FromReleases(Action exit) {
      return new VelopackUpdates(
          new RekeyUpdateManager(new GithubSource(Repository, null, prerelease: false)), exit);
    }

    public async Task<UpdateStatus> CheckAndDownloadAsync() {
      try {
        UpdateInfo? update = await _manager.CheckForUpdatesAsync();
        if (update is null) {
          return UpdateStatus.UpToDate;
        }

        await _manager.DownloadUpdatesAsync(update);
        _ready = update.TargetFullRelease;
        return UpdateStatus.ReadyToRestart;
      } catch (Exception e) {
        // The type only: messages may name paths inside the user profile.
        Trace.WriteLine($"Rekey could not update: {e.GetType().Name}");
        return UpdateStatus.Failed;
      }
    }

    // Velopack's updater waits for Rekey to exit, so settings and word lists are saved first.
    public void RestartToUpdate() {
      if (_ready is null) {
        return;
      }

      _manager.WaitExitThenApplyUpdates(_ready, silent: true, restart: true, []);
      _exit();
    }
  }
}
