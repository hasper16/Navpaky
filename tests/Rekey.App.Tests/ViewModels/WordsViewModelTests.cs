using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Languages;
using Rekey.Core.Words;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  public sealed class WordsViewModelTests {
    private static readonly TimeSpan _now = TimeSpan.FromSeconds(42);
    private static readonly DateTimeOffset _used = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly List<WordListEdited> _edits = [];
    private readonly WordsViewModel _model;

    public WordsViewModelTests() {
      _model = new WordsViewModel(_edits.Add, () => _now);
    }

    [Fact]
    public void ShowsTheListsSortedFromTheEngine() {
      _model.Show(new WordListsDocument(
          [
              new ProtectedWord("zulk", WordSource.User, _used),
              new ProtectedWord("brix", WordSource.Auto, _used),
          ],
          [new LearnedWord("кринжик", Language.Russian, WordSource.User, _used)]));

      Assert.Equal(["brix", "zulk"], _model.Protected);
      LearnedEntry learned = Assert.Single(_model.Learned);
      Assert.Equal(("кринжик", Language.Russian), (learned.Text, learned.Language));
    }

    [Fact]
    public void AddingAProtectedWordAsksTheEngine() {
      Assert.True(_model.AddProtected("  vexo "));

      WordListEdited edit = Assert.Single(_edits);
      Assert.Equal(WordListEdited.Protect(_now, "vexo"), edit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("two words")]
    public void AWordWithASpaceOrNothingIsRefused(string text) {
      Assert.False(_model.AddProtected(text));
      Assert.False(_model.AddLearned(text, Language.English));

      Assert.Empty(_edits);
    }

    [Fact]
    public void RemovingAProtectedWordAsksTheEngine() {
      _model.RemoveProtected("vexo");

      Assert.Equal(WordListEdited.Unprotect(_now, "vexo"), Assert.Single(_edits));
    }

    [Fact]
    public void AddingAndRemovingALearnedWordAskTheEngine() {
      _model.AddLearned("плюмбус", Language.Ukrainian);
      _model.RemoveLearned(new LearnedEntry("плюмбус", Language.Ukrainian));

      WordListEdited[] expected = [
          WordListEdited.Learn(_now, "плюмбус", Language.Ukrainian),
          WordListEdited.Unlearn(_now, "плюмбус", Language.Ukrainian)];
      Assert.Equal(expected, _edits);
    }
  }
}
