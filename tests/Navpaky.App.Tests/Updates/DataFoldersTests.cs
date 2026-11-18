using System.IO;
using Navpaky.App.Updates;
using Velopack.Locators;
using Velopack.Logging;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class DataFoldersTests {
    private const string _appData = @"C:\Users\Invented\AppData\Roaming";
    private const string _localAppData = @"C:\Users\Invented\AppData\Local";

    [Fact]
    public void SettingsRoamAndLocalStateStaysOutsideTheInstallRoot() {
      DataFolders data = DataFolders.Resolve(
          _appData, _localAppData, Installed(@$"{_localAppData}\navpaky-app"));

      Assert.Equal(@$"{_appData}\Navpaky", data.Settings);
      Assert.Equal(@$"{_localAppData}\Navpaky", data.Local);
    }

    [Fact]
    public void FoldersAreTheSameWhenNavpakyIsNotInstalled() {
      DataFolders data = DataFolders.Resolve(_appData, _localAppData, null);

      Assert.Equal(@$"{_appData}\Navpaky", data.Settings);
      Assert.Equal(@$"{_localAppData}\Navpaky", data.Local);
    }

    [Theory]
    [InlineData(@"C:\Users\Invented\AppData\Local\Navpaky")]
    [InlineData(@"C:\Users\Invented\AppData\Local\navpaky\")]
    [InlineData(@"C:\Users\Invented\AppData\Local")]
    [InlineData(@"C:\Users\Invented\AppData\Roaming\Navpaky")]
    [InlineData(@"C:\Users\Invented\AppData")]
    public void InstallRootHoldingEitherFolderIsRefused(string root) {
      Assert.Throws<InvalidOperationException>(
          () => DataFolders.Resolve(_appData, _localAppData, Installed(root)));
    }

    [Fact]
    public void RootSharingOnlyANamePrefixIsAccepted() {
      DataFolders data = DataFolders.Resolve(
          _appData, _localAppData, Installed(@$"{_localAppData}\Navpaky2"));

      Assert.Equal(@$"{_localAppData}\Navpaky", data.Local);
    }

    private static TestVelopackLocator Installed(string root) {
      return new TestVelopackLocator(
          "navpaky-app", "1.0.0", Path.Join(root, "packages"), Path.Join(root, "current"),
          root, Path.Join(root, "Update.exe"), "win", NullVelopackLogger.Instance, null,
          Path.Join(root, "current", "Navpaky.App.exe"));
    }
  }
}
