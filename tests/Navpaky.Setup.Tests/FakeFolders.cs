using System.IO;
using Navpaky.Setup.Install;

namespace Navpaky.Setup.Tests {
  // An in-memory file system for the folder rules: files, empty folders, read-only roots and
  // free space.
  internal sealed class FakeFolders : IFolderProbe {
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _folders = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _readOnly = [];

    public long Free { get; set; } = long.MaxValue;

    public void Add(string file) {
      _files.Add(file);
    }

    public void AddFolder(string folder) {
      _folders.Add(folder);
    }

    public void Install(string root) {
      Add(Path.Join(root, "Update.exe"));
      Add(Path.Join(root, "current", "Navpaky.App.exe"));
    }

    public void ReadOnly(string root) {
      _readOnly.Add(root);
    }

    public bool HasEntries(string folder) {
      string prefix = folder.TrimEnd('\\') + '\\';
      return _files.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
          || _folders.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public bool FileExists(string path) {
      return _files.Contains(path);
    }

    public bool CanWrite(string folder) {
      return !_readOnly.Exists(root =>
          folder.StartsWith(root, StringComparison.OrdinalIgnoreCase));
    }

    public long FreeBytes(string folder) {
      return Free;
    }
  }
}
