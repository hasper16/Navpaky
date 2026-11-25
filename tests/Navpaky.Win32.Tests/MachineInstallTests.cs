using System.IO;
using Microsoft.Win32;
using Xunit;

namespace Navpaky.Win32.Tests {
  // Both "hives" are throwaway keys under HKCU\Software and the Start menus are temporary folders,
  // so the tests never touch HKLM or the real Start menu.
  public sealed class MachineInstallTests : IDisposable {
    private const string _entry = @"Microsoft\Windows\CurrentVersion\Uninstall\navpaky-app";
    private const string _activeSetup = @"Microsoft\Active Setup\Installed Components\Navpaky";

    private readonly string _keys = $@"Software\Navpaky.Tests\{Guid.NewGuid():N}";
    private readonly string _machineEntry;
    private readonly string _userEntry;
    private readonly string _machineActiveSetup;
    private readonly string _folder = Directory.CreateTempSubdirectory("navpaky-machine").FullName;
    private readonly string _root;
    private readonly string _common;
    private readonly string _userPrograms;
    private readonly MachineInstall _install;

    public MachineInstallTests() {
      _machineEntry = $@"{_keys}\Machine\{_entry}";
      _userEntry = $@"{_keys}\User\{_entry}";
      _machineActiveSetup = $@"{_keys}\Machine\{_activeSetup}";
      _root = Path.Join(_folder, "Program Files", "Navpaky");
      Directory.CreateDirectory(_root);
      File.WriteAllBytes(Path.Join(_root, "Navpaky.exe"), [0x4D, 0x5A]);
      _common = Directory.CreateDirectory(Path.Join(_folder, "Common")).FullName;
      _userPrograms = Directory.CreateDirectory(Path.Join(_folder, "User")).FullName;
      _install = new MachineInstall(
          new RegistryRoot(Registry.CurrentUser, $@"{_keys}\Machine"),
          new RegistryRoot(Registry.CurrentUser, $@"{_keys}\User"),
          _common,
          _userPrograms);
    }

    public void Dispose() {
      Registry.CurrentUser.DeleteSubKeyTree(_keys, throwOnMissingSubKey: false);
      Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public void WritesTheMachineEntryWithAnElevatingUninstall() {
      _install.Register(_root, "0.2.0", signIn: true);

      using RegistryKey entry = Registry.CurrentUser.OpenSubKey(_machineEntry)!;
      Assert.Equal("Navpaky", entry.GetValue("DisplayName"));
      Assert.Equal("0.2.0", entry.GetValue("DisplayVersion"));
      Assert.Equal(_root, entry.GetValue("InstallLocation"));
      Assert.Equal(
          $"\"{Path.Join(_root, "current", "Navpaky.App.exe")}\" --uninstall-all-users",
          entry.GetValue("UninstallString"));
      Assert.Equal(1, entry.GetValue("NoModify"));
      Assert.Equal(1, entry.GetValue("NoRepair"));
    }

    [Fact]
    public void MovesTheDisplayValuesOfVelopacksEntryAndDeletesIt() {
      using (RegistryKey velopack = Registry.CurrentUser.CreateSubKey(_userEntry)) {
        velopack.SetValue("DisplayName", "Navpaky (test)");
        velopack.SetValue("DisplayIcon", @"C:\icon.ico");
        velopack.SetValue("Publisher", "Someone");
      }

      _install.RefreshEntry(_root, "0.2.0");

      using RegistryKey entry = Registry.CurrentUser.OpenSubKey(_machineEntry)!;
      Assert.Equal("Navpaky (test)", entry.GetValue("DisplayName"));
      Assert.Equal(@"C:\icon.ico", entry.GetValue("DisplayIcon"));
      Assert.Equal("Someone", entry.GetValue("Publisher"));
      Assert.Null(Registry.CurrentUser.OpenSubKey(_userEntry));
    }

    [Fact]
    public void RefreshingKeepsEarlierDisplayValuesAndBumpsTheVersion() {
      using (RegistryKey velopack = Registry.CurrentUser.CreateSubKey(_userEntry)) {
        velopack.SetValue("Publisher", "Someone");
      }

      _install.RefreshEntry(_root, "0.1.0");
      _install.RefreshEntry(_root, "0.2.0");

      using RegistryKey entry = Registry.CurrentUser.OpenSubKey(_machineEntry)!;
      Assert.Equal("0.2.0", entry.GetValue("DisplayVersion"));
      Assert.Equal("Someone", entry.GetValue("Publisher"));
    }

    [Fact]
    public void SignInOnRegistersActiveSetupWithTheRunEntryStub() {
      _install.Register(_root, "0.1.0", signIn: true);

      using RegistryKey key = Registry.CurrentUser.OpenSubKey(_machineActiveSetup)!;
      Assert.Equal("Navpaky", key.GetValue(string.Empty));
      Assert.Equal("1,0,0,0", key.GetValue("Version"));
      Assert.Equal(1, key.GetValue("IsInstalled"));
      Assert.Equal(
          @"reg.exe add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v Navpaky "
              + $"/t REG_SZ /d \"\\\"{Path.Join(_root, "Navpaky.exe")}\\\"\" /f",
          key.GetValue("StubPath"));
    }

    [Fact]
    public void SignInOffRemovesActiveSetup() {
      _install.Register(_root, "0.1.0", signIn: true);

      _install.Register(_root, "0.1.0", signIn: false);

      Assert.Null(Registry.CurrentUser.OpenSubKey(_machineActiveSetup));
    }

    [Fact]
    public void MarksActiveSetupDoneForTheCurrentUser() {
      _install.MarkActiveSetupDone();

      using RegistryKey key = Registry.CurrentUser.OpenSubKey($@"{_keys}\User\{_activeSetup}")!;
      Assert.Equal("1,0,0,0", key.GetValue("Version"));
    }

    [Fact]
    public void ReplacesTheUsersStartMenuLinkWithASharedOne() {
      string userLink = Path.Join(_userPrograms, DesktopShortcut.FileName);
      DesktopShortcut.Create(userLink, _root);

      _install.Register(_root, "0.1.0", signIn: true);

      Assert.False(File.Exists(userLink));
      Assert.Equal(
          Path.Join(_root, "Navpaky.exe"),
          DesktopShortcut.TargetOf(Path.Join(_common, DesktopShortcut.FileName)));
    }

    [Fact]
    public void UnregisterRemovesEverythingMachineWide() {
      _install.Register(_root, "0.1.0", signIn: true);

      _install.Unregister(_root);

      Assert.Null(Registry.CurrentUser.OpenSubKey(_machineEntry));
      Assert.Null(Registry.CurrentUser.OpenSubKey(_machineActiveSetup));
      Assert.False(File.Exists(Path.Join(_common, DesktopShortcut.FileName)));
    }

    [Fact]
    public void UnregisteringTwiceDoesNothing() {
      _install.Unregister(_root);
      _install.Unregister(_root);

      Assert.Null(Registry.CurrentUser.OpenSubKey(_machineEntry));
    }

    [Theory]
    [InlineData(@"Navpaky", true)]
    [InlineData(@"NAVPAKY\", true)]
    [InlineData(@"Navpaky2", false)]
    public void KnowsTheMachineRoot(string name, bool expected) {
      string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

      Assert.Equal(expected, MachineInstall.IsMachineRoot(Path.Join(programFiles, name)));
    }

    [Fact]
    public void APerUserFolderIsNotTheMachineRoot() {
      Assert.False(MachineInstall.IsMachineRoot(_root));
    }
  }
}
