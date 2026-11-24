namespace Navpaky.Setup {
  // One wizard per user session (design risks of add-install-wizard): the single-file bundle
  // takes seconds to start, so users double-click again. A later start wakes the first and exits.
  internal sealed class SingleInstance : IDisposable {
    private readonly Mutex _claim;
    private readonly EventWaitHandle _wake;
    private readonly RegisteredWaitHandle _waiting;

    private SingleInstance(Mutex claim, EventWaitHandle wake, Action woken) {
      _claim = claim;
      _wake = wake;
      _waiting = ThreadPool.RegisterWaitForSingleObject(
          wake, (_, _) => woken(), null, Timeout.Infinite, executeOnlyOnce: false);
    }

    // Null when another wizard holds the name; that one is woken. woken runs on a pool thread.
    public static SingleInstance? TryClaim(string name, Action woken) {
      var claim = new Mutex(initiallyOwned: false, $@"Local\{name}", out bool first);
      var wake = new EventWaitHandle(false, EventResetMode.AutoReset, $@"Local\{name}.wake");
      if (first) {
        return new SingleInstance(claim, wake, woken);
      }

      wake.Set();
      wake.Dispose();
      claim.Dispose();
      return null;
    }

    public void Dispose() {
      _waiting.Unregister(null);
      _wake.Dispose();
      _claim.Dispose();
    }
  }
}
