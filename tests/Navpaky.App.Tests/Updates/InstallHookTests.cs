using System.IO;
using Navpaky.App.Updates;
using Navpaky.Core;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class InstallHookTests : IDisposable {
    private static readonly Guid _attempt = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-hook").FullName;
    private readonly DataFolders _data;
    private readonly string _install;
    private readonly List<(bool On, string Exe)> _signIn = [];
    private readonly FakeShortcut _shortcut = new();

    public InstallHookTests() {
      _data = new DataFolders(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
      _install = Path.Join(_root, "Programs", "Navpaky");
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void WithoutChoicesOnlyTheMarkerIsWritten() {
      Hook().Run();

      Assert.True(File.Exists(Path.Join(_data.Local, InstallMarker.FileName)));
      Assert.Empty(_signIn);
      Assert.Empty(_shortcut.Calls);
      Assert.False(File.Exists(Path.Join(_data.Settings, "settings.json")));
    }

    [Fact]
    public void ANewInstallAppliesEveryChoiceAndAcknowledgesLast() {
      InstallChoices choices = Write(KeptData.New, signIn: false, shortcut: true);

      Hook().Run();

      Settings settings = new SettingsStore(_data.Settings).Load();
      Assert.Equal(UiLanguage.English, settings.UiLanguage);
      Assert.Equal(_install, settings.InstallFolder);
      Assert.True(settings.DesktopShortcut);
      Assert.True(settings.InstallQuestionsAnswered);
      Assert.False(settings.WelcomeCompleted);
      Assert.Equal([(false, Path.Join(_install, "Navpaky.exe"))], _signIn);
      Assert.Equal(["create"], _shortcut.Calls);
      Assert.True(File.Exists(Path.Join(_data.Local, InstallMarker.FileName)));
      Assert.True(File.Exists(Path.Join(_data.Local, choices.AcknowledgementName)));
    }

    [Fact]
    public void NoShortcutRemovesOurs() {
      Write(KeptData.New, signIn: true, shortcut: false);

      Hook().Run();

      Assert.Equal(["remove"], _shortcut.Calls);
      Assert.Equal([(true, Path.Join(_install, "Navpaky.exe"))], _signIn);
    }

    [Fact]
    public void KeepingTheDataKeepsItAndSkipsTheWelcome() {
      KeepSomeData(new Settings(WelcomeCompleted: false, Hotkey: Hotkey.DoubleCtrl));
      Write(KeptData.Keep, signIn: true, shortcut: false);

      Hook().Run();

      Settings settings = new SettingsStore(_data.Settings).Load();
      Assert.Equal(Hotkey.DoubleCtrl, settings.Hotkey);
      Assert.True(settings.WelcomeCompleted);
      Assert.True(File.Exists(Path.Join(_data.Local, "stats.json")));
      Assert.Empty(Backups());
    }

    [Fact]
    public void StartingFreshMovesTheDataAsideWithoutDeletingIt() {
      KeepSomeData(new Settings(WelcomeCompleted: true, Hotkey: Hotkey.DoubleCtrl));
      InstallChoices choices = Write(KeptData.Fresh, signIn: true, shortcut: false);

      Hook().Run();

      Settings settings = new SettingsStore(_data.Settings).Load();
      Assert.Equal(default, settings.Hotkey);
      Assert.False(settings.WelcomeCompleted);
      Assert.False(File.Exists(Path.Join(_data.Local, "stats.json")));
      Assert.True(File.Exists(Path.Join(_data.Local, InstallChoices.FileName)));
      string suffix = $".fresh-{choices.Attempt:N}";
      Assert.True(File.Exists(Path.Join(_data.Settings + suffix, "settings.json")));
      Assert.True(File.Exists(Path.Join(_data.Local + suffix, "stats.json")));
      Assert.True(File.Exists(Path.Join(_data.Local + suffix, InstallMarker.FileName)));
    }

    [Fact]
    public void AFailingStepLeavesNoAcknowledgement() {
      InstallChoices choices = Write(KeptData.New, signIn: true, shortcut: true);
      _shortcut.Fails = true;

      Assert.ThrowsAny<IOException>(() => Hook().Run());

      Assert.False(File.Exists(Path.Join(_data.Local, choices.AcknowledgementName)));
    }

    private InstallHook Hook() {
      return new InstallHook(_data, _install, (on, exe) => _signIn.Add((on, exe)), _shortcut);
    }

    private InstallChoices Write(KeptData data, bool signIn, bool shortcut) {
      var choices = new InstallChoices(
          _attempt, UiLanguage.English, signIn, shortcut, data, _install);
      choices.WriteTo(_data.Local);
      return choices;
    }

    private void KeepSomeData(Settings settings) {
      new SettingsStore(_data.Settings).Save(settings);
      Directory.CreateDirectory(_data.Local);
      File.WriteAllText(Path.Join(_data.Local, "stats.json"), "{}");
      InstallMarker.Write(_data.Local);
    }

    private IEnumerable<string> Backups() {
      return Directory.EnumerateDirectories(_root, "*.fresh-*", SearchOption.AllDirectories);
    }

    private sealed class FakeShortcut : IDesktopShortcut {
      public List<string> Calls { get; } = [];

      public bool Fails { get; set; }

      public void Create(string installFolder) {
        Calls.Add("create");
        if (Fails) {
          throw new IOException("desktop is read-only");
        }
      }

      public void Remove(string installFolder) {
        Calls.Add("remove");
      }
    }
  }
}
