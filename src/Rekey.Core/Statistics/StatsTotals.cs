namespace Rekey.Core.Statistics {
  /// <summary>
  /// Holds the counters of a range of days added together, as the statistics window shows them.
  /// </summary>
  /// <param name="Counts">The value of every counter.</param>
  /// <param name="Durations">The replacement durations of the range.</param>
  public sealed record StatsTotals(
      IReadOnlyDictionary<StatCounter, long> Counts, DurationStats Durations) {
    /// <summary>
    /// Gets the number of Corrections applied, automatic and manual.
    /// </summary>
    public long Corrections =>
        Count(StatCounter.AutoCorrections) + Count(StatCounter.ManualCorrections);

    /// <summary>
    /// Gets the number of Undos.
    /// </summary>
    public long Undos => Count(StatCounter.Undos);

    /// <summary>
    /// Gets the share of Corrections that were undone, or <see langword="null" /> when there
    /// were no Corrections.
    /// </summary>
    /// <value>A value from 0 up; above 1 when Undos outnumber the Corrections in range.</value>
    public double? UndoShare => Corrections == 0 ? null : (double)Undos / Corrections;

    /// <summary>
    /// Returns the totals of the days that end today.
    /// </summary>
    /// <param name="document">The stored counters.</param>
    /// <param name="today">The local date.</param>
    /// <param name="days">The number of days, today included.</param>
    /// <returns>The totals.</returns>
    public static StatsTotals Of(StatsDocument document, DateOnly today, int days) {
      DateOnly first = today.AddDays(1 - days);
      DayStats[] range = [.. document.Days.Where(day => day.Day >= first && day.Day <= today)];
      Dictionary<StatCounter, long> counts = Enum.GetValues<StatCounter>()
          .ToDictionary(counter => counter, counter => range.Sum(day => day.Count(counter)));
      DurationStats durations = range.Aggregate(
          DurationStats.Empty, (sum, day) => sum.Merge(day.Durations));
      return new StatsTotals(counts, durations);
    }

    /// <summary>
    /// Returns the value of one counter.
    /// </summary>
    /// <param name="counter">One of the enumeration values that specifies the counter.</param>
    /// <returns>The value.</returns>
    public long Count(StatCounter counter) {
      return Counts.GetValueOrDefault(counter);
    }
  }
}
