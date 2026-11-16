using Rekey.Core;

namespace Rekey.Win32.Keyboard {
  /// <summary>
  /// Notices when the worker thread stops processing input and decides when Rekey restarts.
  /// </summary>
  /// <remarks>
  /// Runs on its own thread so it keeps working while the worker is stuck; it only reads the
  /// worker's heartbeat and queue length. Not thread-safe: that one thread owns it.
  /// </remarks>
  internal sealed class FreezeWatchdog {
    // Design decisions (D7): a stall longer than 5 s is a freeze, a second freeze within an hour
    // restarts; the original restarts at most once per 10 minutes.
    private static readonly TimeSpan _frozenAfter = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _repeatWindow = TimeSpan.FromHours(1);
    private static readonly TimeSpan _restartGap = TimeSpan.FromMinutes(10);

    // Spec telemetry: a stall over 2 s is reported as a freeze, though not yet an Incident.
    private static readonly TimeSpan _reportAfter = TimeSpan.FromSeconds(2);

    private readonly IWorkerPulse _pulse;
    private readonly TimeProvider _time;
    private long _lastHeartbeat = -1;
    private long _movingSince;
    private bool _frozen;
    private bool _reported;
    private DateTimeOffset? _lastFreeze;
    private DateTimeOffset? _lastRestart;
    private bool _restartEnabled = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="FreezeWatchdog" /> class.
    /// </summary>
    /// <param name="pulse">The worker's heartbeat and queue.</param>
    /// <param name="time">The clock.</param>
    /// <param name="lastRestart">
    /// The time of the last restart after a freeze, kept across restarts, or
    /// <see langword="null" /> when there was none.
    /// </param>
    public FreezeWatchdog(IWorkerPulse pulse, TimeProvider time, DateTimeOffset? lastRestart) {
      _pulse = pulse;
      _time = time;
      _lastRestart = lastRestart;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a repeated freeze restarts Rekey; the settings
    /// thread writes it while the guard's thread reads it.
    /// </summary>
    public bool RestartEnabled {
      get => Volatile.Read(ref _restartEnabled);
      set => Volatile.Write(ref _restartEnabled, value);
    }

    /// <summary>Checks the worker once.</summary>
    /// <returns>What the host must do.</returns>
    public FreezeOutcome Check() {
      long heartbeat = _pulse.Heartbeat;

      // A stall counts only while input waits: an idle worker has nothing to fall behind on.
      if (heartbeat != _lastHeartbeat || _pulse.QueuedInput == 0) {
        _lastHeartbeat = heartbeat;
        _movingSince = _time.GetTimestamp();
        _frozen = false;
        _reported = false;
        return FreezeOutcome.None;
      }

      TimeSpan stalled = _time.GetElapsedTime(_movingSince);
      bool report = !_reported && stalled > _reportAfter;
      _reported |= report;
      if (_frozen || stalled <= _frozenAfter) {
        return report ? FreezeOutcome.Stall : FreezeOutcome.None;
      }

      _frozen = true;
      DateTimeOffset now = _time.GetUtcNow();
      bool repeated = now - _lastFreeze < _repeatWindow;
      _lastFreeze = now;
      bool restartAllowed = _lastRestart is not { } last || now - last >= _restartGap;
      if (repeated && RestartEnabled && restartAllowed) {
        _lastRestart = now;
        _lastFreeze = null;
        return new FreezeOutcome(
            true, new Incident(now, IncidentType.FreezeRestart, []), report);
      }

      return new FreezeOutcome(false, new Incident(now, IncidentType.InputFrozen, []), report);
    }
  }
}
