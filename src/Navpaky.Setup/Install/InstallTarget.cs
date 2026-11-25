using System.IO;

namespace Navpaky.Setup.Install {
  // The folder rules of the install wizard (spec install-wizard, design D5).
  internal static class InstallTarget {
    private const string _folderName = "Navpaky";

    // Velopack's layout plus our root launcher (design D1 of warn-reinstall-extra-files).
    private static readonly HashSet<string> _installNames = new(
        ["current", "packages", "Update.exe", "Navpaky.exe"], StringComparer.OrdinalIgnoreCase);

    public static FolderCheck Check(
        string pick,
        string? existingInstall,
        DataFolderPair data,
        long neededBytes,
        IFolderProbe folders) {
      // An existing install is reused exactly as it is: a second folder would orphan it.
      string folder = existingInstall is not null
          ? Normalize(existingInstall)
          : OwnFolder(Normalize(pick));
      IReadOnlyList<string>? extras = HoldsNavpaky(folder, folders)
          ? Extras(folders.Entries(folder))
          : [];
      return new FolderCheck(
          folder,
          Verdict(folder, data, neededBytes, folders, listed: extras is not null),
          existingInstall is not null,
          extras ?? []);
    }

    // The entries the setup would delete with the install it replaces; null when unlisted.
    private static List<string>? Extras(IReadOnlyList<string>? entries) {
      return entries?
          .Where(name => !_installNames.Contains(name))
          .Order(StringComparer.OrdinalIgnoreCase)
          .ToList();
    }

    // A folder that can't be listed can't be checked for what the setup would delete.
    private static FolderVerdict Verdict(
        string folder,
        DataFolderPair data,
        long neededBytes,
        IFolderProbe folders,
        bool listed) {
      if (Overlaps(folder, Normalize(data.Roaming)) || Overlaps(folder, Normalize(data.Local))) {
        return FolderVerdict.OverlapsData;
      }

      // Before HasEntries, which would throw on a folder that denies listing.
      if (!listed) {
        return FolderVerdict.NotWritable;
      }

      if (folders.HasEntries(folder) && !HoldsNavpaky(folder, folders)) {
        return FolderVerdict.NotEmpty;
      }

      if (!folders.CanWrite(folder)) {
        return FolderVerdict.NotWritable;
      }

      // neededBytes is what the setup itself checks (SetupPackage), overhead included.
      return folders.FreeBytes(folder) < neededBytes
          ? FolderVerdict.NotEnoughSpace
          : FolderVerdict.Ok;
    }

    private static string OwnFolder(string pick) {
      return Path.GetFileName(pick).Equals(_folderName, StringComparison.OrdinalIgnoreCase)
          ? pick
          : Path.Join(pick, _folderName);
    }

    private static bool HoldsNavpaky(string folder, IFolderProbe folders) {
      return folders.FileExists(Path.Join(folder, "Update.exe"))
          && folders.FileExists(Path.Join(folder, "current", "Navpaky.App.exe"));
    }

    private static bool Overlaps(string a, string b) {
      return Within(a, b) || Within(b, a);
    }

    // Whether inner is outer itself or lies under it.
    private static bool Within(string inner, string outer) {
      return inner.Equals(outer, StringComparison.OrdinalIgnoreCase)
          || inner.StartsWith(
              outer + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    // A full path without a trailing separator, except for a drive root ("D:\").
    private static string Normalize(string path) {
      string full = Path.GetFullPath(path);
      return full.Length > 3 ? Path.TrimEndingDirectorySeparator(full) : full;
    }
  }
}
