using System.ComponentModel;
using System.Diagnostics;
using Rekey.Core;

namespace Rekey.App.Tray {
  // The apps with a window open now, for "Add a running app".
  internal static class RunningApps {
    public static IReadOnlyList<(AppId App, string Name)> List() {
      var apps = new Dictionary<string, (AppId, string)>(StringComparer.OrdinalIgnoreCase);
      foreach (Process process in Process.GetProcesses()) {
        using (process) {
          if (process.Id == Environment.ProcessId || process.MainWindowHandle == 0
              || PathOf(process) is not { } path) {
            continue;
          }

          var app = new AppId(path);
          apps.TryAdd(app.Key, (app, AppNames.Of(app)));
        }
      }

      return [.. apps.Values.OrderBy(app => app.Item2, StringComparer.CurrentCultureIgnoreCase)];
    }

    // An elevated or protected process does not show its path; Rekey leaves it out.
    private static string? PathOf(Process process) {
      try {
        return process.MainModule?.FileName;
      } catch (Win32Exception) {
        return null;
      } catch (InvalidOperationException) {
        return null;
      }
    }
  }
}
