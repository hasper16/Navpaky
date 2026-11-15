namespace Rekey.Core.Telemetry.Schemas {
  /// <summary>
  /// Represents the optional email sign-up; it is sent on its own and never linked to a report.
  /// </summary>
  /// <param name="Email">The email address the user entered.</param>
  /// <param name="NewVersions">
  /// <see langword="true" /> to hear about new versions; otherwise, <see langword="false" />.
  /// </param>
  /// <param name="ReportedProblems">
  /// <see langword="true" /> to hear about problems the user reported; otherwise,
  /// <see langword="false" />.
  /// </param>
  public sealed record SignUp(string Email, bool NewVersions, bool ReportedProblems) {
    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>The schema version. The default is 1.</value>
    public int SchemaVersion { get; } = 1;
  }
}
