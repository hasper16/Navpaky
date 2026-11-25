using System.IO;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Words;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  // Task 6.4 of add-install-wizard, design D7: what counts as data kept from an earlier install.
  public sealed class KeptItemsTests : IDisposable {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-kept").FullName;
    private readonly DataFolderPair _data;

    public KeptItemsTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void NoDataIsNotKept() {
      Assert.Null(KeptItems.Find(_data));
    }

    [Fact]
    public void TheWizardsOwnFilesAreNotKeptData() {
      Directory.CreateDirectory(_data.Local);
      File.WriteAllText(Path.Join(_data.Local, InstallChoices.FileName), "{}");
      File.WriteAllText(Path.Join(_data.Local, $"install-done-{Guid.NewGuid():N}"), string.Empty);

      Assert.Null(KeptItems.Find(_data));
    }

    [Fact]
    public void SettingsAreKeptWithTheirDetails() {
      var settings = new Settings {
        Hotkey = Hotkey.DoubleCtrl,
        ExcludedApps = new ExcludedAppList(["zorbel.exe", "quindle.exe"]),
      };
      new SettingsStore(_data.Roaming).Save(settings);
      new WordListsStore(_data.Roaming).Save(new WordListsDocument(
          [], [new LearnedWord("флумп", Language.Ukrainian, WordSource.Auto, _monday)]));
      Directory.CreateDirectory(_data.Local);
      File.WriteAllText(Path.Join(_data.Local, "stats.json"), "{}");

      KeptItems? kept = KeptItems.Find(_data);

      Assert.Equal(settings, kept?.Settings);
      Assert.Equal(1, kept?.LearnedWords);
      Assert.True(kept?.Statistics);
    }

    [Fact]
    public void ACorruptSettingsFileIsKeptWithoutDetailsAndLeftAsItIs() {
      Directory.CreateDirectory(_data.Roaming);
      File.WriteAllText(Path.Join(_data.Roaming, "settings.json"), "{ not json");

      KeptItems? kept = KeptItems.Find(_data);

      Assert.NotNull(kept);
      Assert.Null(kept.Settings);
      Assert.True(kept.SettingsUnreadable);
      Assert.Equal(["settings.json"], Directory.GetFiles(_data.Roaming).Select(Path.GetFileName));
    }

    // Found in the 2026-11-25 VM run: a lone folder in the local data folder read as an
    // unreadable settings file.
    [Fact]
    public void LocalStateAloneIsKeptWithoutClaimingUnreadableSettings() {
      Directory.CreateDirectory(_data.Local);
      File.WriteAllText(Path.Join(_data.Local, "incidents.jsonl"), string.Empty);

      KeptItems? kept = KeptItems.Find(_data);

      Assert.NotNull(kept);
      Assert.False(kept.Statistics);
      Assert.False(kept.SettingsUnreadable);
    }
  }
}
