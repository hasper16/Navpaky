using System.Text.Json;

namespace Rekey.Core.Statistics {
  // The stats.json contract: counters are named in camelCase so the file stays readable, and a
  // file that breaks the contract is not read at all.
  internal static class StatsJson {
    private const int _version = 1;

    // Durations beyond a day are not real replacements; the bound keeps every total within
    // what TimeSpan holds.
    private static readonly double _maxMilliseconds = TimeSpan.FromDays(1).TotalMilliseconds;

    public static byte[] Write(StatsDocument document) {
      var file = new StatsFile(_version, [.. document.Days.Select(ToFile)]);
      return JsonSerializer.SerializeToUtf8Bytes(file, StatsJsonContext.Default.StatsFile);
    }

    // Null when the text is not a valid version 1 file.
    public static StatsDocument? Parse(ReadOnlySpan<byte> json) {
      StatsFile? file;
      try {
        file = JsonSerializer.Deserialize(json, StatsJsonContext.Default.StatsFile);
      } catch (JsonException) {
        return null;
      }

      if (file is not { Version: _version, Days: { } days }) {
        return null;
      }

      var parsed = new List<DayStats>(days.Count);
      foreach (DayFile? day in days) {
        if (day is null || FromFile(day) is not { } stats) {
          return null;
        }

        parsed.Add(stats);
      }

      return new StatsDocument([.. parsed.DistinctBy(day => day.Day).OrderBy(day => day.Day)]);
    }

    private static DayFile ToFile(DayStats day) {
      Dictionary<string, long> counts = day.Counts
          .Where(pair => pair.Value != 0)
          .ToDictionary(pair => JsonNamingPolicy.CamelCase.ConvertName(pair.Key.ToString()),
              pair => pair.Value);
      DurationStats d = day.Durations;
      return new DayFile(
          day.Day,
          counts,
          new DurationFile(
              d.Count, d.TotalMilliseconds, d.MinMilliseconds, d.MaxMilliseconds, [.. d.Buckets]));
    }

    private static DayStats? FromFile(DayFile day) {
      var counts = new Dictionary<StatCounter, long>();
      foreach ((string name, long value) in day.Counts ?? []) {
        if (value < 0) {
          return null;
        }

        // A counter a later version added is left out.
        if (Enum.TryParse(name, ignoreCase: true, out StatCounter counter)
            && Enum.IsDefined(counter)) {
          counts[counter] = value;
        }
      }

      DurationStats? durations = day.Durations is null
          ? DurationStats.Empty
          : FromFile(day.Durations);
      return durations is null ? null : new DayStats(day.Day, counts, durations);
    }

    private static DurationStats? FromFile(DurationFile file) {
      if (file.Buckets is not { Count: DurationStats.BucketCount } buckets
          || buckets.Any(count => count < 0)
          || SumOrNull(buckets) != file.Count
          || file.TotalMilliseconds < 0
          || file.TotalMilliseconds > _maxMilliseconds * file.Count
          || file.MinMilliseconds < 0
          || file.MaxMilliseconds < file.MinMilliseconds
          || file.MaxMilliseconds > _maxMilliseconds) {
        return null;
      }

      return new DurationStats(
          file.Count, file.TotalMilliseconds, file.MinMilliseconds, file.MaxMilliseconds,
          [.. buckets]);
    }

    private static long? SumOrNull(List<long> counts) {
      try {
        return counts.Sum();
      } catch (OverflowException) {
        return null;
      }
    }

    internal sealed record StatsFile(int Version, List<DayFile?>? Days);

    internal sealed record DayFile(
        DateOnly Day, Dictionary<string, long>? Counts, DurationFile? Durations);

    internal sealed record DurationFile(
        long Count,
        double TotalMilliseconds,
        double MinMilliseconds,
        double MaxMilliseconds,
        List<long>? Buckets);
  }
}
