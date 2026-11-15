using Rekey.App.Onboarding;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class AppLocationTests {
    private const string _local = @"C:\Users\Invented\AppData\Local";

    private static readonly LocationFolders _folders = new(
        InstallRoot: @$"{_local}\key-switcher",
        Temporary: [@$"{_local}\Temp\", @"D:\Scratch"],
        Downloads: @"C:\Users\Invented\Downloads");

    [Theory]
    [InlineData(@"C:\Users\Invented\AppData\Local\Temp\Rekey.App.exe")]
    [InlineData(@"C:\Users\Invented\AppData\Local\temp\copy\Rekey.App.exe")]
    [InlineData(@"D:\Scratch\Rekey.App.exe")]
    public void TemporaryFolderIsTemporary(string exe) {
      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Theory]
    [InlineData(@"E:\Archives\rekey.zip\Rekey.App.exe")]
    [InlineData(@"E:\Archives\Rekey-1.0.ZIP\bin\Rekey.App.exe")]
    public void PathThroughAZipIsTemporary(string exe) {
      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void DownloadsFolderIsTemporary() {
      string exe = @"C:\Users\Invented\Downloads\Rekey\Rekey.App.exe";

      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void InstallRootIsInstalled() {
      string exe = @$"{_local}\key-switcher\current\Rekey.App.exe";

      Assert.Equal(LocationKind.Installed, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Theory]
    [InlineData(@"E:\Tools\Rekey\Rekey.App.exe")]
    [InlineData(@"E:\Tools\rekey.zipped\Rekey.App.exe")]
    [InlineData(@"C:\Users\Invented\Downloads2\Rekey.App.exe")]
    public void OtherFoldersAreNeitherInstalledNorTemporary(string exe) {
      Assert.Equal(LocationKind.Other, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void WithoutAnInstallRootNothingIsInstalled() {
      LocationFolders portable = _folders with { InstallRoot = null, Downloads = null };

      Assert.Equal(
          LocationKind.Other,
          AppLocation.Classify(@$"{_local}\key-switcher\current\Rekey.App.exe", portable, false));
    }

    [Fact]
    public void DebugBuildIsNeverTemporary() {
      string exe = @$"{_local}\Temp\Rekey.App.exe";

      Assert.Equal(LocationKind.Other, AppLocation.Classify(exe, _folders, debugBuild: true));
    }
  }
}
