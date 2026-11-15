using System.Globalization;
using Rekey.App.ViewModels;
using Rekey.Core.Statistics;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  // Spec statistics-and-diagnostics: the window shows Words analysed, Corrections, Undos and the
  // undo share for today and the last 30 days, the Decision breakdown and the durations.
  public sealed class StatisticsViewModelTests : IDisposable {
    private static readonly DateOnly _monday = new(2026, 11, 16);

    private readonly CultureInfo _culture = CultureInfo.CurrentCulture;

    public StatisticsViewModelTests() {
      CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public void Dispose() {
      CultureInfo.CurrentCulture = _culture;
    }

    [Fact]
    public void RowsShowTodayAndTheLast30Days() {
      StatsDocument stats = Seeded();
      var model = new StatisticsViewModel(() => stats, () => _monday);

      Assert.Equal(("10", "30"), Row(model, Strings.StatWordsAnalysed));
      Assert.Equal(("2", "6"), Row(model, Strings.StatCorrections));
      Assert.Equal(("1", "2"), Row(model, Strings.StatUndos));
      Assert.Equal(("50 %", "33 %"), Row(model, Strings.StatUndoShare));
      Assert.Equal(("8", "24"), Row(model, Strings.StatKeepDecisions));
      Assert.Equal(("20 ms", "20 ms"), Row(model, Strings.StatMedianDuration));
    }

    [Fact]
    public void EmptyStatisticsShowDashes() {
      var model = new StatisticsViewModel(() => StatsDocument.Empty, () => _monday);

      Assert.Equal(("0", "0"), Row(model, Strings.StatCorrections));
      Assert.Equal(("—", "—"), Row(model, Strings.StatUndoShare));
      Assert.Equal(("—", "—"), Row(model, Strings.StatMedianDuration));
    }

    [Fact]
    public void RefreshReadsTheLatestCounters() {
      StatsDocument stats = StatsDocument.Empty;
      var model = new StatisticsViewModel(() => stats, () => _monday);
      stats = Seeded();

      model.Refresh();

      Assert.Equal(("10", "30"), Row(model, Strings.StatWordsAnalysed));
    }

    private static (string Today, string Month) Row(StatisticsViewModel model, string name) {
      StatRow row = Assert.Single(model.Rows, row => row.Name == name);
      return (row.Today, row.Month);
    }

    // Three days alike: 10 Words, 8 kept, 2 Auto Corrections of 20 ms; one Undo today and one
    // on the day before. A day 40 days ago is outside both ranges.
    private static StatsDocument Seeded() {
      DayStats[] days = [.. new[] { -40, -2, -1, 0 }.Select(offset => {
        DayStats day = DayStats.Empty(_monday.AddDays(offset));
        for (int i = 0; i < 10; i++) {
          day = day.Add(StatCounter.WordsAnalysed);
        }

        for (int i = 0; i < 8; i++) {
          day = day.Add(StatCounter.KeepDecisions);
        }

        for (int i = 0; i < 2; i++) {
          day = day.Add(StatCounter.AutoCorrections);
          day = day with { Durations = day.Durations.Add(TimeSpan.FromMilliseconds(20)) };
        }

        return offset is 0 or -1 ? day.Add(StatCounter.Undos) : day;
      })];
      return new StatsDocument(days);
    }
  }
}
