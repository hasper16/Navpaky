using System.IO;

namespace Navpaky.App.Updates {
  // Each installation leaves a new id in the local data folder, so the uninstall's data question
  // can tell a reinstall made while it was open from the uninstall still running (design D8 of
  // rename-to-navpaky).
  internal static class InstallMarker {
    public const string FileName = "install-id.txt";

    public static void Write(string localFolder) {
      Directory.CreateDirectory(localFolder);
      File.WriteAllText(Path.Join(localFolder, FileName), Guid.NewGuid().ToString("N"));
    }
  }
}
