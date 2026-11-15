using System.IO;
using Rekey.Core;

namespace Rekey.App.ViewModels {
  // The one place the app's settings change (design D4): the tray and the settings window
  // update it on the UI thread, the engine and the views follow Changed, and the file is written
  // 500 ms after the last change.
  internal sealed class SettingsHub : IDisposable {
    private static readonly TimeSpan _saveDelay = TimeSpan.FromMilliseconds(500);

    private readonly Action<Settings> _save;
    private readonly Action<Exception> _failed;
    private readonly ITimer _timer;

    // Guards _unsaved, which the UI thread fills and a save on the timer's thread or a flush
    // takes; one save at a time writes the file.
    private readonly Lock _pendingLock = new();
    private readonly Lock _fileLock = new();
    private Settings? _unsaved;

    // Written only on the UI thread.
    private Settings _current;

    public SettingsHub(
        Settings initial,
        Action<Settings> save,
        TimeProvider time,
        Action<Exception>? failed = null) {
      _current = initial;
      _save = save;
      _failed = failed ?? (_ => { });
      _timer = time.CreateTimer(
          _ => SaveOnTimer(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public event Action<Settings>? Changed;

    public Settings Current => _current;

    public void Update(Func<Settings, Settings> change) {
      Settings next = change(_current);
      if (next == _current) {
        return;
      }

      _current = next;
      lock (_pendingLock) {
        _unsaved = next;
      }

      _timer.Change(_saveDelay, Timeout.InfiniteTimeSpan);
      Changed?.Invoke(next);
    }

    // Saves a pending change now; a failure reaches the caller.
    public void Flush() {
      _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
      Save();
    }

    public void Dispose() {
      _timer.Dispose();
    }

    // An exception must not leave a timer callback, which would end the process. The snapshot
    // stays pending, so the next change or the flush on exit saves it again.
    private void SaveOnTimer() {
      try {
        Save();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        _failed(e);
      }
    }

    private void Save() {
      lock (_fileLock) {
        Settings? settings;
        lock (_pendingLock) {
          settings = _unsaved;
          _unsaved = null;
        }

        if (settings is null) {
          return;
        }

        try {
          _save(settings);
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
          lock (_pendingLock) {
            // A newer snapshot from the UI thread replaces the one that failed.
            _unsaved ??= settings;
          }

          throw;
        }
      }
    }
  }
}
