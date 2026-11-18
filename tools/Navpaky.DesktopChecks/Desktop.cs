using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Navpaky.DesktopChecks {
  // Finds top-level windows by title and the points to click in them.
  internal static class Desktop {
    public static HWND? FindWindow(string titlePart, TimeSpan timeout) {
      HWND found = HWND.Null;
      EventLog.WaitFor(
          () => {
            found = Find(titlePart);
            return !found.IsNull;
          },
          timeout);
      return found.IsNull ? null : found;
    }

    public static Point Center(HWND window) {
      PInvoke.GetWindowRect(window, out RECT rect);
      return new Point((rect.left + rect.right) / 2, (rect.top + rect.bottom) / 2);
    }

    public static string TitleOf(HWND window) {
      Span<char> title = stackalloc char[256];
      int length = PInvoke.GetWindowText(window, title);
      return title[..Math.Max(length, 0)].ToString();
    }

    // Every visible top-level window with exactly this title.
    public static int CountWindows(string title) {
      int count = 0;
      PInvoke.EnumWindows(
          (window, _) => {
            if (PInvoke.IsWindowVisible(window) && TitleOf(window) == title) {
              count++;
            }

            return true;
          },
          default);
      return count;
    }

    private static HWND Find(string titlePart) {
      HWND found = HWND.Null;
      PInvoke.EnumWindows(
          (window, _) => {
            if (PInvoke.IsWindowVisible(window)
                && TitleOf(window).Contains(titlePart, StringComparison.Ordinal)) {
              found = window;
              return false;
            }

            return true;
          },
          default);
      return found;
    }
  }
}
