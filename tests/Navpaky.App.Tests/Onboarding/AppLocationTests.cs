using Navpaky.App.Onboarding;
using Xunit;

namespace Navpaky.App.Tests.Onboarding {
  public sealed class AppLocationTests {
    private const string _local = @"C:\Users\Invented\AppData\Local";

    private static readonly LocationFolders _folders = new(
        InstallRoot: @$"{_local}\navpaky-app",
        Temporary: [@$"{_local}\Temp\", @"D:\Scratch"],
        Downloads: @"C:\Users\Invented\Downloads");

    [Theory]
    [InlineData(@"C:\Users\Invented\AppData\Local\Temp\Navpaky.App.exe")]
    [InlineData(@"C:\Users\Invented\AppData\Local\temp\copy\Navpaky.App.exe")]
    [InlineData(@"D:\Scratch\Navpaky.App.exe")]
    public void TemporaryFolderIsTemporary(string exe) {
      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Theory]
    [InlineData(@"E:\Archives\navpaky.zip\Navpaky.App.exe")]
    [InlineData(@"E:\Archives\Navpaky-1.0.ZIP\bin\Navpaky.App.exe")]
    public void PathThroughAZipIsTemporary(string exe) {
      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void DownloadsFolderIsTemporary() {
      string exe = @"C:\Users\Invented\Downloads\Navpaky\Navpaky.App.exe";

      Assert.Equal(LocationKind.Temporary, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void DownloadsAtADriveRootIsTemporary() {
      LocationFolders atRoot = _folders with { Downloads = @"D:\" };

      Assert.Equal(
          LocationKind.Temporary,
          AppLocation.Classify(@"D:\Navpaky\Navpaky.App.exe", atRoot, debugBuild: false));
    }

    [Fact]
    public void InstallRootIsInstalled() {
      string exe = @$"{_local}\navpaky-app\current\Navpaky.App.exe";

      Assert.Equal(LocationKind.Installed, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Theory]
    [InlineData(@"E:\Tools\Navpaky\Navpaky.App.exe")]
    [InlineData(@"E:\Tools\navpaky.zipped\Navpaky.App.exe")]
    [InlineData(@"C:\Users\Invented\Downloads2\Navpaky.App.exe")]
    public void OtherFoldersAreNeitherInstalledNorTemporary(string exe) {
      Assert.Equal(LocationKind.Other, AppLocation.Classify(exe, _folders, debugBuild: false));
    }

    [Fact]
    public void WithoutAnInstallRootNothingIsInstalled() {
      LocationFolders portable = _folders with { InstallRoot = null, Downloads = null };

      Assert.Equal(
          LocationKind.Other,
          AppLocation.Classify(@$"{_local}\navpaky-app\current\Navpaky.App.exe", portable, false));
    }

    [Fact]
    public void DebugBuildIsNeverTemporary() {
      string exe = @$"{_local}\Temp\Navpaky.App.exe";

      Assert.Equal(LocationKind.Other, AppLocation.Classify(exe, _folders, debugBuild: true));
    }
  }
}
