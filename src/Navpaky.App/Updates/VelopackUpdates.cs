using System.Diagnostics;
using Navpaky.Core;
using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;

namespace Navpaky.App.Updates {
  // Checks, downloads and applies updates through Velopack (design D1 and D3 of
  // add-auto-update-and-installer).
  internal sealed class VelopackUpdates : IUpdateService {
    // The public repository's GitHub Releases over HTTPS, fixed in the app: no access token
    // ships and the user cannot point Navpaky elsewhere.
    public const string Repository = "https://github.com/hasper16/key-switcher";

    private readonly NavpakyUpdateManager _manager;
    private readonly Action _exit;
    private readonly Action<Incident> _recordIncident;
    private readonly TimeProvider _clock;
    private VelopackAsset? _ready;

    public VelopackUpdates(
        NavpakyUpdateManager manager,
        Action exit,
        Action<Incident> recordIncident,
        TimeProvider clock) {
      _manager = manager;
      _exit = exit;
      _recordIncident = recordIncident;
      _clock = clock;
    }

    public bool IsAvailable => _manager.IsInstalled;

    public static VelopackUpdates FromReleases(Action exit, Action<Incident> recordIncident) {
      var source = new GithubSource(Repository, null, prerelease: false);
      return new VelopackUpdates(
          new NavpakyUpdateManager(source, NavpakyPublisher.Verify),
          exit,
          recordIncident,
          TimeProvider.System);
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
      } catch (ChecksumFailedException) {
        Record(IncidentType.UpdateChecksumFailed, []);
      } catch (UpdateSignatureException e) {
        Record(IncidentType.UpdateSignatureFailed, [e.Verdict]);
      } catch (Exception e) {
        // The type only: messages may name paths inside the user profile.
        Trace.WriteLine($"Navpaky could not update: {e.GetType().Name}");
      }

      return UpdateStatus.Failed;
    }

    // Velopack's updater waits for Navpaky to exit, so settings and word lists are saved first.
    public void RestartToUpdate() {
      if (_ready is null) {
        return;
      }

      // If the updater cannot start, Navpaky keeps running and the update stays ready to retry.
      try {
        _manager.WaitExitThenApplyUpdates(_ready, silent: true, restart: true, []);
      } catch (Exception e) {
        Trace.WriteLine($"Navpaky could not start its updater: {e.GetType().Name}");
        return;
      }

      _exit();
    }

    private void Record(IncidentType type, IReadOnlyList<Enum> details) {
      _recordIncident(new Incident(_clock.GetUtcNow(), type, details));
    }
  }
}
