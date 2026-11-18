using System.IO;
using Navpaky.App.Onboarding;
using Velopack.Locators;

namespace Navpaky.App.Updates {
  // Where Navpaky keeps its data (design D7 of rename-to-navpaky): settings and word lists in
  // the roaming profile, so they may follow the user; statistics, logs and other machine state
  // in the local profile, since they can hold typed words and must not roam.
  internal sealed record DataFolders(string Settings, string Local) {
    // Not Product.Name alone: Windows paths ignore case, so %LOCALAPPDATA%\Navpaky would be the
    // install root %LOCALAPPDATA%\navpaky.
    private const string _localName = Product.Name + " Data";

    // Velopack deletes its whole install root on uninstall, so data inside it could not be kept
    // when the user declines deletion; a packaging mistake must not reach a user's data.
    public static DataFolders Resolve(
        string appData, string localAppData, IVelopackLocator? locator) {
      var folders = new DataFolders(
          Path.GetFullPath(Path.Join(appData, Product.Name)),
          Path.GetFullPath(Path.Join(localAppData, _localName)));
      if (locator?.RootAppDir is { } root
          && (AppLocation.IsInside(root, folders.Settings)
              || AppLocation.IsInside(root, folders.Local))) {
        throw new InvalidOperationException("A data folder lies inside the install root.");
      }

      return folders;
    }
  }
}
