namespace Rekey.Core.Statistics {
  /// <summary>
  /// Holds the stored daily counters, oldest day first.
  /// </summary>
  /// <param name="Days">The days with counters, oldest first, one entry per date.</param>
  public sealed record StatsDocument(IReadOnlyList<DayStats> Days) {
    /// <summary>
    /// Gets the counters of no day.
    /// </summary>
    public static StatsDocument Empty { get; } = new([]);
  }
}
