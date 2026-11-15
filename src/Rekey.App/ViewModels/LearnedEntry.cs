using Rekey.Core.Languages;

namespace Rekey.App.ViewModels {
  // One Learned Word as the list shows it.
  internal sealed record LearnedEntry(string Text, Language Language);
}
