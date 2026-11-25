using System.IO;

namespace Navpaky.Setup.Install {
  // Which install the wizard must reuse, and which folder the Folder page proposes (design D5).
  internal static class InstallLookup {
    // Only a real install counts: its Installed apps entry names a folder that still holds
    // Velopack's Update.exe. The user's own install comes first, as before all-users installs.
    public static ExistingInstall? Existing(IUninstallEntry entry, IFolderProbe folders) {
      if (Installed(entry.InstallLocation(), folders) is { } mine) {
        return new ExistingInstall(mine, ForAllUsers: false);
      }

      return Installed(entry.MachineInstallLocation(), folders) is { } machine
          ? new ExistingInstall(machine, ForAllUsers: true)
          : null;
    }

    // The existing install (locked) first, then the folder kept settings remember from an earlier,
    // uninstalled one (editable), then the default.
    public static string Proposed(string? existing, string? remembered, string defaultFolder) {
      return existing ?? remembered ?? defaultFolder;
    }

    private static string? Installed(string? folder, IFolderProbe folders) {
      return !string.IsNullOrEmpty(folder) && folders.FileExists(Path.Join(folder, "Update.exe"))
          ? folder
          : null;
    }
  }
}
