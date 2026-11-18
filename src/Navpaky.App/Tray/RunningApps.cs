using System.Diagnostics;
using Navpaky.Core;
using Navpaky.Win32.Focus;

namespace Navpaky.App.Tray {
  // The apps with a window open now, for "Add a running app", named the way Auto Correction
  // matches them: a Store app by its package identity, others by program path.
  internal static class RunningApps {
    public static IReadOnlyList<(AppId App, string Name)> List() {
      var apps = new Dictionary<string, (AppId, string)>(StringComparer.OrdinalIgnoreCase);
      foreach (Process process in Process.GetProcesses()) {
        using (process) {
          if (process.MainWindowHandle != 0
              && ForegroundApps.AppOf(process.MainWindowHandle) is { } app) {
            apps.TryAdd(app.Key, (app, AppNames.Of(app)));
          }
        }
      }

      return [.. apps.Values.OrderBy(app => app.Item2, StringComparer.CurrentCultureIgnoreCase)];
    }
  }
}
