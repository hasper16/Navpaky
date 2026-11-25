namespace Navpaky.App.Updates {
  // Closes an all-users Navpaky when the elevated update or uninstall asks every session to
  // (design D6 of add-machine-wide-install). Polling: the event exists only while the hook runs.
  internal sealed class CloseForUpdate : IDisposable {
    // Design decision: often enough for the hook's 10 s wait, one kernel call per tick.
    public static readonly TimeSpan Period = TimeSpan.FromSeconds(2);

    private readonly Func<bool> _isRaised;
    private readonly Action _close;
    private readonly TimeSpan _period;
    private ITimer? _timer;

    // 1 once the close was asked for; timer callbacks may overlap.
    private int _closing;

    public CloseForUpdate(Func<bool> isRaised, Action close, TimeSpan period) {
      _isRaised = isRaised;
      _close = close;
      _period = period;
    }

    public void Start() {
      _timer = TimeProvider.System.CreateTimer(_ => Check(), null, _period, _period);
    }

    public void Check() {
      if (_isRaised() && Interlocked.Exchange(ref _closing, 1) == 0) {
        _close();
      }
    }

    public void Dispose() {
      _timer?.Dispose();
    }
  }
}
