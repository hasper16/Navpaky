using Navpaky.Core.Statistics;
using Xunit;

namespace Navpaky.Core.Tests.Statistics {
  // Design D2: today's bucket in memory, 30 daily buckets, the median from the histogram.
  public sealed class StatsRecorderTests {
    private static readonly DateOnly _monday = new(2026, 11, 16);

    [Fact]
    public void AutoCorrectionIncreasesTodaysCount() {
      var recorder = new StatsRecorder(StatsDocument.Empty);

      recorder.Record(new RecordStat(StatCounter.AutoCorrections, Ms(20)), _monday);
      recorder.Record(new RecordStat(StatCounter.AutoCorrections, Ms(30)), _monday);

      DayStats today = Assert.Single(recorder.Current.Days);
      Assert.Equal(_monday, today.Day);
      Assert.Equal(2, today.Count(StatCounter.AutoCorrections));
      Assert.Equal(2, today.Durations.Count);
    }

    [Fact]
    public void ANewDayGetsItsOwnBucket() {
      var recorder = new StatsRecorder(StatsDocument.Empty);

      recorder.Record(new RecordStat(StatCounter.Undos), _monday);
      recorder.Record(new RecordStat(StatCounter.Undos), _monday.AddDays(1));

      Assert.Equal([1, 1], recorder.Current.Days.Select(day => day.Count(StatCounter.Undos)));
    }

    [Fact]
    public void OnlyTheLast30DaysAreKept() {
      var recorder = new StatsRecorder(new StatsDocument([
          DayStats.Empty(_monday.AddDays(-30)).Add(StatCounter.Undos),
          DayStats.Empty(_monday.AddDays(-29)).Add(StatCounter.Undos),
      ]));

      recorder.Record(new RecordStat(StatCounter.Undos), _monday);

      Assert.Equal(
          [_monday.AddDays(-29), _monday], recorder.Current.Days.Select(day => day.Day));
    }

    [Fact]
    public void ClearForgetsEveryCounter() {
      var recorder = new StatsRecorder(
          new StatsDocument([DayStats.Empty(_monday).Add(StatCounter.Undos)]));

      recorder.Clear();

      Assert.Empty(recorder.Current.Days);
    }

    [Fact]
    public void MedianComesFromTheHistogram() {
      DurationStats durations = DurationStats.Empty.Add(Ms(3.7)).Add(Ms(12.2)).Add(Ms(90));

      Assert.Equal(Ms(12), durations.Median);
      Assert.Equal(Ms(3.7), durations.Min);
      Assert.Equal(Ms(90), durations.Max);
      Assert.Equal(Ms(35.3).TotalMilliseconds, durations.Average!.Value.TotalMilliseconds, 6);
    }

    [Fact]
    public void MedianOfLongDurationsStaysWithinTheLongest() {
      DurationStats durations = DurationStats.Empty.Add(Ms(450)).Add(Ms(500)).Add(Ms(700));

      Assert.Equal(Ms(450), durations.Median);
    }

    [Fact]
    public void NoDurationsHaveNoMedian() {
      Assert.Null(DurationStats.Empty.Median);
    }

    [Fact]
    public void SummaryAddsTheDaysInRangeAndTheUndoShare() {
      var document = new StatsDocument([
          DayStats.Empty(_monday.AddDays(-30)).Add(StatCounter.Undos),
          Day(_monday.AddDays(-1), auto: 3, manual: 1, undos: 1),
          Day(_monday, auto: 1, manual: 0, undos: 1),
      ]);

      StatsTotals today = StatsTotals.Of(document, _monday, days: 1);
      StatsTotals month = StatsTotals.Of(document, _monday, days: 30);

      Assert.Equal(1, today.Corrections);
      Assert.Equal(1.0, today.UndoShare);
      Assert.Equal(5, month.Corrections);
      Assert.Equal(2, month.Undos);
      Assert.Equal(0.4, month.UndoShare);
      Assert.Equal(4, month.Durations.Count);
    }

    [Fact]
    public void NoCorrectionsHaveNoUndoShare() {
      Assert.Null(StatsTotals.Of(StatsDocument.Empty, _monday, days: 30).UndoShare);
    }

    private static DayStats Day(DateOnly day, int auto, int manual, int undos) {
      DayStats stats = DayStats.Empty(day);
      for (int i = 0; i < auto; i++) {
        stats = stats.Add(StatCounter.AutoCorrections);
        stats = stats with { Durations = stats.Durations.Add(Ms(10)) };
      }

      for (int i = 0; i < manual; i++) {
        stats = stats.Add(StatCounter.ManualCorrections);
      }

      for (int i = 0; i < undos; i++) {
        stats = stats.Add(StatCounter.Undos);
      }

      return stats;
    }

    private static TimeSpan Ms(double milliseconds) {
      return TimeSpan.FromMilliseconds(milliseconds);
    }
  }
}
