namespace Navpaky.App.Telemetry {
  // Design D1 of add-telemetry: pings the UI dispatcher every 500 ms; a ping left unanswered
  // for more than 2 s is a freeze, reported once per stall. The timer's ticks run one after
  // another and own every field except _answered, which only the dispatcher writes.
  internal sealed class DispatcherFreezeWatch : IDisposable {
    private static readonly TimeSpan _period = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan _frozenAfter = TimeSpan.FromSeconds(2);

    private readonly TimeProvider _time;
    private readonly Action<Action> _post;
    private readonly Action _onFreeze;
    private readonly ITimer _timer;
    private long _answered;
    private long _sent;
    private DateTimeOffset _sentAt;
    private bool _reported;

    public DispatcherFreezeWatch(TimeProvider time, Action<Action> post, Action onFreeze) {
      _time = time;
      _post = post;
      _onFreeze = onFreeze;
      _timer = time.CreateTimer(
          _ => Tick(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Start() {
      _timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    }

    public void Dispose() {
      _timer.Dispose();
    }

    private void Tick() {
      DateTimeOffset now = _time.GetUtcNow();
      if (Volatile.Read(ref _answered) == _sent) {
        _reported = false;
        _sentAt = now;
        long ping = ++_sent;
        _post(() => Volatile.Write(ref _answered, ping));
      } else if (!_reported && now - _sentAt > _frozenAfter) {
        _reported = true;
        _onFreeze();
      }

      _timer.Change(_period, Timeout.InfiniteTimeSpan);
    }
  }
}
