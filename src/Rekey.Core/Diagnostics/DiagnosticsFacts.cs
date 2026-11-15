using Rekey.Core.Languages;
using Rekey.Core.Statistics;

namespace Rekey.Core.Diagnostics {
  /// <summary>
  /// Holds what a diagnostics export may contain.
  /// </summary>
  /// <remarks>
  /// There is no field for Protected Words, Learned Words or window titles, so an export cannot
  /// hold them; typed text is only in <paramref name="AnalysisLog" />, which is exported only on
  /// request.
  /// </remarks>
  /// <param name="Time">The time of the export.</param>
  /// <param name="AppVersion">The version of Rekey.</param>
  /// <param name="WindowsVersion">The version of Windows.</param>
  /// <param name="Layouts">The installed Layouts, in Windows' order.</param>
  /// <param name="Settings">The user's settings.</param>
  /// <param name="Statistics">The usage counters.</param>
  /// <param name="Incidents">The logged Incidents, oldest first.</param>
  /// <param name="AnalysisLog">The lines of the word-analysis log, oldest first.</param>
  public sealed record DiagnosticsFacts(
      DateTimeOffset Time,
      string AppVersion,
      string WindowsVersion,
      IReadOnlyList<LayoutId> Layouts,
      Settings Settings,
      StatsDocument Statistics,
      IReadOnlyList<IncidentEntry> Incidents,
      IReadOnlyList<string> AnalysisLog);
}
