using Navpaky.Core;
using Sentry;
using Sentry.Protocol;

namespace Navpaky.App.Telemetry {
  // Design D1: a report is rebuilt from the fields listed here, so anything Sentry or a future
  // integration adds (user, request, extra, breadcrumbs, device and server names, module paths,
  // exception messages, local variables) is dropped.
  internal static class ReportAllowlist {
    public static SentryEvent Rebuild(
        SentryEvent source, string release, IReadOnlyList<IncidentType> incidents) {
      var report = new SentryEvent {
        Level = source.Level,
        Release = release,
        SentryExceptions = source.SentryExceptions?.Select(Strip).ToList(),
      };
      report.Contexts.OperatingSystem.Name = "Windows";
      report.Contexts.OperatingSystem.Version = Environment.OSVersion.Version.ToString();
      report.SetTag("incidents", string.Join(',', incidents));
      return report;
    }

    private static SentryException Strip(SentryException source) {
      return new SentryException {
        Type = source.Type,
        Mechanism = source.Mechanism is { } mechanism
            ? new Mechanism { Type = mechanism.Type, Handled = mechanism.Handled }
            : null,
        Stacktrace = source.Stacktrace is { } trace
            ? new SentryStackTrace { Frames = [.. trace.Frames.Select(Strip)] }
            : null,
      };
    }

    private static SentryStackFrame Strip(SentryStackFrame source) {
      return new SentryStackFrame {
        Module = source.Module,
        Function = source.Function,
        LineNumber = source.LineNumber,
        InApp = source.InApp,
      };
    }
  }
}
