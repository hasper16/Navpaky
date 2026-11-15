namespace Rekey.Core.Statistics {
  /// <summary>
  /// Keeps the daily counters in memory and publishes them as immutable snapshots.
  /// </summary>
  /// <remarks>
  /// One thread records and clears; any thread may read <see cref="Current" />.
  /// </remarks>
  public sealed class StatsRecorder {
    // Design D2: 30 daily buckets.
    private const int _keptDays = 30;

    private StatsDocument _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsRecorder" /> class.
    /// </summary>
    /// <param name="stored">The counters read from storage.</param>
    public StatsRecorder(StatsDocument stored) {
      _current = stored;
    }

    /// <summary>
    /// Gets the counters as of the latest record.
    /// </summary>
    public StatsDocument Current => Volatile.Read(ref _current);

    /// <summary>
    /// Counts one event in a day's counters and forgets days more than 30 days before it.
    /// </summary>
    /// <param name="stat">The event to count.</param>
    /// <param name="today">The local date.</param>
    public void Record(RecordStat stat, DateOnly today) {
      DayStats day = _current.Days.LastOrDefault(stats => stats.Day == today)
          ?? DayStats.Empty(today);
      day = day.Add(stat.Counter);
      if (stat.Duration is { } duration) {
        day = day with { Durations = day.Durations.Add(duration) };
      }

      DateOnly oldest = today.AddDays(1 - _keptDays);
      DayStats[] days = [
          .. _current.Days.Where(stats => stats.Day >= oldest && stats.Day != today),
          day,
      ];
      Volatile.Write(ref _current, new StatsDocument([.. days.OrderBy(stats => stats.Day)]));
    }

    /// <summary>
    /// Forgets every counter.
    /// </summary>
    public void Clear() {
      Volatile.Write(ref _current, StatsDocument.Empty);
    }
  }
}
