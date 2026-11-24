using System.IO;

namespace Navpaky.Setup.Install {
  // The folder rules of the install wizard (spec install-wizard, design D5).
  internal static class InstallTarget {
    // The room Velopack's setup asks for on top of the package itself.
    public const long Margin = 50_000_000;

    private const string _folderName = "Navpaky";

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
      return new FolderCheck(
          folder, Verdict(folder, data, neededBytes, folders), existingInstall is not null);
    }

    private static FolderVerdict Verdict(
        string folder, DataFolderPair data, long neededBytes, IFolderProbe folders) {
      if (Overlaps(folder, Normalize(data.Roaming)) || Overlaps(folder, Normalize(data.Local))) {
        return FolderVerdict.OverlapsData;
      }

      if (folders.HasEntries(folder) && !HoldsNavpaky(folder, folders)) {
        return FolderVerdict.NotEmpty;
      }

      if (!folders.CanWrite(folder)) {
        return FolderVerdict.NotWritable;
      }

      return folders.FreeBytes(folder) < neededBytes + Margin
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
