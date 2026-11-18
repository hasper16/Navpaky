using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core.Statistics;

namespace Navpaky.App.ViewModels {
  // The statistics window: today and the last 30 days side by side, read from the worker's
  // latest snapshot of the counters.
  internal sealed class StatisticsViewModel : ObservableObject {
    private const string _none = "—";
    private const int _monthDays = 30;

    private readonly Func<StatsDocument> _read;
    private readonly Func<DateOnly> _today;

    public StatisticsViewModel(Func<StatsDocument> read, Func<DateOnly> today) {
      _read = read;
      _today = today;
      Refresh();
    }

    public IReadOnlyList<StatRow> Rows { get; private set; } = [];

    public void Refresh() {
      StatsDocument stats = _read();
      DateOnly today = _today();
      StatsTotals day = StatsTotals.Of(stats, today, 1);
      StatsTotals month = StatsTotals.Of(stats, today, _monthDays);
      Rows = [
          Counter(Strings.StatWordsAnalysed, StatCounter.WordsAnalysed, day, month),
          Row(Strings.StatCorrections, day, month, totals => Number(totals.Corrections)),
          Counter(Strings.StatAutoCorrections, StatCounter.AutoCorrections, day, month),
          Counter(Strings.StatManualCorrections, StatCounter.ManualCorrections, day, month),
          Row(Strings.StatUndos, day, month, totals => Number(totals.Undos)),
          Row(Strings.StatUndoShare, day, month, totals => Share(totals.UndoShare)),
          Counter(Strings.StatCorrectDecisions, StatCounter.CorrectDecisions, day, month),
          Counter(Strings.StatDeferDecisions, StatCounter.DeferDecisions, day, month),
          Counter(Strings.StatKeepDecisions, StatCounter.KeepDecisions, day, month),
          Counter(
              Strings.StatProtectedWordDecisions, StatCounter.ProtectedWordDecisions, day, month),
          Row(Strings.StatMinDuration, day, month, totals => Ms(totals.Durations.Min)),
          Row(Strings.StatMedianDuration, day, month, totals => Ms(totals.Durations.Median)),
          Row(Strings.StatMaxDuration, day, month, totals => Ms(totals.Durations.Max)),
          Row(Strings.StatAverageDuration, day, month, totals => Ms(totals.Durations.Average)),
      ];
      OnPropertyChanged(nameof(Rows));
    }

    private static StatRow Counter(
        string name, StatCounter counter, StatsTotals day, StatsTotals month) {
      return Row(name, day, month, totals => Number(totals.Count(counter)));
    }

    private static StatRow Row(
        string name, StatsTotals day, StatsTotals month, Func<StatsTotals, string> format) {
      return new StatRow(name, format(day), format(month));
    }

    private static string Number(long value) {
      return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static string Share(double? share) {
      return share is { } value ? value.ToString("P0", CultureInfo.CurrentCulture) : _none;
    }

    private static string Ms(TimeSpan? duration) {
      return duration is { } value
          ? UiText.Format(Strings.StatMilliseconds, Math.Round(value.TotalMilliseconds, 1))
          : _none;
    }
  }
}
