using Navpaky.Core.Languages;

namespace Navpaky.App.ViewModels {
  // One Learned Word as the list shows it.
  internal sealed record LearnedEntry(string Text, Language Language) {
    // The UI string that names Language, such as LanguageUkrainian.
    public string LanguageKey => $"Language{Language}";
  }
}
