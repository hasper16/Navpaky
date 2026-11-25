using System.IO;

namespace Navpaky.Setup.Install {
  // The real file system for the folder rules.
  internal sealed class DiskFolders : IFolderProbe {
    public bool HasEntries(string folder) {
      return Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any();
    }

    public IReadOnlyList<string>? Entries(string folder) {
      try {
        return Directory.EnumerateFileSystemEntries(folder)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return null;
      }
    }

    public bool FileExists(string path) {
      return File.Exists(path);
    }

    // Creates and deletes a probe file: folder rights and Controlled folder access both show.
    public bool CanWrite(string folder) {
      string? existing = folder;
      while (existing is not null && !Directory.Exists(existing)) {
        existing = Path.GetDirectoryName(existing);
      }

      if (existing is null) {
        return false;
      }

      try {
        using var probe = new FileStream(
            Path.Join(existing, $".navpaky-probe-{Guid.NewGuid():N}"),
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            1,
            FileOptions.DeleteOnClose);
        return true;
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return false;
      }
    }

    // A path with no drive letter, such as a network share, counts as having no room.
    public long FreeBytes(string folder) {
      try {
        return new DriveInfo(Path.GetPathRoot(Path.GetFullPath(folder))!).AvailableFreeSpace;
      } catch (Exception e) when (e is ArgumentException or IOException) {
        return 0;
      }
    }
  }
}
