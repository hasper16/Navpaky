using System.Globalization;
using System.Text;
using Navpaky.Core.Statistics;
using Xunit;

namespace Navpaky.Core.Tests.Statistics {
  // Design D2: 30 daily buckets with a duration histogram, kept in stats.json.
  public sealed class StatsStoreTests : IDisposable {
    private static readonly DateOnly _monday = new(2026, 11, 16);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("navpaky-stats-");
    private readonly string _folder;

    public StatsStoreTests() {
      _folder = Path.Join(_root.FullName, "Navpaky");
    }

    public static TheoryData<string> BrokenHistograms() {
      // Bucket counts whose sum overflows, durations beyond what TimeSpan holds, and more
      // Corrections in a day than a person can make, which would overflow 30 days added up.
      string overflow = Histogram(1, 1, 1, 1, [long.MaxValue, 1]);
      string huge = Histogram(1, 1e20, 1e20, 1e20, [0, .. new long[199], 1]);
      string tooMany = Histogram(long.MaxValue, long.MaxValue, 1, 1, [0, long.MaxValue]);
      string tooManyUndos = "{\"version\":1,\"days\":[{\"day\":\"2026-11-16\","
          + "\"counts\":{\"undos\":9223372036854775807}}]}";
      return [overflow, huge, tooMany, tooManyUndos];
    }

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void CountersSurviveARestart() {
      DurationStats durations = DurationStats.Empty
          .Add(TimeSpan.FromMilliseconds(12.5))
          .Add(TimeSpan.FromMilliseconds(250));
      var day = new DayStats(
          _monday,
          new Dictionary<StatCounter, long> {
            [StatCounter.WordsAnalysed] = 40,
            [StatCounter.AutoCorrections] = 3,
            [StatCounter.KeepDecisions] = 37,
          },
          durations);

      new StatsStore(_folder).Save(new StatsDocument([day]));
      StatsDocument reloaded = new StatsStore(_folder).Load();

      DayStats only = Assert.Single(reloaded.Days);
      Assert.Equal(_monday, only.Day);
      Assert.Equal(40, only.Count(StatCounter.WordsAnalysed));
      Assert.Equal(3, only.Count(StatCounter.AutoCorrections));
      Assert.Equal(0, only.Count(StatCounter.Undos));
      Assert.Equal(2, only.Durations.Count);
      Assert.Equal(262.5, only.Durations.TotalMilliseconds);
      Assert.Equal(12.5, only.Durations.MinMilliseconds);
      Assert.Equal(250, only.Durations.MaxMilliseconds);
      Assert.Equal(durations.Buckets, only.Durations.Buckets);
    }

    [Fact]
    public void FileIsNamedStatsJsonAndHoldsNoText() {
      var store = new StatsStore(_folder);
      store.Save(new StatsDocument([DayStats.Empty(_monday).Add(StatCounter.Undos)]));

      Assert.Equal(Path.Join(_folder, "stats.json"), store.FilePath);
      string json = File.ReadAllText(store.FilePath, Encoding.UTF8);
      Assert.Contains("\"undos\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void HistogramHasOneMillisecondBucketsUpTo200Ms() {
      DurationStats durations = DurationStats.Empty
          .Add(TimeSpan.FromMilliseconds(0.4))
          .Add(TimeSpan.FromMilliseconds(199.9))
          .Add(TimeSpan.FromMilliseconds(200))
          .Add(TimeSpan.FromSeconds(3));

      Assert.Equal(201, durations.Buckets.Count);
      Assert.Equal(1, durations.Buckets[0]);
      Assert.Equal(1, durations.Buckets[199]);
      Assert.Equal(2, durations.Buckets[200]);
    }

    [Fact]
    public void MissingFileGivesNoDays() {
      Assert.Empty(new StatsStore(_folder).Load().Days);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("{\"version\":1,\"days\":[{\"day\":\"2026-11-16\",\"counts\":{},\"durations\":"
        + "{\"count\":1,\"totalMilliseconds\":1,\"minMilliseconds\":1,\"maxMilliseconds\":1,"
        + "\"buckets\":[1]}}]}")]
    [InlineData("{\"version\":1,\"days\":[{\"day\":\"2026-11-16\",\"counts\":{\"undos\":-1}}]}")]
    [MemberData(nameof(BrokenHistograms))]
    public void UnreadableFileIsKeptAsideAndGivesNoDays(string content) {
      Directory.CreateDirectory(_folder);
      var store = new StatsStore(_folder);
      File.WriteAllText(store.FilePath, content);

      StatsDocument loaded = store.Load();

      Assert.Empty(loaded.Days);
      Assert.Equal(content, File.ReadAllText(store.FilePath + ".bak"));
    }

    [Fact]
    public void ThirtyDaysAtTheDailyLimitAddUpWithoutOverflow() {
      // A day at the limit loads; a month of them still adds up within long and TimeSpan.
      const long limit = 1_000_000_000;
      string day = Histogram(limit, limit * 86_400_000d, 0, 86_400_000, [limit]);
      Directory.CreateDirectory(_folder);
      var store = new StatsStore(_folder);
      File.WriteAllText(store.FilePath, day);
      DayStats loaded = Assert.Single(store.Load().Days);
      var month = new StatsDocument([.. Enumerable.Range(0, 30).Select(
          offset => loaded with { Day = _monday.AddDays(-offset) })]);

      StatsTotals totals = StatsTotals.Of(month, _monday, days: 30);

      Assert.Equal(30 * limit, totals.Durations.Count);
      Assert.Equal(TimeSpan.FromDays(1), totals.Durations.Average);
    }

    [Fact]
    public void DeleteAlsoRemovesFilesKeptAside() {
      Directory.CreateDirectory(_folder);
      var store = new StatsStore(_folder);
      File.WriteAllText(store.FilePath, "not json");
      store.Load();
      File.WriteAllText(store.FilePath + ".20261116090000000.bak", "{}");
      store.Save(StatsDocument.Empty);

      store.Delete();

      Assert.Empty(Directory.GetFiles(_folder));
    }

    // An idle PC hands the same snapshot every minute; writing it again only wears the disk.
    [Fact]
    public void SavingTheSameSnapshotAgainWritesNothing() {
      var store = new StatsStore(_folder);
      var document = new StatsDocument([DayStats.Empty(_monday)]);
      store.Save(document);
      File.Delete(store.FilePath);

      store.Save(document);

      Assert.False(File.Exists(store.FilePath));
    }

    [Fact]
    public void SavingAfterDeleteWritesTheSameSnapshotAgain() {
      var store = new StatsStore(_folder);
      var document = new StatsDocument([DayStats.Empty(_monday)]);
      store.Save(document);
      store.Delete();

      store.Save(document);

      Assert.True(File.Exists(store.FilePath));
    }

    [Fact]
    public void DeleteRemovesTheFile() {
      var store = new StatsStore(_folder);
      store.Save(new StatsDocument([DayStats.Empty(_monday)]));

      store.Delete();
      store.Delete();

      Assert.False(File.Exists(store.FilePath));
    }

    private static string Histogram(
        long count, double total, double min, double max, long[] firstBuckets) {
      long[] rest = new long[DurationStats.BucketCount - firstBuckets.Length];
      string buckets = string.Join(',', firstBuckets.Concat(rest));
      string durations = string.Create(CultureInfo.InvariantCulture,
          $"{{\"count\":{count},\"totalMilliseconds\":{total:R},")
          + string.Create(CultureInfo.InvariantCulture,
              $"\"minMilliseconds\":{min:R},\"maxMilliseconds\":{max:R},")
          + $"\"buckets\":[{buckets}]}}";
      return "{\"version\":1,\"days\":[{\"day\":\"2026-11-16\",\"counts\":{},"
          + $"\"durations\":{durations}}}]}}";
    }
  }
}
