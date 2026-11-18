namespace Navpaky.Core.Telemetry.Schemas {
  /// <summary>
  /// Holds the versions and languages every Usage Digest and setup signal carries.
  /// </summary>
  /// <param name="AppVersion">The Navpaky version.</param>
  /// <param name="OsVersion">The Windows version, such as 10.0.26100.</param>
  /// <param name="UiLanguage">The UI language as a culture name, such as en-US.</param>
  /// <param name="LayoutLanguages">
  /// The distinct language codes of the installed Layouts, such as en and ru.
  /// </param>
  public sealed record AppEnvironment(
      string AppVersion,
      string OsVersion,
      string UiLanguage,
      IReadOnlyList<string> LayoutLanguages);
}
