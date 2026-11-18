namespace Navpaky.App.Telemetry {
  // The project's own telemetry endpoints, fixed in the app and HTTPS only (spec telemetry,
  // "Secure transport"). No backend exists yet (design, Non-Goals), so each is null and Navpaky
  // sends nothing even with consent. Set them here once the backend and the Sentry project
  // exist; docs/privacy.md lists what each one receives.
  internal static class TelemetryEndpoints {
    public static string? SentryDsn => null;

    public static Uri? UsageDigest => null;

    public static Uri? SetupSignal => null;

    public static Uri? SignUp => null;
  }
}
