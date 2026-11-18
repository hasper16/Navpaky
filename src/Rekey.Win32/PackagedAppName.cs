using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Rekey.Win32 {
  /// <summary>
  /// Reads the name the Start menu shows for a packaged (Store) app.
  /// </summary>
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "The shell returns the name as memory the caller frees.")]
  public static unsafe class PackagedAppName {
    /// <summary>
    /// Returns the display name of a packaged app, in the Windows display language, or
    /// <see langword="null" /> when the shell does not know it.
    /// </summary>
    /// <param name="aumid">The application user model id.</param>
    /// <returns>The app's display name, or <see langword="null" />.</returns>
    public static string? Of(string aumid) {
      try {
        PInvoke.SHCreateItemFromParsingName(
            @"shell:AppsFolder\" + aumid, null, out IShellItem item).ThrowOnFailure();
        try {
          item.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, out PWSTR name);
          try {
            return name.ToString();
          } finally {
            PInvoke.CoTaskMemFree(name.Value);
          }
        } finally {
          Marshal.ReleaseComObject(item);
        }
      } catch (COMException) {
        return null;
      }
    }
  }
}
