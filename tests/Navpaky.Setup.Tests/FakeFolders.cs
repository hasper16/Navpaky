using System.IO;
using Navpaky.Setup.Install;

namespace Navpaky.Setup.Tests {
  // An in-memory file system for the folder rules: files, empty folders, read-only roots and
  // free space.
  internal sealed class FakeFolders : IFolderProbe {
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _folders = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _readOnly = [];
    private readonly HashSet<string> _unlistable = new(StringComparer.OrdinalIgnoreCase);

    public long Free { get; set; } = long.MaxValue;

    public void Add(string file) {
      _files.Add(file);
    }

    public void AddFolder(string folder) {
      _folders.Add(folder);
    }

    // The layout Velopack's setup leaves, with our root launcher.
    public void Install(string root) {
      Add(Path.Join(root, "Update.exe"));
      Add(Path.Join(root, "Navpaky.exe"));
      Add(Path.Join(root, "current", "Navpaky.App.exe"));
      Add(Path.Join(root, "packages", "navpaky-app-0.1.0-full.nupkg"));
    }

    public void Unlistable(string folder) {
      _unlistable.Add(folder);
    }

    public void ReadOnly(string root) {
      _readOnly.Add(root);
    }

    public bool HasEntries(string folder) {
      string prefix = folder.TrimEnd('\\') + '\\';
      return _files.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
          || _folders.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<string>? Entries(string folder) {
      if (_unlistable.Contains(folder)) {
        return null;
      }

      string prefix = folder.TrimEnd('\\') + '\\';
      return _files.Concat(_folders)
          .Where(path => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
          .Select(path => path[prefix.Length..].Split('\\')[0])
          .Distinct(StringComparer.OrdinalIgnoreCase)
          .ToList();
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
