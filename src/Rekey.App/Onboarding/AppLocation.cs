using System.IO;

namespace Rekey.App.Onboarding {
  // Design D4 of add-onboarding: a copy in a temporary folder, inside a ZIP or in Downloads
  // stops working once a cleanup removes it.
  internal static class AppLocation {
    public static LocationKind Classify(string exePath, LocationFolders folders, bool debugBuild) {
      if (debugBuild) {
        return LocationKind.Other;
      }

      string path = Path.GetFullPath(exePath);
      if (folders.InstallRoot is { } root && IsInside(root, path)) {
        return LocationKind.Installed;
      }

      bool temporary = folders.Temporary.Any(folder => IsInside(folder, path))
          || (folders.Downloads is { } downloads && IsInside(downloads, path))
          || ThroughZip(path);
      return temporary ? LocationKind.Temporary : LocationKind.Other;
    }

    public static bool IsInside(string folder, string path) {
      // A drive root such as D:\ keeps its separator when trimmed.
      string parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder));
      if (!Path.EndsInDirectorySeparator(parent)) {
        parent += Path.DirectorySeparatorChar;
      }

      return (path + Path.DirectorySeparatorChar).StartsWith(
          parent, StringComparison.OrdinalIgnoreCase);
    }

    // Explorer runs a program from an open ZIP out of a folder named after the archive.
    private static bool ThroughZip(string path) {
      string? folder = Path.GetDirectoryName(path);
      return folder is not null && folder.Split(Path.DirectorySeparatorChar).Any(
          segment => segment.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
    }
  }
}
