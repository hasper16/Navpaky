using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Rekey.Win32 {
  /// <summary>
  /// Reads the user's known folders that .NET does not name.
  /// </summary>
  public static class KnownFolders {
    /// <summary>
    /// Returns the user's Downloads folder, which the user may have moved.
    /// </summary>
    /// <returns>The full path, or <see langword="null" /> when Windows cannot name it.</returns>
    [SuppressMessage(
        "Reliability",
        "S3869:\"SafeHandle.DangerousGetHandle\" should not be called",
        Justification = "The string is copied while the using block still owns the handle.")]
    public static string? Downloads() {
      HRESULT result = PInvoke.SHGetKnownFolderPath(
          PInvoke.FOLDERID_Downloads,
          KNOWN_FOLDER_FLAG.KF_FLAG_DEFAULT,
          null,
          out CoTaskMemFreePWSTRSafeHandle path);
      using (path) {
        return result.Succeeded ? Marshal.PtrToStringUni(path.DangerousGetHandle()) : null;
      }
    }
  }
}
