using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Microsoft.Diagnostics.Runtime;
using Rekey.Core;
using Rekey.Core.Diagnostics;
using Rekey.Core.Telemetry;

namespace Rekey.App.Telemetry {
  // Everything Rekey may send (spec telemetry), started and stopped by the user's consent.
  // Created, updated and disposed on the UI thread; ReportEngineStall runs on the freeze
  // guard's thread.
  internal sealed class TelemetryHost : IDisposable {
    // Spec telemetry: a crash report carries the Incident types of the last 10 minutes.
    private static readonly TimeSpan _incidentWindow = TimeSpan.FromMinutes(10);

    private readonly string _dataFolder;
    private readonly int _uiThreadId;
    private readonly CrashReports _crashReports;
    private readonly DispatcherFreezeWatch _uiWatch;

    public TelemetryHost(string dataFolder, Dispatcher dispatcher) {
      _dataFolder = dataFolder;
      _uiThreadId = dispatcher.Thread.ManagedThreadId;
      Sent = new SentLog(dataFolder);
      _crashReports = new CrashReports(new CrashReportSetup(
          TelemetryEndpoints.SentryDsn, Version, RecentIncidents, RecordSent));
      _uiWatch = new DispatcherFreezeWatch(
          TimeProvider.System,
          ping => dispatcher.BeginInvoke(DispatcherPriority.Send, ping),
          () => ReportFreeze("Ui", _uiThreadId));
    }

    public static string Version { get; } = (Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0").Split('+')[0];

    public SentLog Sent { get; }

    public void Start(Settings settings) {
      Apply(settings);
      _uiWatch.Start();
    }

    public void Apply(Settings settings) {
      _crashReports.Apply(settings.DiagnosticsConsent);
    }

    public void ReportEngineStall(int workerThreadId) {
      ReportFreeze("Engine", workerThreadId);
    }

    public void Flush() {
      _crashReports.Flush();
    }

    public void Dispose() {
      _uiWatch.Dispose();
      _crashReports.Dispose();
    }

    private void ReportFreeze(string thread, int managedThreadId) {
      if (!_crashReports.IsActive) {
        return;
      }

      try {
        _crashReports.ReportFreeze(thread, ThreadStacks.Of(managedThreadId));
      } catch (Exception e) when (e is ClrDiagnosticsException or InvalidOperationException
          or NotSupportedException or IOException) {
        // The snapshot may be refused; the freeze is still worth reporting without its stack.
        _crashReports.ReportFreeze(thread, []);
      }
    }

    private IReadOnlyList<IncidentType> RecentIncidents() {
      DateTimeOffset since = DateTimeOffset.UtcNow - _incidentWindow;
      try {
        return [.. new IncidentLog(_dataFolder).Read()
            .Where(entry => entry.Time >= since)
            .Select(entry => entry.Type)];
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return [];
      }
    }

    private void RecordSent(string json) {
      try {
        Sent.Append(json);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        System.Diagnostics.Trace.WriteLine(
            $"Rekey could not keep a sent request: {e.GetType().Name}");
      }
    }
  }
}
