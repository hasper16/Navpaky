using System.Collections.Concurrent;
using Navpaky.Core;

namespace Navpaky.Win32.Diagnostics {
  /// <summary>
  /// Owns the word-analysis log on a thread of its own, so a slow disk never holds typing back.
  /// </summary>
  /// <remarks>
  /// Any thread may call it; the work runs in the order it was asked for, so turning the log off
  /// deletes whatever was written before (fix 2.9 of add-final-acceptance).
  /// </remarks>
  public sealed class AnalysisLogWriter : IDisposable {
    private static readonly TimeSpan _stopWait = TimeSpan.FromSeconds(2);

    private readonly AnalysisLog _log;
    private readonly BlockingCollection<Action> _work = [];
    private readonly Thread _thread;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisLogWriter" /> class and starts its
    /// thread.
    /// </summary>
    /// <param name="log">The log the writer owns from now on.</param>
    public AnalysisLogWriter(AnalysisLog log) {
      _log = log;
      _thread = new Thread(Run) { IsBackground = true, Name = "Navpaky analysis log" };
      _thread.Start();
    }

    /// <summary>Adds an analysed Word to the log while it is on.</summary>
    /// <param name="entry">The analysed Word.</param>
    public void Write(LogAnalysis entry) {
      Post(() => _log.Write(entry));
    }

    /// <summary>Turns the log on, or off, which deletes it.</summary>
    /// <param name="enabled">Whether the log is kept.</param>
    public void UseEnabled(bool enabled) {
      Post(() => _log.Enabled = enabled);
    }

    /// <summary>
    /// Tries again to delete the files that could not be deleted when the log was turned off.
    /// </summary>
    public void RetryDeletes() {
      Post(_log.RetryDeletes);
    }

    /// <summary>Waits until the work asked for so far is done.</summary>
    /// <param name="timeout">How long to wait at most.</param>
    /// <returns>Whether the work was done in time.</returns>
    public bool Drain(TimeSpan timeout) {
      var done = new TaskCompletionSource();
      Post(() => done.TrySetResult());
      return done.Task.Wait(timeout);
    }

    /// <inheritdoc />
    public void Dispose() {
      _work.CompleteAdding();
      if (_thread.Join(_stopWait)) {
        _work.Dispose();
      }
    }

    private void Post(Action work) {
      try {
        _work.Add(work);
      } catch (InvalidOperationException) {
        // Disposed: Navpaky is quitting and writes nothing more.
      }
    }

    private void Run() {
      foreach (Action work in _work.GetConsumingEnumerable()) {
        try {
          work();
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
          // A file in use stays until the next try; a Word that cannot be logged is left out.
        }
      }
    }
  }
}
