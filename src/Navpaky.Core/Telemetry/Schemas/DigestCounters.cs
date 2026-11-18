using Navpaky.Core.Statistics;

namespace Navpaky.Core.Telemetry.Schemas {
  /// <summary>
  /// Holds the counters of one day that the Usage Digest reports.
  /// </summary>
  /// <param name="Analysed">The number of evaluated Words.</param>
  /// <param name="AutoCorrections">The number of Auto Corrections.</param>
  /// <param name="ManualCorrections">The number of Manual Corrections.</param>
  /// <param name="Undos">The number of Undos.</param>
  /// <param name="CorrectDecisions">The number of Decisions to correct.</param>
  /// <param name="DeferDecisions">The number of Decisions to defer.</param>
  /// <param name="KeepDecisions">The number of Decisions to keep.</param>
  /// <param name="ProtectedDecisions">The number of Words kept as Protected Words.</param>
  public sealed record DigestCounters(
      long Analysed,
      long AutoCorrections,
      long ManualCorrections,
      long Undos,
      long CorrectDecisions,
      long DeferDecisions,
      long KeepDecisions,
      long ProtectedDecisions) {
    /// <summary>
    /// Returns the counters of one day.
    /// </summary>
    /// <param name="day">The day's statistics.</param>
    /// <returns>The counters.</returns>
    public static DigestCounters Of(DayStats day) {
      return new DigestCounters(
          day.Count(StatCounter.WordsAnalysed),
          day.Count(StatCounter.AutoCorrections),
          day.Count(StatCounter.ManualCorrections),
          day.Count(StatCounter.Undos),
          day.Count(StatCounter.CorrectDecisions),
          day.Count(StatCounter.DeferDecisions),
          day.Count(StatCounter.KeepDecisions),
          day.Count(StatCounter.ProtectedWordDecisions));
    }
  }
}
