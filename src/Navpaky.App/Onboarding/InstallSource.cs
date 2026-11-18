using System.IO;
using Navpaky.App.Updates;

namespace Navpaky.App.Onboarding {
  // Where "Install" leads (design D4 of add-onboarding): the Velopack setup that came with this
  // copy, otherwise the project's releases page.
  internal static class InstallSource {
    public const string SetupName = "navpaky-win-Setup.exe";

    public static readonly string ReleasesPage = VelopackUpdates.Repository + "/releases/latest";

    public static string For(string exeFolder, Func<string, bool> fileExists) {
      string setup = Path.Join(exeFolder, SetupName);
      return fileExists(setup) ? setup : ReleasesPage;
    }
  }
}
