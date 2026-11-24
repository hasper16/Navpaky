using System.IO;
using Navpaky.Core;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class FreshBackupsTests : IDisposable {
    private static readonly Guid _attempt = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-backups").FullName;
    private readonly DataFolderPair _data;

    public FreshBackupsTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void DiscardingDeletesBothBackups() {
      MoveAsideLikeTheHook();

      FreshBackups.Discard(_data, _attempt);

      Assert.Empty(Directory.EnumerateDirectories(_root, "*.fresh-*", SearchOption.AllDirectories));
      Assert.Equal("new", File.ReadAllText(Path.Join(_data.Roaming, "settings.json")));
    }

    [Fact]
    public void RestoringPutsTheKeptDataBackExactly() {
      MoveAsideLikeTheHook();

      FreshBackups.Restore(_data, _attempt);

      Assert.Equal("kept", File.ReadAllText(Path.Join(_data.Roaming, "settings.json")));
      Assert.Equal("kept stats", File.ReadAllText(Path.Join(_data.Local, "stats.json")));
      Assert.Equal("old marker", File.ReadAllText(Path.Join(_data.Local, "install-id.txt")));
      Assert.False(File.Exists(Path.Join(_data.Local, "install-done-x")));
      Assert.True(File.Exists(Path.Join(_data.Local, InstallChoices.FileName)));
      Assert.Empty(Directory.EnumerateDirectories(_root, "*.fresh-*", SearchOption.AllDirectories));
    }

    [Fact]
    public void RestoringWithoutABackupChangesNothing() {
      Write(Path.Join(_data.Roaming, "settings.json"), "current");

      FreshBackups.Restore(_data, _attempt);

      Assert.Equal("current", File.ReadAllText(Path.Join(_data.Roaming, "settings.json")));
    }

    [Fact]
    public void ALeftoverFromACrashedRunIsRestored() {
      MoveAsideLikeTheHook();

      FreshBackups.RestoreLeftovers(_data);

      Assert.Equal("kept", File.ReadAllText(Path.Join(_data.Roaming, "settings.json")));
      Assert.Equal("kept stats", File.ReadAllText(Path.Join(_data.Local, "stats.json")));
      Assert.Empty(Directory.EnumerateDirectories(_root, "*.fresh-*", SearchOption.AllDirectories));
    }

    // The install completed (its acknowledgement is there) but deleting the backup failed: the
    // next start finishes the deletion instead of putting old data over newer.
    [Fact]
    public void ALeftoverOfACompletedInstallIsDiscarded() {
      MoveAsideLikeTheHook();
      Write(Path.Join(_data.Local, $"install-done-{_attempt:N}"), string.Empty);

      FreshBackups.RestoreLeftovers(_data);

      Assert.Equal("new", File.ReadAllText(Path.Join(_data.Roaming, "settings.json")));
      Assert.False(File.Exists(Path.Join(_data.Local, $"install-done-{_attempt:N}")));
      Assert.Empty(Directory.EnumerateDirectories(_root, "*.fresh-*", SearchOption.AllDirectories));
    }

    private static void Write(string path, string text) {
      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      File.WriteAllText(path, text);
    }

    // What InstallHook does for Start fresh, then what it writes afterwards.
    private void MoveAsideLikeTheHook() {
      string suffix = FreshBackups.Suffix(_attempt);
      Write(Path.Join(_data.Roaming + suffix, "settings.json"), "kept");
      Write(Path.Join(_data.Local + suffix, "stats.json"), "kept stats");
      Write(Path.Join(_data.Local + suffix, "install-id.txt"), "old marker");
      Write(Path.Join(_data.Roaming, "settings.json"), "new");
      Write(Path.Join(_data.Local, "install-id.txt"), "new marker");
      Write(Path.Join(_data.Local, "install-done-x"), string.Empty);
      Write(Path.Join(_data.Local, InstallChoices.FileName), "{}");
    }
  }
}
