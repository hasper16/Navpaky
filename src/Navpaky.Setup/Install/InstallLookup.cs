using System.IO;

namespace Navpaky.Setup.Install {
  // Which install the wizard must reuse, and which folder the Folder page proposes (design D5).
  internal static class InstallLookup {
    // Only a real install counts: its Installed apps entry names a folder that still holds
    // Velopack's Update.exe.
    public static string? Existing(IUninstallEntry entry, IFolderProbe folders) {
      string? folder = entry.InstallLocation();
      bool installed = !string.IsNullOrEmpty(folder)
          && folders.FileExists(Path.Join(folder, "Update.exe"));
      return installed ? folder : null;
    }

    // The existing install (locked) first, then the folder kept settings remember from an earlier,
    // uninstalled one (editable), then the default.
    public static string Proposed(string? existing, string? remembered, string defaultFolder) {
      return existing ?? remembered ?? defaultFolder;
    }
  }
}
