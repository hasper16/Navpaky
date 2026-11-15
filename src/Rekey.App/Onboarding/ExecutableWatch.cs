using System.IO;

namespace Rekey.App.Onboarding {
  // Notices that Rekey's own executable was moved or deleted while it runs (spec onboarding),
  // so the user learns it will not start again. The folder watcher answers at once; the check
  // every minute catches what it misses, such as a parent folder being moved (design D4).
  internal sealed class ExecutableWatch : IDisposable {
    public static readonly TimeSpan CheckPeriod = TimeSpan.FromSeconds(60);

    private readonly string _path;
    private readonly TimeProvider _time;
    private FileSystemWatcher? _watcher;
    private ITimer? _timer;

    // 1 once Disappeared was raised; the watcher and the timer threads both check.
    private int _reported;

    public ExecutableWatch(string path, TimeProvider time) {
      _path = path;
      _time = time;
    }

    // Raised once, on a watcher or timer thread.
    public event Action? Disappeared;

    public void Start() {
      _watcher = new FileSystemWatcher(Path.GetDirectoryName(_path)!) {
        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
      };
      _watcher.Deleted += (_, _) => Check();
      _watcher.Renamed += (_, _) => Check();
      _watcher.Error += (_, _) => Check();
      _watcher.EnableRaisingEvents = true;
      _timer = _time.CreateTimer(_ => Check(), null, CheckPeriod, CheckPeriod);
    }

    public void Dispose() {
      _timer?.Dispose();
      _watcher?.Dispose();
    }

    private void Check() {
      if (!File.Exists(_path) && Interlocked.Exchange(ref _reported, 1) == 0) {
        Disappeared?.Invoke();
      }
    }
  }
}
