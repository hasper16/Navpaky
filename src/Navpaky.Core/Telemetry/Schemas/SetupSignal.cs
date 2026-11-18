namespace Navpaky.Core.Telemetry.Schemas {
  /// <summary>
  /// Represents the one-time anonymous setup signal sent when the welcome window closes.
  /// </summary>
  /// <param name="RequestId">
  /// A random identifier of this request alone, so the server can drop a repeated delivery.
  /// </param>
  /// <param name="Environment">The versions and languages.</param>
  /// <param name="LayoutCount">The number of installed Layouts.</param>
  /// <param name="CoveredLanguageCount">
  /// The number of Languages of the installed Layouts that Navpaky has a profile for.
  /// </param>
  /// <param name="Settings">The main settings the user chose.</param>
  /// <param name="Progress">How far the user got in the welcome window.</param>
  public sealed record SetupSignal(
      Guid RequestId,
      AppEnvironment Environment,
      int LayoutCount,
      int CoveredLanguageCount,
      SettingStates Settings,
      WelcomeProgress Progress) {
    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>The schema version. The default is 1.</value>
    public int SchemaVersion { get; } = 1;
  }
}
