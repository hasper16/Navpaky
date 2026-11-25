using System.Diagnostics;
using System.IO;

namespace Navpaky.App.Updates {
  // The uninstall of an all-users install's Installed apps entry (design D4 of
  // add-machine-wide-install): Velopack's uninstall run without elevation exits 0 and leaves every
  // file, and Windows does not elevate an UninstallString by itself.
  internal static class AllUsersUninstall {
    public const string Argument = "--uninstall-all-users";

    // Only an all-users install, whose root only administrators can write, elevates for this; a
    // per-user copy must not ask for elevation to run an Update.exe its user can replace.
    public static bool Applies(IReadOnlyList<string> args, bool machine) {
      return machine && args is [Argument];
    }

    // Unelevated, Navpaky starts itself again through UAC, so the prompt names Navpaky; elevated,
    // it starts Velopack's uninstall and exits at once, holding no file the uninstall deletes.
    public static ProcessStartInfo Next(bool elevated, string self, string root) {
      if (!elevated) {
        return new ProcessStartInfo(self, [Argument]) { UseShellExecute = true, Verb = "runas" };
      }

      return new ProcessStartInfo(Path.Join(root, "Update.exe"), ["--uninstall"]) {
        UseShellExecute = false,
      };
    }
  }
}
