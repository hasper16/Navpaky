using Rekey.App.Tray;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Tray {
  public sealed class AppNamesTests {
    // Fix 2.8 of add-final-acceptance: the tray said "Windows Terminal Host", the description
    // of the process, instead of the app's own name.
    [Fact]
    public void PackagedAppGoesByItsShellName() {
      var terminal = new AppId(@"C:\Zorbit\WindowsTerminal.exe", "Zorbit.Terminal_8abc!App");

      Assert.Equal("Terminal", AppNames.Of(terminal, _ => "Terminal"));
    }

    [Fact]
    public void PackagedAppWithoutShellNameGoesByItsFile() {
      var terminal = new AppId(@"C:\Zorbit\WindowsTerminal.exe", "Zorbit.Terminal_8abc!App");

      Assert.Equal("WindowsTerminal", AppNames.Of(terminal, _ => null));
    }

    [Fact]
    public void ExcludedStoreAppEntryGoesByItsShellName() {
      Assert.Equal("Terminal", AppNames.OfKey("Zorbit.Terminal_8abc!App", _ => "Terminal"));
    }

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
    public void PackageIdentityTheShellDoesNotKnowIsNamedByItsPackage() {
      Assert.Equal(
          "WindowsTerminal",
          AppNames.OfKey("Microsoft.WindowsTerminal_8wekyb3d8bbwe!App", _ => null));
    }

    [Fact]
    public void AProgramPathWithABangIsStillAPath() {
      Assert.Equal("editor", AppNames.OfKey(@"C:\Tools\Acme!Beta\editor.exe"));
    }
  }
}
