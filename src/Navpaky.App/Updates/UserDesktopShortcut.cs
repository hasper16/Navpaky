using System.IO;
using Navpaky.Win32;

namespace Navpaky.App.Updates {
  // Navpaky's shortcut on the current user's desktop (design D6 of add-install-wizard).
  internal sealed class UserDesktopShortcut : IDesktopShortcut {
    public void Create(string installFolder) {
      DesktopShortcut.Create(LinkPath(), installFolder);
    }

    public void Remove(string installFolder) {
      DesktopShortcut.Remove(LinkPath(), installFolder);
    }

    private static string LinkPath() {
      return Path.Join(
          Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
          DesktopShortcut.FileName);
    }
  }
}
