using System.Buffers;
using System.Text;
using System.Text.Json;
using Rekey.Core;
using Sentry;
using Sentry.Protocol;

namespace Rekey.App.Telemetry {
  // Crash and freeze reports through Sentry (spec telemetry, design D1). The hub exists only
  // while the user agrees to crash reports; withdrawing consent closes the gate first, so
  // what is still queued fails instead of being sent. Apply and Dispose run on the UI thread.
  internal sealed class CrashReports : IDisposable {
    private readonly CrashReportSetup _setup;
    private volatile IDisposable? _sdk;
    private ConsentGate? _gate;

    public CrashReports(CrashReportSetup setup) {
      _setup = setup;
    }

    public void Apply(DiagnosticsConsent consent) {
      if (consent.CrashReports && _sdk is null && _setup.Dsn is { } dsn) {
        _gate = ConsentGate.ForNetwork();
        _sdk = SentrySdk.Init(options => Configure(options, dsn, _gate));
      } else if (!consent.CrashReports && _sdk is not null) {
        Stop();
      }
    }

    public bool IsActive => _sdk is not null;

    // A freeze is reported as an exception named after the stalled thread, so the server
    // groups it like a crash; the frames come from ThreadStacks.
    public void ReportFreeze(string thread, IReadOnlyList<SentryStackFrame> frames) {
      if (_sdk is null) {
        return;
      }

      SentrySdk.CaptureEvent(new SentryEvent {
        Level = SentryLevel.Warning,
        SentryExceptions = [new SentryException {
          Type = $"Navpaky.Freeze.{thread}",
          Stacktrace = new SentryStackTrace { Frames = [.. frames.Reverse()] },
        }],
      });
    }

    // Sends what is queued before Rekey quits; withdrawing consent stops without this.
    public void Flush() {
      if (_sdk is not null) {
        SentrySdk.Flush(TimeSpan.FromSeconds(2));
      }
    }

    public void Dispose() {
      Stop();
    }

    private static string Json(SentryEvent report) {
      var buffer = new ArrayBufferWriter<byte>();
      using (var writer = new Utf8JsonWriter(buffer)) {
        report.WriteTo(writer, null);
      }

      return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private void Stop() {
      _gate?.Close();
      _gate = null;
      _sdk?.Dispose();
      _sdk = null;
    }

    private void Configure(SentryOptions options, string dsn, ConsentGate gate) {
      options.Dsn = dsn;
      options.Release = _setup.Release;
      options.IsGlobalModeEnabled = true;
      options.SendDefaultPii = false;
      options.IsEnvironmentUser = false;
      options.AttachStacktrace = true;
      options.MaxBreadcrumbs = 0;
      options.AutoSessionTracking = false;
      options.ReportAssembliesMode = ReportAssembliesMode.None;
      options.SendClientReports = false;
      options.DisableFileWrite = true;
      options.CacheDirectoryPath = null;

      // A fatal crash flushes for this long before the process ends; nothing is cached on disk.
      options.ShutdownTimeout = TimeSpan.FromSeconds(2);
      options.CreateHttpMessageHandler = () => gate;
      options.DisableDiagnosticSourceIntegration();
      options.SetBeforeBreadcrumb((_, _) => null);
      options.SetBeforeSend((sentryEvent, hint) => {
        hint.Attachments.Clear();
        SentryEvent report = ReportAllowlist.Rebuild(
            sentryEvent, _setup.Release, _setup.RecentIncidents());
        _setup.RecordSent(Json(report));
        return report;
      });
      if (_setup.Transport is { } transport) {
        options.Transport = transport;
      }
    }
  }
}
