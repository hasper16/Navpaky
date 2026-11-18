namespace Navpaky.Core.Telemetry.Schemas {
  /// <summary>
  /// Represents the daily Usage Digest: counters of one day and no identifier that lasts
  /// longer than the request.
  /// </summary>
  /// <param name="RequestId">
  /// A random identifier of this request alone, so the server can drop a repeated delivery.
  /// </param>
  /// <param name="Day">The local day the counters cover.</param>
  /// <param name="Environment">The versions and languages.</param>
  /// <param name="Settings">The on/off states of the main settings.</param>
  /// <param name="Counters">The day's counters.</param>
  public sealed record UsageDigest(
      Guid RequestId,
      DateOnly Day,
      AppEnvironment Environment,
      SettingStates Settings,
      DigestCounters Counters) {
    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>The schema version. The default is 1.</value>
    public int SchemaVersion { get; } = 1;
  }
}
