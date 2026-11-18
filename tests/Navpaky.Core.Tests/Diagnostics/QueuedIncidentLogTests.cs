using Navpaky.Core.Diagnostics;
using Xunit;

namespace Navpaky.Core.Tests.Diagnostics {
  // Incidents come from the worker, the freeze guard and start-up; one background writer owns
  // the file.
  public sealed class QueuedIncidentLogTests : IDisposable {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("navpaky-queued-");

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public async Task IncidentsFromManyThreadsAreAllWrittenByDispose() {
      var log = new IncidentLog(_root.FullName);
      using (var queued = new QueuedIncidentLog(log, _ => { })) {
        await Task.WhenAll(Enumerable.Range(0, 4).Select(thread => Task.Run(() => {
          for (int i = 0; i < 25; i++) {
            queued.Record(new Incident(_monday, IncidentType.HookReinstalled, []));
          }
        })));
      }

      Assert.Equal(100, log.Read().Count);
    }

    [Fact]
    public void WriteFailureIsReportedAndLoggingGoesOn() {
      string blocked = Path.Join(_root.FullName, "file");
      File.WriteAllText(blocked, string.Empty);
      var failures = new List<Exception>();

      // The folder is a file, so every write fails.
      using (var queued = new QueuedIncidentLog(new IncidentLog(blocked), failures.Add)) {
        queued.Record(new Incident(_monday, IncidentType.HookReinstalled, []));
        queued.Record(new Incident(_monday, IncidentType.SelfPaused, []));
      }

      Assert.Equal(2, failures.Count);
    }
  }
}
