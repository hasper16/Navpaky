using System.IO;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class InstallTargetTests {
    private const long _needed = 230_000_000;
    private const string _roaming = @"C:\Users\olena\AppData\Roaming\Navpaky";
    private const string _local = @"C:\Users\olena\AppData\Local\Navpaky";
    private const string _machine = @"C:\Program Files\Navpaky";

    private readonly FakeFolders _folders = new();

    [Theory]
    [InlineData(@"D:\Tools", @"D:\Tools\Navpaky")]
    [InlineData(@"D:\Tools\", @"D:\Tools\Navpaky")]
    [InlineData(@"D:\Apps\Navpaky", @"D:\Apps\Navpaky")]
    [InlineData(@"D:\Apps\NAVPAKY", @"D:\Apps\NAVPAKY")]
    [InlineData(@"D:\", @"D:\Navpaky")]
    public void APickGetsItsOwnNavpakyFolder(string pick, string expected) {
      Assert.Equal(expected, Check(pick).Folder);
    }

    [Fact]
    public void AnExistingInstallIsUsedAsItIsAndLocked() {
      const string existing = @"C:\Users\olena\AppData\Local\navpaky-app";
      _folders.Install(existing);

      FolderCheck check = InstallTarget.Check(
          @"D:\Tools", existing, Data(), _needed, _folders);

      Assert.Equal(existing, check.Folder);
      Assert.True(check.Locked);
      Assert.Equal(FolderVerdict.Ok, check.Verdict);
    }

    [Fact]
    public void AFolderWithForeignFilesIsRefused() {
      _folders.Add(@"D:\Tools\Navpaky\keep-me.txt");

      Assert.Equal(FolderVerdict.NotEmpty, Check(@"D:\Tools").Verdict);
    }

    [Fact]
    public void AnEmptyFolderIsAccepted() {
      _folders.AddFolder(@"D:\Tools\Navpaky");

      Assert.Equal(FolderVerdict.Ok, Check(@"D:\Tools").Verdict);
    }

    [Fact]
    public void AFolderHoldingANavpakyInstallIsAccepted() {
      _folders.Install(@"D:\Tools\Navpaky");

      FolderCheck check = Check(@"D:\Tools");

      Assert.Equal(FolderVerdict.Ok, check.Verdict);
      Assert.Empty(check.Extras);
    }

    // Task 1.2 of warn-reinstall-extra-files: the setup replaces the whole folder.
    [Fact]
    public void EntriesBesideTheInstallAreListedSorted() {
      _folders.Install(@"D:\Tools\Navpaky");
      _folders.Add(@"D:\Tools\Navpaky\notes.txt");
      _folders.Add(@"D:\Tools\Navpaky\backup\old.zip");
      _folders.AddFolder(@"D:\Tools\Navpaky\.hidden");

      FolderCheck check = Check(@"D:\Tools");

      Assert.Equal(FolderVerdict.Ok, check.Verdict);
      Assert.Equal([".hidden", "backup", "notes.txt"], check.Extras);
    }

    [Fact]
    public void TheExistingInstallsFolderListsItsExtrasToo() {
      const string existing = @"C:\Users\olena\AppData\Local\navpaky-app";
      _folders.Install(existing);
      _folders.Add(existing + @"\zorbel.ini");

      FolderCheck check = InstallTarget.Check(
          @"D:\Tools", existing, Data(), _needed, _folders);

      Assert.Equal(["zorbel.ini"], check.Extras);
    }

    [Fact]
    public void TheInstallsOwnNamesCountInAnyCase() {
      _folders.Add(@"D:\Apps\Navpaky\update.exe");
      _folders.Add(@"D:\Apps\Navpaky\NAVPAKY.EXE");
      _folders.Add(@"D:\Apps\Navpaky\Current\Navpaky.App.exe");
      _folders.AddFolder(@"D:\Apps\Navpaky\PACKAGES");

      Assert.Empty(Check(@"D:\Apps\Navpaky").Extras);
    }

    [Fact]
    public void AnInstallFolderThatCannotBeListedIsRefused() {
      _folders.Install(@"D:\Tools\Navpaky");
      _folders.Unlistable(@"D:\Tools\Navpaky");

      Assert.Equal(FolderVerdict.NotWritable, Check(@"D:\Tools").Verdict);
    }

    [Theory]
    [InlineData(@"C:\Users\olena\AppData\Local\Navpaky")]
    [InlineData(@"C:\Users\olena\AppData\Local\Navpaky\Tools")]
    [InlineData(@"C:\Users\olena\AppData\Roaming")]
    [InlineData(@"c:\users\olena\appdata\roaming\navpaky\x")]
    public void AFolderOverlappingTheDataIsRefused(string pick) {
      Assert.Equal(FolderVerdict.OverlapsData, Check(pick).Verdict);
    }

    [Fact]
    public void AFolderContainingTheDataIsRefused() {
      // Only a folder used as it is can contain a data folder: an existing install's.
      const string existing = @"C:\Users\olena\AppData\Local";

      FolderCheck check = InstallTarget.Check(
          @"D:\Tools", existing, Data(), _needed, _folders);

      Assert.Equal(FolderVerdict.OverlapsData, check.Verdict);
    }

    [Theory]
    [InlineData(@"C:\Users\olena\AppData\Local\Programs")]
    [InlineData(@"C:\Users\olena\AppData\Local\NavpakyOld")]
    public void ANeighbourOfTheDataIsAccepted(string pick) {
      Assert.Equal(FolderVerdict.Ok, Check(pick).Verdict);
    }

    [Fact]
    public void AFolderTheUserCannotWriteToIsRefused() {
      _folders.ReadOnly(@"C:\Program Files");

      Assert.Equal(FolderVerdict.NotWritable, Check(@"C:\Program Files").Verdict);
    }

    [Fact]
    public void TooLittleSpaceIsRefused() {
      _folders.Free = _needed - 1;

      Assert.Equal(FolderVerdict.NotEnoughSpace, Check(@"D:\Tools").Verdict);
    }

    [Fact]
    public void JustEnoughSpaceIsAccepted() {
      _folders.Free = _needed;

      Assert.Equal(FolderVerdict.Ok, Check(@"D:\Tools").Verdict);
    }

    [Fact]
    public void ForAllUsersTheProgramFilesFolderNeedsNoWriteRights() {
      _folders.ReadOnly(@"C:\Program Files");

      FolderCheck check = AllUsers();

      Assert.Equal(_machine, check.Folder);
      Assert.Equal(FolderVerdict.Ok, check.Verdict);
      Assert.True(check.Locked);
    }

    [Fact]
    public void ForAllUsersAForeignFolderIsStillRefused() {
      _folders.ReadOnly(@"C:\Program Files");
      _folders.Add(Path.Join(_machine, "other.exe"));

      Assert.Equal(FolderVerdict.NotEmpty, AllUsers().Verdict);
    }

    [Fact]
    public void ForAllUsersSpaceIsStillChecked() {
      _folders.Free = _needed - 1;

      Assert.Equal(FolderVerdict.NotEnoughSpace, AllUsers().Verdict);
    }

    private static DataFolderPair Data() {
      return new DataFolderPair(_roaming, _local);
    }

    private FolderCheck Check(string pick) {
      return InstallTarget.Check(pick, existingInstall: null, Data(), _needed, _folders);
    }

    private FolderCheck AllUsers() {
      return InstallTarget.Check(
          _machine, existingInstall: null, Data(), _needed, _folders, forAllUsers: true);
    }
  }
}
