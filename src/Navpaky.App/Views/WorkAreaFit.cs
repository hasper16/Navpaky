using System.Windows;

namespace Navpaky.App.Views {
  // Keeps a window that sizes to its content inside the screen's work area: such a window keeps
  // its top as it grows, so its bottom buttons can end up under the taskbar.
  internal static class WorkAreaFit {
    public static double MaxHeight(double designed, Rect workArea) {
      return Math.Min(designed, workArea.Height);
    }

    public static double Top(double top, double height, Rect workArea) {
      return Math.Max(workArea.Top, Math.Min(top, workArea.Bottom - height));
    }

    // Applies both to a window, now and whenever its size changes.
    public static void Attach(Window window) {
      Rect area = SystemParameters.WorkArea;
      window.MaxHeight = MaxHeight(window.MaxHeight, area);
      window.SizeChanged += (_, _) => {
        window.Top = Top(window.Top, window.ActualHeight, SystemParameters.WorkArea);
      };
    }
  }
}
