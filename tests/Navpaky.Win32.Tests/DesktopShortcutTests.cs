using System.IO;
using Xunit;

namespace Navpaky.Win32.Tests {
  public sealed class DesktopShortcutTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-link").FullName;
    private readonly string _install;
    private readonly string _link;

    public DesktopShortcutTests() {
      _install = Path.Join(_root, "Programs", "Navpaky");
      Directory.CreateDirectory(_install);
      File.WriteAllBytes(Path.Join(_install, "Navpaky.exe"), [0x4D, 0x5A]);
      _link = Path.Join(_root, "Desktop", DesktopShortcut.FileName);
      Directory.CreateDirectory(Path.GetDirectoryName(_link)!);
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void CreatesALinkToTheRootLauncher() {
      DesktopShortcut.Create(_link, _install);

      Assert.True(File.Exists(_link));
      Assert.Equal(Path.Join(_install, "Navpaky.exe"), DesktopShortcut.TargetOf(_link));
    }

    [Fact]
    public void CreatingAgainReplacesTheLink() {
      DesktopShortcut.Create(_link, _install);
      DesktopShortcut.Create(_link, _install);

      Assert.Equal(Path.Join(_install, "Navpaky.exe"), DesktopShortcut.TargetOf(_link));
    }

    [Fact]
    public void RemovesOurLink() {
      DesktopShortcut.Create(_link, _install);

      DesktopShortcut.Remove(_link, _install);

      Assert.False(File.Exists(_link));
    }

    [Fact]
    public void LeavesALinkThatPointsElsewhere() {
      string other = Path.Join(_root, "Other");
      Directory.CreateDirectory(other);
      File.WriteAllBytes(Path.Join(other, "Navpaky.exe"), [0x4D, 0x5A]);
      DesktopShortcut.Create(_link, other);

      DesktopShortcut.Remove(_link, _install);

      Assert.True(File.Exists(_link));
    }

    [Fact]
    public void RemovingAMissingLinkDoesNothing() {
      DesktopShortcut.Remove(_link, _install);

      Assert.False(File.Exists(_link));
    }
  }
}
