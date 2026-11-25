using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class InstallLookupTests {
    private const string _default = @"C:\Users\olena\AppData\Local\Programs\Navpaky";
    private const string _installed = @"C:\Users\olena\AppData\Local\navpaky-app";
    private const string _machine = @"C:\Program Files\Navpaky";

    private readonly FakeFolders _folders = new();

    [Fact]
    public void TheUninstallEntryNamesTheExistingInstall() {
      _folders.Install(_installed);

      Assert.Equal(
          new ExistingInstall(_installed, ForAllUsers: false),
          InstallLookup.Existing(new FakeEntry(_installed), _folders));
    }

    [Fact]
    public void TheMachineEntryNamesAnAllUsersInstall() {
      _folders.Install(_machine);

      Assert.Equal(
          new ExistingInstall(_machine, ForAllUsers: true),
          InstallLookup.Existing(new FakeEntry(null, _machine), _folders));
    }

    [Fact]
    public void AMachineEntryWhoseFolderLostItsFilesIsNoInstall() {
      Assert.Null(InstallLookup.Existing(new FakeEntry(null, _machine), _folders));
    }

    [Fact]
    public void AnEntryWhoseFolderLostItsFilesIsNoInstall() {
      Assert.Null(InstallLookup.Existing(new FakeEntry(_installed), _folders));
    }

    [Fact]
    public void NoEntryIsNoInstall() {
      Assert.Null(InstallLookup.Existing(new FakeEntry(null), _folders));
    }

    [Fact]
    public void TheExistingInstallIsProposed() {
      Assert.Equal(
          _installed, InstallLookup.Proposed(_installed, remembered: @"D:\Old\Navpaky", _default));
    }

    [Fact]
    public void WithoutAnInstallTheRememberedFolderIsProposed() {
      Assert.Equal(
          @"D:\Old\Navpaky", InstallLookup.Proposed(null, remembered: @"D:\Old\Navpaky", _default));
    }

    [Fact]
    public void WithNeitherTheDefaultIsProposed() {
      Assert.Equal(_default, InstallLookup.Proposed(null, remembered: null, _default));
    }

    private sealed class FakeEntry(string? location, string? machine = null) : IUninstallEntry {
      public string? InstallLocation() {
        return location;
      }

      public string? MachineInstallLocation() {
        return machine;
      }
    }
  }
}
