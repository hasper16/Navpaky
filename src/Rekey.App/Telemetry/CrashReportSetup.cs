using Rekey.Core;
using Sentry.Extensibility;

namespace Rekey.App.Telemetry {
  // What crash reports need from the app. Dsn is null until the project has its own Sentry
  // endpoint (TelemetryEndpoints); then nothing starts. Transport replaces the network in tests.
  internal sealed record CrashReportSetup(
      string? Dsn,
      string Release,
      Func<IReadOnlyList<IncidentType>> RecentIncidents,
      Action<string> RecordSent,
      ITransport? Transport = null);
}
