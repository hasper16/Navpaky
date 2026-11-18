using System.Diagnostics;
using System.IO;
using Rekey.Core;
using Rekey.Win32;

namespace Rekey.App.Tray {
  // The name users know an app by, for "Turn off in Telegram" and the Excluded Apps list.
  internal static class AppNames {
    public static string Of(AppId app) {
      return Of(app, PackagedAppName.Of);
    }

    // A Store app goes by the name the Start menu shows; its process description may name a
    // host, such as "Windows Terminal Host".
    public static string Of(AppId app, Func<string, string?> shellName) {
      if (app.Aumid is { } aumid && shellName(aumid) is { Length: > 0 } name) {
        return name;
      }

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
      return OfKey(key, PackagedAppName.Of);
    }

    public static string OfKey(string key, Func<string, string?> shellName) {
      // A package identity never holds a backslash; a path may hold a "!".
      int bang = key.IndexOf('!', StringComparison.Ordinal);
      if (bang < 0 || key.Contains('\\', StringComparison.Ordinal)) {
        return Of(new AppId(key), shellName);
      }

      if (shellName(key) is { Length: > 0 } name) {
        return name;
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
