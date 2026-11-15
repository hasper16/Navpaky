namespace Rekey.Core.Statistics {
  /// <summary>
  /// Holds the counters of one local day; they never contain text.
  /// </summary>
  /// <param name="Day">The local date the counters belong to.</param>
  /// <param name="Counts">The counters that are not zero.</param>
  /// <param name="Durations">The replacement durations of the day's Corrections.</param>
  public sealed record DayStats(
      DateOnly Day, IReadOnlyDictionary<StatCounter, long> Counts, DurationStats Durations) {
    /// <summary>
    /// Returns the counters of a day with nothing counted yet.
    /// </summary>
    /// <param name="day">The local date.</param>
    /// <returns>The empty counters.</returns>
    public static DayStats Empty(DateOnly day) {
      return new DayStats(day, new Dictionary<StatCounter, long>(), DurationStats.Empty);
    }

    /// <summary>
    /// Returns the value of one counter.
    /// </summary>
    /// <param name="counter">One of the enumeration values that specifies the counter.</param>
    /// <returns>The value; 0 when nothing was counted.</returns>
    public long Count(StatCounter counter) {
      return Counts.GetValueOrDefault(counter);
    }

    /// <summary>
    /// Returns these counters with one counter increased by one.
    /// </summary>
    /// <param name="counter">One of the enumeration values that specifies the counter.</param>
    /// <returns>The new counters.</returns>
    public DayStats Add(StatCounter counter) {
      var counts = new Dictionary<StatCounter, long>(Counts);
      counts[counter] = Count(counter) + 1;
      return this with { Counts = counts };
    }
  }
}
