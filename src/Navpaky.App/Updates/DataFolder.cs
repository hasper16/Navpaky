using System.IO;
using Navpaky.App.Onboarding;
using Velopack.Locators;

namespace Navpaky.App.Updates {
  // Where Navpaky keeps settings, word lists, statistics and logs (design D1 of
  // add-auto-update-and-installer).
  internal static class DataFolder {
    // Not Product.Name alone: Windows paths ignore case, so it would be the install root
    // %LOCALAPPDATA%\navpaky (design D1 of rename-to-navpaky).
    public const string Name = Product.Name + " Data";

    // Velopack deletes its whole install root on uninstall, so data inside it could not be kept
    // when the user declines deletion; a packaging mistake must not reach a user's data.
    public static string Resolve(string localAppData, IVelopackLocator? locator) {
      string data = Path.GetFullPath(Path.Join(localAppData, Name));
      if (locator?.RootAppDir is { } root && AppLocation.IsInside(root, data)) {
        throw new InvalidOperationException("The data folder lies inside the install root.");
      }

      return data;
    }
  }
}
