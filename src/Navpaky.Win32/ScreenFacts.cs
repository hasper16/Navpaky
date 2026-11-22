using System.Drawing;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Navpaky.Win32 {
  /// <summary>
  /// Reads where the text caret and the mouse pointer are, and prepares the cursor indicator's
  /// window, in physical screen pixels.
  /// </summary>
  public static class ScreenFacts {
    /// <summary>
    /// Returns the caret of the foreground window on screen, if that window shows a system
    /// caret.
    /// </summary>
    /// <returns>
    /// The caret's left, top, right and bottom edges, or <see langword="null" /> when the
    /// foreground app draws its own caret, as many browsers and Electron apps do.
    /// </returns>
    public static (int Left, int Top, int Right, int Bottom)? Caret() {
      HWND foreground = PInvoke.GetForegroundWindow();
      if (foreground.IsNull) {
        return null;
      }

      uint thread = PInvoke.GetWindowThreadProcessId(foreground, out _);
      var info = new GUITHREADINFO { cbSize = (uint)Marshal.SizeOf<GUITHREADINFO>() };
      if (!PInvoke.GetGUIThreadInfo(thread, ref info) || info.hwndCaret.IsNull) {
        return null;
      }

      RECT caret = info.rcCaret;
      var topLeft = new Point(caret.left, caret.top);
      var bottomRight = new Point(caret.right, caret.bottom);
      if (!PInvoke.ClientToScreen(info.hwndCaret, ref topLeft)
          || !PInvoke.ClientToScreen(info.hwndCaret, ref bottomRight)) {
        return null;
      }

      return (topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
    }

    /// <summary>
    /// Returns where the mouse pointer is.
    /// </summary>
    /// <returns>The pointer's hot spot.</returns>
    public static (int X, int Y) Mouse() {
      return PInvoke.GetCursorPos(out Point point) ? (point.X, point.Y) : (0, 0);
    }

    /// <summary>
    /// Returns the work area of the monitor that shows a point: the screen without the taskbar.
    /// </summary>
    /// <param name="x">The point's horizontal position.</param>
    /// <param name="y">The point's vertical position.</param>
    /// <returns>The work area's left, top, right and bottom edges.</returns>
    public static (int Left, int Top, int Right, int Bottom) WorkAreaAt(int x, int y) {
      HMONITOR monitor = PInvoke.MonitorFromPoint(
          new Point(x, y), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
      var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
      if (!PInvoke.GetMonitorInfo(monitor, ref info)) {
        return (x - 1000, y - 1000, x + 1000, y + 1000);
      }

      RECT work = info.rcWork;
      return (work.left, work.top, work.right, work.bottom);
    }

    /// <summary>
    /// Gives the focus back to a window the user worked in; Windows allows it while Navpaky's
    /// own window is in front.
    /// </summary>
    /// <param name="window">The window's handle.</param>
    /// <returns><see langword="true" /> when the window came to the front.</returns>
    public static bool Activate(nint window) {
      return window != 0 && PInvoke.SetForegroundWindow((HWND)window);
    }

    /// <summary>
    /// Makes a window an overlay that never takes focus, lets clicks through and stays out of
    /// the taskbar and Alt+Tab (design D5 of add-tray-and-settings-ui).
    /// </summary>
    /// <param name="window">The window handle.</param>
    public static void MakeOverlay(nint window) {
      var handle = new HWND(window);
      int style = PInvoke.GetWindowLong(handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
      const WINDOW_EX_STYLE overlay = WINDOW_EX_STYLE.WS_EX_NOACTIVATE
          | WINDOW_EX_STYLE.WS_EX_TRANSPARENT | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
      _ = PInvoke.SetWindowLong(handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, style | (int)overlay);
    }
  }
}
