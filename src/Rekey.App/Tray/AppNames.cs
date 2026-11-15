using System.Diagnostics;
using System.IO;
using Rekey.Core;

namespace Rekey.App.Tray {
  // The name users know an app by, for "Turn off in Telegram" and the Excluded Apps list.
  internal static class AppNames {
    public static string Of(AppId app) {
      string? description = null;
      try {
        description = FileVersionInfo.GetVersionInfo(app.ExePath).FileDescription;
      } catch (FileNotFoundException) {
        // The executable is gone or out of reach; its file name still names it.
      } catch (ArgumentException) {
        // An empty path: a Store app Rekey could not trace to a file.
      }

      return Choose(description, app.ExePath.Length > 0 ? app.ExePath : app.Key);
    }

    // An Excluded Apps entry: a program path, or the package identity (AUMID) of a Store app,
    // which names no file.
    public static string OfKey(string key) {
      int bang = key.IndexOf('!', StringComparison.Ordinal);
      if (bang < 0) {
        return Of(new AppId(key));
      }

      string family = key[..bang].Split('_')[0];
      return family[(family.LastIndexOf('.') + 1)..];
    }

    public static string Choose(string? description, string exePath) {
      return string.IsNullOrWhiteSpace(description)
          ? Path.GetFileNameWithoutExtension(exePath)
          : description.Trim();
    }
  }
}
