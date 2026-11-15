using System.Threading.Channels;

namespace Rekey.Core.Diagnostics {
  /// <summary>
  /// Takes Incidents from any thread and appends them to an <see cref="IncidentLog" /> on one
  /// background task.
  /// </summary>
  /// <remarks>
  /// Recording never blocks or touches the disk, so the freeze guard can record while the worker
  /// is stuck.
  /// </remarks>
  public sealed class QueuedIncidentLog : IDisposable {
    // How long disposing waits for queued Incidents to reach the disk.
    private static readonly TimeSpan _drainWait = TimeSpan.FromSeconds(2);

    private readonly Channel<Incident> _queue = Channel.CreateUnbounded<Incident>(
        new UnboundedChannelOptions { SingleReader = true });

    private readonly IncidentLog _log;
    private readonly Action<Exception> _failed;
    private readonly Task _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedIncidentLog" /> class.
    /// </summary>
    /// <param name="log">The log to append to; from now on only this instance appends.</param>
    /// <param name="failed">
    /// The handler of an Incident that could not be written; it is called on the background task.
    /// </param>
    public QueuedIncidentLog(IncidentLog log, Action<Exception> failed) {
      _log = log;
      _failed = failed;
      _writer = Task.Run(WriteAsync);
    }

    /// <summary>
    /// Queues an Incident to be logged; safe to call from any thread.
    /// </summary>
    /// <param name="incident">The Incident.</param>
    public void Record(Incident incident) {
      _queue.Writer.TryWrite(incident);
    }

    /// <summary>
    /// Stops taking Incidents and waits up to 2 seconds for the queued ones to be written.
    /// </summary>
    public void Dispose() {
      _queue.Writer.TryComplete();
      _writer.Wait(_drainWait);
    }

    private async Task WriteAsync() {
      await foreach (Incident incident in _queue.Reader.ReadAllAsync().ConfigureAwait(false)) {
        try {
          _log.Append(incident);
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
          _failed(e);
        }
      }
    }
  }
}
