using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Words;

namespace Navpaky.App.ViewModels {
  // The Protected Words and Learned Words editors. The engine owns the lists (design D5 of
  // add-word-learning): edits go to it as events and the lists come back as snapshots.
  internal sealed class WordsViewModel : ObservableObject {
    private readonly Action<WordListEdited> _edit;
    private readonly Func<TimeSpan> _now;

    public WordsViewModel(Action<WordListEdited> edit, Func<TimeSpan> now) {
      _edit = edit;
      _now = now;
    }

    public IReadOnlyList<string> Protected { get; private set; } = [];

    public IReadOnlyList<LearnedEntry> Learned { get; private set; } = [];

    public void Show(WordListsDocument lists) {
      Protected = [.. lists.Protected.Select(word => word.Text)
          .Order(StringComparer.CurrentCultureIgnoreCase)];
      Learned = [.. lists.Learned
          .Select(word => new LearnedEntry(word.Text, word.Language))
          .OrderBy(word => word.Text, StringComparer.CurrentCultureIgnoreCase)];
      OnPropertyChanged(string.Empty);
    }

    public bool AddProtected(string text) {
      return Send(text, word => WordListEdited.Protect(_now(), word));
    }

    public void RemoveProtected(string text) {
      _edit(WordListEdited.Unprotect(_now(), text));
    }

    public bool AddLearned(string text, Language language) {
      return Send(text, word => WordListEdited.Learn(_now(), word, language));
    }

    public void RemoveLearned(LearnedEntry word) {
      _edit(WordListEdited.Unlearn(_now(), word.Text, word.Language));
    }

    // A word is one run of characters: Navpaky compares Words, which end at a space.
    private bool Send(string text, Func<string, WordListEdited> edit) {
      string word = text.Trim();
      if (word.Length == 0 || word.Any(char.IsWhiteSpace)) {
        return false;
      }

      _edit(edit(word));
      return true;
    }
  }
}
