using Navpaky.Core.Detection;
using Navpaky.Core.Statistics;
using Navpaky.Core.Tests.Detection;
using Navpaky.Core.Words;
using Xunit;

namespace Navpaky.Core.Tests.Statistics {
  // Spec statistics-and-diagnostics: the engine counts Words analysed, Corrections, Undos and
  // Decisions, and nothing while statistics are off.
  public sealed class CountingTests {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AutoCorrectionIsCountedWithItsDuration() {
      Typist typist = Default().Type("ghbdtn").Hold(KeyKind.Space).Finish();

      // Typist's events are 100 ms apart: the Space, then the finished replacement.
      Assert.Equal(
          [
            new RecordStat(StatCounter.WordsAnalysed),
            new RecordStat(StatCounter.CorrectDecisions),
            new RecordStat(StatCounter.AutoCorrections, TimeSpan.FromMilliseconds(100)),
          ],
          typist.Stats);
    }

    [Fact]
    public void CancelledCorrectionIsNotCounted() {
      Typist typist = Default().Type("ghbdtn").Hold(KeyKind.Space).Finish(applied: false);

      Assert.DoesNotContain(typist.Stats, stat => stat.Counter == StatCounter.AutoCorrections);
    }

    [Fact]
    public void KeptWordIsCountedAsAnalysedAndKept() {
      Typist typist = Default().Type("hello").Hold(KeyKind.Space);

      Assert.Equal(
          [new RecordStat(StatCounter.WordsAnalysed), new RecordStat(StatCounter.KeepDecisions)],
          typist.Stats);
    }

    [Fact]
    public void ProtectedWordIsCountedAsKeptForThatReason() {
      var lists = new WordLists();
      lists.Protect("ntcn", WordSource.User, _monday);

      Typist typist = new Typist(new Settings(), lists).Type("ntcn").Hold(KeyKind.Space);

      Assert.Equal(
          [
            new RecordStat(StatCounter.WordsAnalysed),
            new RecordStat(StatCounter.ProtectedWordDecisions),
          ],
          typist.Stats);
    }

    [Fact]
    public void DeferredWordIsCountedAsDeferred() {
      Typist typist = new Typist(
          (word, _) => KeyMapFixtures.EnglishUs.Render(word) == "yt"
              ? Decision.Defer(KeyMapFixtures.Russian.Layout, "не")
              : Decision.Keep)
          .Type("yt").Hold(KeyKind.Space);

      Assert.Contains(new RecordStat(StatCounter.DeferDecisions), typist.Stats);
    }

    [Fact]
    public void ManualCorrectionIsCounted() {
      Typist typist = Default().Type("ghbdtn").Hotkey().Finish();

      Assert.Equal(
          [new RecordStat(StatCounter.ManualCorrections, TimeSpan.FromMilliseconds(100))],
          typist.Stats);
    }

    [Fact]
    public void ConvertedSelectionIsCountedAsManualCorrection() {
      Typist typist = Default().Hotkey("ghbdtn").Finish();

      Assert.Equal(StatCounter.ManualCorrections, Assert.Single(typist.Stats).Counter);
    }

    [Fact]
    public void UndoIsCounted() {
      Typist typist = Default().Type("ghbdtn").Hold(KeyKind.Space).Finish();
      typist.Stats.Clear();

      typist.RequestUndo().Finish();

      Assert.Equal([new RecordStat(StatCounter.Undos)], typist.Stats);
    }

    [Fact]
    public void NothingIsCountedWhileStatisticsAreOff() {
      Typist typist = new Typist(new Settings(StatisticsEnabled: false))
          .Type("ghbdtn").Hold(KeyKind.Space).Finish()
          .RequestUndo().Finish()
          .Type("hello").Hold(KeyKind.Space)
          .Type("ghbdtn").Hotkey().Finish();

      Assert.Empty(typist.Stats);
    }

    [Fact]
    public void CountingStopsWhenTheUserTurnsStatisticsOff() {
      Typist typist = Default().Use(new Settings(StatisticsEnabled: false))
          .Type("ghbdtn").Hold(KeyKind.Space).Finish();

      Assert.Empty(typist.Stats);
    }

    private static Typist Default() {
      return new Typist(new Settings());
    }
  }
}
