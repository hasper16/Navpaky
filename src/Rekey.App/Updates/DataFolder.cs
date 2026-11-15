using System.IO;
using Velopack.Locators;

namespace Rekey.App.Updates {
  // Where Rekey keeps settings, word lists, statistics and logs (design D1 of
  // add-auto-update-and-installer).
  internal static class DataFolder {
    // Velopack deletes its whole install root on uninstall, so data inside it could not be kept
    // when the user declines deletion; a packaging mistake must not reach a user's data.
    public static string Resolve(string localAppData, IVelopackLocator? locator) {
      string data = Path.GetFullPath(Path.Join(localAppData, "Rekey"));
      if (locator?.RootAppDir is { } root && Contains(root, data)) {
        throw new InvalidOperationException("The data folder lies inside the install root.");
      }

      return data;
    }

    private static bool Contains(string root, string path) {
      string parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root))
          + Path.DirectorySeparatorChar;
      return (path + Path.DirectorySeparatorChar).StartsWith(
          parent, StringComparison.OrdinalIgnoreCase);
    }
  }
}
