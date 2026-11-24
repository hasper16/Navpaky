using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;

namespace Navpaky.Win32 {
  /// <summary>
  /// Creates and removes Navpaky's desktop shortcut through the Windows shell link object
  /// (add-install-wizard, design D6).
  /// </summary>
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "The shell link object takes and fills raw UTF-16 buffers.")]
  public static unsafe class DesktopShortcut {
    /// <summary>
    /// The file name of Navpaky's shortcut.
    /// </summary>
    public const string FileName = "Navpaky.lnk";

    private const string _launcher = "Navpaky.exe";

    // MAX_PATH: the shell link object reports its target in a buffer of this size.
    private const int _pathLength = 260;

    /// <summary>
    /// Creates or replaces a shortcut that starts the install's root launcher.
    /// </summary>
    /// <param name="linkPath">The full path of the <c>.lnk</c> file.</param>
    /// <param name="installFolder">The install root that holds <c>Navpaky.exe</c>.</param>
    public static void Create(string linkPath, string installFolder) {
      string target = Path.Join(installFolder, _launcher);
      var link = (IShellLinkW)new ShellLink();
      try {
        fixed (char* path = target) {
          link.SetPath(path);
          link.SetIconLocation(path, 0);
        }

        fixed (char* folder = installFolder) {
          link.SetWorkingDirectory(folder);
        }

        fixed (char* file = linkPath) {
          ((IPersistFile)link).Save(file, true);
        }
      } finally {
        Marshal.ReleaseComObject(link);
      }
    }

    /// <summary>
    /// Removes the shortcut when it starts something inside the install folder; a shortcut the
    /// user pointed elsewhere stays.
    /// </summary>
    /// <param name="linkPath">The full path of the <c>.lnk</c> file.</param>
    /// <param name="installFolder">The install root.</param>
    public static void Remove(string linkPath, string installFolder) {
      string root = Path.TrimEndingDirectorySeparator(installFolder) + Path.DirectorySeparatorChar;
      if (TargetOf(linkPath) is { } target
          && target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) {
        File.Delete(linkPath);
      }
    }

    /// <summary>
    /// Returns the program a shortcut starts.
    /// </summary>
    /// <param name="linkPath">The full path of the <c>.lnk</c> file.</param>
    /// <returns>
    /// The target's full path, or <see langword="null" /> when there is no shortcut.
    /// </returns>
    public static string? TargetOf(string linkPath) {
      if (!File.Exists(linkPath)) {
        return null;
      }

      var link = (IShellLinkW)new ShellLink();
      try {
        char* path = stackalloc char[_pathLength];
        fixed (char* file = linkPath) {
          ((IPersistFile)link).Load(file, STGM.STGM_READ);
        }

        WIN32_FIND_DATAW found = default;
        link.GetPath(path, _pathLength, &found, 0);
        return new string(path);
      } finally {
        Marshal.ReleaseComObject(link);
      }
    }
  }
}
