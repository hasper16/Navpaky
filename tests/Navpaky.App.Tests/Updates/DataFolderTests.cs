using System.IO;
using Navpaky.App.Updates;
using Velopack.Locators;
using Velopack.Logging;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class DataFolderTests {
    private const string _localAppData = @"C:\Users\Invented\AppData\Local";

    [Fact]
    public void DataLivesOutsideTheInstallRoot() {
      string data = DataFolder.Resolve(_localAppData, Installed(@$"{_localAppData}\navpaky"));

      Assert.Equal(@$"{_localAppData}\Navpaky Data", data);
    }

    [Fact]
    public void DataFolderIsTheSameWhenNavpakyIsNotInstalled() {
      Assert.Equal(@$"{_localAppData}\Navpaky Data", DataFolder.Resolve(_localAppData, null));
    }

    [Theory]
    [InlineData(@"C:\Users\Invented\AppData\Local\Navpaky Data")]
    [InlineData(@"C:\Users\Invented\AppData\Local\navpaky data\")]
    [InlineData(@"C:\Users\Invented\AppData\Local")]
    public void InstallRootHoldingTheDataIsRefused(string root) {
      Assert.Throws<InvalidOperationException>(
          () => DataFolder.Resolve(_localAppData, Installed(root)));
    }

    [Fact]
    public void RootSharingOnlyANamePrefixIsAccepted() {
      string data = DataFolder.Resolve(_localAppData, Installed(@$"{_localAppData}\Navpaky Data2"));

      Assert.Equal(@$"{_localAppData}\Navpaky Data", data);
    }

    private static TestVelopackLocator Installed(string root) {
      return new TestVelopackLocator(
          "navpaky", "1.0.0", Path.Join(root, "packages"), Path.Join(root, "current"),
          root, Path.Join(root, "Update.exe"), "win", NullVelopackLogger.Instance, null,
          Path.Join(root, "current", "Navpaky.App.exe"));
    }
  }
}
