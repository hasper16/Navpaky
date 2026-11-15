using System.IO;
using Rekey.App.Updates;
using Velopack.Locators;
using Velopack.Logging;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class DataFolderTests {
    private const string _localAppData = @"C:\Users\Invented\AppData\Local";

    [Fact]
    public void DataLivesOutsideTheInstallRoot() {
      string data = DataFolder.Resolve(_localAppData, Installed(@$"{_localAppData}\key-switcher"));

      Assert.Equal(@$"{_localAppData}\Rekey", data);
    }

    [Fact]
    public void DataFolderIsTheSameWhenRekeyIsNotInstalled() {
      Assert.Equal(@$"{_localAppData}\Rekey", DataFolder.Resolve(_localAppData, null));
    }

    [Theory]
    [InlineData(@"C:\Users\Invented\AppData\Local\Rekey")]
    [InlineData(@"C:\Users\Invented\AppData\Local\rekey\")]
    [InlineData(@"C:\Users\Invented\AppData\Local")]
    public void InstallRootHoldingTheDataIsRefused(string root) {
      Assert.Throws<InvalidOperationException>(
          () => DataFolder.Resolve(_localAppData, Installed(root)));
    }

    [Fact]
    public void RootSharingOnlyANamePrefixIsAccepted() {
      string data = DataFolder.Resolve(_localAppData, Installed(@$"{_localAppData}\Rekey2"));

      Assert.Equal(@$"{_localAppData}\Rekey", data);
    }

    private static TestVelopackLocator Installed(string root) {
      return new TestVelopackLocator(
          "key-switcher", "1.0.0", Path.Join(root, "packages"), Path.Join(root, "current"),
          root, Path.Join(root, "Update.exe"), "win", NullVelopackLogger.Instance, null,
          Path.Join(root, "current", "Rekey.App.exe"));
    }
  }
}
