namespace Navpaky.Core.Statistics {
  /// <summary>
  /// Holds the replacement durations of one day as a histogram, so the median needs no samples.
  /// </summary>
  /// <param name="Count">The number of durations.</param>
  /// <param name="TotalMilliseconds">The sum of the durations, in milliseconds.</param>
  /// <param name="MinMilliseconds">The shortest duration, in milliseconds; 0 when empty.</param>
  /// <param name="MaxMilliseconds">The longest duration, in milliseconds; 0 when empty.</param>
  /// <param name="Buckets">
  /// The number of durations in each 1 ms bucket below 200 ms, then the number of 200 ms or more.
  /// </param>
  public sealed record DurationStats(
      long Count,
      double TotalMilliseconds,
      double MinMilliseconds,
      double MaxMilliseconds,
      IReadOnlyList<long> Buckets) {
    /// <summary>
    /// The number of buckets: 200 of 1 ms and one for 200 ms or more (design D2).
    /// </summary>
    public const int BucketCount = 201;

    /// <summary>
    /// Gets the durations of a day without any.
    /// </summary>
    public static DurationStats Empty { get; } = new(0, 0, 0, 0, new long[BucketCount]);

    /// <summary>
    /// Gets the average duration, or <see langword="null" /> when there is none.
    /// </summary>
    public TimeSpan? Average =>
        Count == 0 ? null : TimeSpan.FromMilliseconds(TotalMilliseconds / Count);

    /// <summary>
    /// Gets the shortest duration, or <see langword="null" /> when there is none.
    /// </summary>
    public TimeSpan? Min => Count == 0 ? null : TimeSpan.FromMilliseconds(MinMilliseconds);

    /// <summary>
    /// Gets the longest duration, or <see langword="null" /> when there is none.
    /// </summary>
    public TimeSpan? Max => Count == 0 ? null : TimeSpan.FromMilliseconds(MaxMilliseconds);

    /// <summary>
    /// Gets the median duration to the nearest bucket, or <see langword="null" /> when there is
    /// none.
    /// </summary>
    /// <value>
    /// The lower bound of the bucket that holds the middle duration, kept between the shortest
    /// and the longest duration.
    /// </value>
    public TimeSpan? Median {
      get {
        if (Count == 0) {
          return null;
        }

        long middle = (Count + 1) / 2;
        long seen = 0;
        int bucket = 0;
        while (seen + Buckets[bucket] < middle) {
          seen += Buckets[bucket];
          bucket++;
        }

        double milliseconds = Math.Clamp(bucket, MinMilliseconds, MaxMilliseconds);
        return TimeSpan.FromMilliseconds(milliseconds);
      }
    }

    /// <summary>
    /// Returns these durations with one more.
    /// </summary>
    /// <param name="duration">The duration to add.</param>
    /// <returns>The durations with <paramref name="duration" /> added.</returns>
    public DurationStats Add(TimeSpan duration) {
      double milliseconds = Math.Max(0, duration.TotalMilliseconds);
      long[] buckets = [.. Buckets];
      buckets[BucketOf(milliseconds)]++;
      return new DurationStats(
          Count + 1,
          TotalMilliseconds + milliseconds,
          Count == 0 ? milliseconds : Math.Min(MinMilliseconds, milliseconds),
          Math.Max(MaxMilliseconds, milliseconds),
          buckets);
    }

    /// <summary>
    /// Returns these durations together with another day's.
    /// </summary>
    /// <param name="other">The other durations.</param>
    /// <returns>The durations of both.</returns>
    public DurationStats Merge(DurationStats other) {
      if (other.Count == 0) {
        return this;
      }

      if (Count == 0) {
        return other;
      }

      return new DurationStats(
          Count + other.Count,
          TotalMilliseconds + other.TotalMilliseconds,
          Math.Min(MinMilliseconds, other.MinMilliseconds),
          Math.Max(MaxMilliseconds, other.MaxMilliseconds),
          [.. Buckets.Zip(other.Buckets, (a, b) => a + b)]);
    }

    private static int BucketOf(double milliseconds) {
      return (int)Math.Min(Math.Floor(milliseconds), BucketCount - 1);
    }
  }
}
