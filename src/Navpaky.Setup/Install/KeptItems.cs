using System.IO;
using Navpaky.Core;
using Navpaky.Core.Words;

namespace Navpaky.Setup.Install {
  // Navpaky data kept from an earlier install, read without changing it (design D7 of
  // add-install-wizard). Settings is null when settings.json is missing or unreadable.
  internal sealed record KeptItems(Settings? Settings, int? LearnedWords, bool Statistics) {
    private const string _settingsFile = "settings.json";
    private const string _statisticsFile = "stats.json";

    public static KeptItems? Find(DataFolderPair data) {
      bool settings = File.Exists(Path.Join(data.Roaming, _settingsFile));
      if (!settings && !HasLocalState(data.Local)) {
        return null;
      }

      return new KeptItems(
          new SettingsStore(data.Roaming).Peek(),
          new WordListsStore(data.Roaming).Peek()?.Learned.Count,
          File.Exists(Path.Join(data.Local, _statisticsFile)));
    }

    // The wizard's own hand-off files are not the user's data.
    private static bool HasLocalState(string folder) {
      return Directory.Exists(folder)
          && Directory.EnumerateFileSystemEntries(folder)
              .Select(Path.GetFileName)
              .Any(name => name != InstallChoices.FileName
                  && name?.StartsWith("install-done-", StringComparison.Ordinal) != true);
    }
  }
}
