using Rekey.App.Telemetry;
using Rekey.Core;
using Sentry;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry: "Nothing is sent without consent", "Typed text never leaves the computer",
  // "Crash and freeze reports"; design D1. Sentry's hub is process-wide, so these tests share
  // one collection and never run in parallel with each other.
  [Collection(nameof(SentryHub))]
  public sealed class CrashReportsTests {
    private const string _dsn = "https://key@sentry.invalid/1";

    private readonly CapturingTransport _transport = new();
    private readonly List<string> _recorded = [];

    [Fact]
    public void NoHubWithoutConsent() {
      using CrashReports reports = NewReports();

      reports.Apply(DiagnosticsConsent.All(false));

      Assert.False(SentrySdk.IsEnabled);
    }

    [Fact]
    public void ConsentStartsTheHubAndWithdrawalStopsIt() {
      using CrashReports reports = NewReports();

      reports.Apply(new DiagnosticsConsent(CrashReports: true, UsageDigest: false));
      Assert.True(SentrySdk.IsEnabled);

      reports.Apply(DiagnosticsConsent.All(false));
      Assert.False(SentrySdk.IsEnabled);
    }

    [Fact]
    public void NoEndpointMeansNoHub() {
      using var reports = new CrashReports(
          new CrashReportSetup(null, "1.0.0", () => [], _recorded.Add, _transport));

      reports.Apply(DiagnosticsConsent.All(true));

      Assert.False(SentrySdk.IsEnabled);
    }

    [Fact]
    public void CrashReportCarriesNoWordUserOrMachine() {
      using CrashReports reports = NewReports();
      reports.Apply(DiagnosticsConsent.All(true));

      SentrySdk.ConfigureScope(scope => {
        scope.User = new SentryUser { Username = "invented-user" };
        scope.SetExtra("buffer", "zorblat");
        scope.AddBreadcrumb("typed zorblat");
      });
      SentrySdk.CaptureException(Thrown());
      SentrySdk.Flush(TimeSpan.FromSeconds(5));

      string sent = Assert.Single(_transport.Sent);
      Assert.DoesNotContain("zorblat", sent, StringComparison.Ordinal);
      Assert.DoesNotContain("invented-user", sent, StringComparison.Ordinal);
      Assert.DoesNotContain(Environment.UserName, sent, StringComparison.OrdinalIgnoreCase);
      Assert.DoesNotContain(Environment.MachineName, sent, StringComparison.OrdinalIgnoreCase);
      Assert.DoesNotContain("\"user\"", sent, StringComparison.Ordinal);
      Assert.DoesNotContain("\"breadcrumbs\"", sent, StringComparison.Ordinal);
    }

    [Fact]
    public void CrashReportKeepsTypeFramesVersionsAndIncidents() {
      using var reports = new CrashReports(new CrashReportSetup(
          _dsn, "1.2.3", () => [IncidentType.InputFrozen], _recorded.Add, _transport));
      reports.Apply(DiagnosticsConsent.All(true));

      SentrySdk.CaptureException(Thrown());
      SentrySdk.Flush(TimeSpan.FromSeconds(5));

      string sent = Assert.Single(_transport.Sent);
      Assert.Contains("System.InvalidOperationException", sent, StringComparison.Ordinal);
      Assert.Contains(nameof(Thrown), sent, StringComparison.Ordinal);
      Assert.Contains("1.2.3", sent, StringComparison.Ordinal);
      Assert.Contains("InputFrozen", sent, StringComparison.Ordinal);
      Assert.Contains(Environment.OSVersion.Version.ToString(), sent, StringComparison.Ordinal);
    }

    [Fact]
    public void FreezeReportCarriesTheThreadAndItsStack() {
      using CrashReports reports = NewReports();
      reports.Apply(DiagnosticsConsent.All(true));

      reports.ReportFreeze(
          "Engine", [new SentryStackFrame { Module = "Rekey.Win32", Function = "Worker.Run" }]);
      SentrySdk.Flush(TimeSpan.FromSeconds(5));

      string sent = Assert.Single(_transport.Sent);
      Assert.Contains("Navpaky.Freeze.Engine", sent, StringComparison.Ordinal);
      Assert.Contains("Worker.Run", sent, StringComparison.Ordinal);
    }

    [Fact]
    public void FreezeWithoutConsentSendsNothing() {
      using CrashReports reports = NewReports();
      reports.Apply(DiagnosticsConsent.All(false));

      reports.ReportFreeze("Ui", []);

      Assert.Empty(_transport.Sent);
    }

    [Fact]
    public void SentReportIsRecordedAsSent() {
      using CrashReports reports = NewReports();
      reports.Apply(DiagnosticsConsent.All(true));

      SentrySdk.CaptureException(Thrown());
      SentrySdk.Flush(TimeSpan.FromSeconds(5));

      string recorded = Assert.Single(_recorded);
      Assert.Contains("System.InvalidOperationException", recorded, StringComparison.Ordinal);
    }

    private static InvalidOperationException Thrown() {
      try {
        throw new InvalidOperationException("the Word was zorblat");
      } catch (InvalidOperationException e) {
        return e;
      }
    }

    private CrashReports NewReports() {
      return new CrashReports(
          new CrashReportSetup(_dsn, "1.0.0", () => [], _recorded.Add, _transport));
    }
  }
}
