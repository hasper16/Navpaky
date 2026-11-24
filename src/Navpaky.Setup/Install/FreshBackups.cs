using System.IO;
using Navpaky.Core;

namespace Navpaky.Setup.Install {
  // The kept data the install hook moved aside for Start fresh (design D4 of add-install-wizard):
  // deleted once the install completed, put back exactly when it failed or a crashed run left it.
  internal static class FreshBackups {
    private const string _marker = ".fresh-";

    public static string Suffix(Guid attempt) {
      return $"{_marker}{attempt:N}";
    }

    public static void Discard(DataFolderPair data, Guid attempt) {
      foreach (string backup in BackupsOf(data, Suffix(attempt))) {
        Directory.Delete(backup, recursive: true);
      }
    }

    public static void Restore(DataFolderPair data, Guid attempt) {
      Restore(data, Suffix(attempt));
    }

    // Before the wizard looks for kept data, so a crash never loses it silently. A backup whose
    // install completed (its acknowledgement is still there) is deleted instead: putting it back
    // would replace data the user has had since.
    public static void RestoreLeftovers(DataFolderPair data) {
      IEnumerable<string> suffixes = new[] { data.Roaming, data.Local }
          .SelectMany(folder => Leftovers(folder))
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .ToList();
      foreach (string suffix in suffixes) {
        string done = Path.Join(data.Local, "install-done-" + suffix[_marker.Length..]);
        if (File.Exists(done)) {
          foreach (string backup in BackupsOf(data, suffix)) {
            Directory.Delete(backup, recursive: true);
          }

          File.Delete(done);
        } else {
          Restore(data, suffix);
        }
      }
    }

    private static void Restore(DataFolderPair data, string suffix) {
      string roamingBackup = data.Roaming + suffix;
      if (Directory.Exists(roamingBackup)) {
        if (Directory.Exists(data.Roaming)) {
          Directory.Delete(data.Roaming, recursive: true);
        }

        Directory.Move(roamingBackup, data.Roaming);
      }

      string localBackup = data.Local + suffix;
      if (!Directory.Exists(localBackup)) {
        return;
      }

      // What the failed install wrote goes; the hand-off file stays for the wizard to remove.
      Directory.CreateDirectory(data.Local);
      foreach (string entry in Directory.EnumerateFileSystemEntries(data.Local)) {
        if (Path.GetFileName(entry) != InstallChoices.FileName) {
          Delete(entry);
        }
      }

      foreach (string entry in Directory.EnumerateFileSystemEntries(localBackup)) {
        string target = Path.Join(data.Local, Path.GetFileName(entry));
        if (Directory.Exists(entry)) {
          Directory.Move(entry, target);
        } else {
          File.Move(entry, target);
        }
      }

      Directory.Delete(localBackup);
    }

    private static IEnumerable<string> BackupsOf(DataFolderPair data, string suffix) {
      return new[] { data.Roaming + suffix, data.Local + suffix }.Where(Directory.Exists);
    }

    // The suffixes of backups that sit next to a data folder.
    private static IEnumerable<string> Leftovers(string folder) {
      string? parent = Path.GetDirectoryName(folder);
      if (parent is null || !Directory.Exists(parent)) {
        return [];
      }

      string prefix = Path.GetFileName(folder) + _marker;
      return Directory.EnumerateDirectories(parent, prefix + "*")
          .Select(backup => Path.GetFileName(backup)[(prefix.Length - _marker.Length)..]);
    }

    private static void Delete(string entry) {
      if (Directory.Exists(entry)) {
        Directory.Delete(entry, recursive: true);
      } else {
        File.Delete(entry);
      }
    }
  }
}
