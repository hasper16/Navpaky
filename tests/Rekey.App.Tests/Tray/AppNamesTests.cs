using Rekey.App.Tray;
using Xunit;

namespace Rekey.App.Tests.Tray {
  public sealed class AppNamesTests {
    [Fact]
    public void FileDescriptionNamesTheApp() {
      Assert.Equal("Telegram Desktop", AppNames.Choose("Telegram Desktop", @"C:\T\Telegram.exe"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void FileNameNamesAnAppWithoutDescription(string? description) {
      Assert.Equal("Telegram", AppNames.Choose(description, @"C:\T\Telegram.exe"));
    }

    [Fact]
    public void DescriptionIsTrimmed() {
      Assert.Equal("Notepad", AppNames.Choose(" Notepad ", @"C:\W\notepad.exe"));
    }

    [Fact]
    public void PackageIdentityIsNamedByItsPackage() {
      Assert.Equal(
          "WindowsTerminal", AppNames.OfKey("Microsoft.WindowsTerminal_8wekyb3d8bbwe!App"));
    }

    [Fact]
    public void AProgramPathWithABangIsStillAPath() {
      Assert.Equal("editor", AppNames.OfKey(@"C:\Tools\Acme!Beta\editor.exe"));
    }
  }
}
