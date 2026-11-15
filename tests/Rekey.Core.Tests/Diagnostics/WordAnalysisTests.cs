using Rekey.Core.Detection;
using Rekey.Core.Tests.Detection;
using Rekey.Core.Words;
using Xunit;

namespace Rekey.Core.Tests.Diagnostics {
  // Spec statistics-and-diagnostics: the word-analysis log records each analysed Word, its
  // Renderings and the Decision, only when the user turned it on.
  public sealed class WordAnalysisTests {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);
    private static readonly Settings _logging = new(AnalysisLogEnabled: true);

    [Fact]
    public void NothingIsLoggedByDefault() {
      Typist typist = new Typist(new Settings()).Type("ghbdtn").Hold(KeyKind.Space);

      Assert.Empty(typist.Analyses);
    }

    [Fact]
    public void AnalysedWordIsLoggedWithItsRenderingsAndDecision() {
      Typist typist = new Typist(_logging).Type("ghbdtn").Hold(KeyKind.Space);

      LogAnalysis analysis = Assert.Single(typist.Analyses);
      Assert.Equal("ghbdtn", analysis.Word);
      Assert.Equal(KeyMapFixtures.EnglishUs.Layout, analysis.Layout);
      Assert.Equal(
          [new TargetRendering(KeyMapFixtures.Russian.Layout, "привет")], analysis.Renderings);
      Assert.Equal(DecisionKind.Correct, analysis.Decision);
      Assert.False(analysis.ProtectedWord);
    }

    [Fact]
    public void LogEntryComesAfterTheReplacement() {
      // The host writes the log synchronously; the replacement must not wait for it.
      Typist typist = new Typist(_logging).Type("ghbdtn").Hold(KeyKind.Space);

      Assert.IsType<LogAnalysis>(typist.All[^1]);
      Assert.Contains(typist.All, action => action is ReplaceText);
    }

    [Fact]
    public void KeptWordIsLogged() {
      Typist typist = new Typist(_logging).Type("hello").Hold(KeyKind.Space);

      Assert.Equal(DecisionKind.Keep, Assert.Single(typist.Analyses).Decision);
    }

    [Fact]
    public void ProtectedWordIsLoggedAsKeptForThatReason() {
      var lists = new WordLists();
      lists.Protect("ntcn", WordSource.User, _monday);

      Typist typist = new Typist(_logging, lists).Type("ntcn").Hold(KeyKind.Space);

      LogAnalysis analysis = Assert.Single(typist.Analyses);
      Assert.Equal(DecisionKind.Keep, analysis.Decision);
      Assert.True(analysis.ProtectedWord);
    }

    [Fact]
    public void TurningTheLogOffStopsLogging() {
      Typist typist = new Typist(_logging).Use(new Settings())
          .Type("ghbdtn").Hold(KeyKind.Space);

      Assert.Empty(typist.Analyses);
    }

    [Fact]
    public void TextFormCarriesNoText() {
      Typist typist = new Typist(_logging).Type("ghbdtn").Hold(KeyKind.Space);

      string text = Assert.Single(typist.Analyses).ToString();

      Assert.DoesNotContain("ghbdtn", text, StringComparison.Ordinal);
      Assert.DoesNotContain("привет", text, StringComparison.Ordinal);
    }
  }
}
