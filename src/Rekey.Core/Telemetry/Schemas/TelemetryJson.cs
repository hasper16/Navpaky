namespace Rekey.Core.Telemetry.Schemas {
  /// <summary>
  /// Writes telemetry requests as the JSON that is sent and kept in the sent log.
  /// </summary>
  public static class TelemetryJson {
    /// <summary>Returns the JSON of a Usage Digest.</summary>
    /// <param name="digest">The digest.</param>
    /// <returns>The JSON on one line.</returns>
    public static string Write(UsageDigest digest) {
      return System.Text.Json.JsonSerializer.Serialize(
          digest, TelemetryJsonContext.Default.UsageDigest);
    }

    /// <summary>Returns the JSON of a setup signal.</summary>
    /// <param name="signal">The setup signal.</param>
    /// <returns>The JSON on one line.</returns>
    public static string Write(SetupSignal signal) {
      return System.Text.Json.JsonSerializer.Serialize(
          signal, TelemetryJsonContext.Default.SetupSignal);
    }

    /// <summary>Returns the JSON of an email sign-up.</summary>
    /// <param name="signUp">The sign-up.</param>
    /// <returns>The JSON on one line.</returns>
    public static string Write(SignUp signUp) {
      return System.Text.Json.JsonSerializer.Serialize(signUp, TelemetryJsonContext.Default.SignUp);
    }
  }
}
