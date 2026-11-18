using System.Windows.Controls;
using System.Windows.Input;

namespace Rekey.App.Views {
  // Makes the wheel scroll a window's main scroll viewer anywhere over it. A scroll viewer
  // inside it, such as the Hotkey page's, takes the wheel even when it has nothing to scroll.
  internal static class OuterWheelScroll {
    // Pixels per wheel notch, as Windows scrolls three lines of about 16 px.
    private const double _pixelsPerNotch = 48;

    public static void Attach(ScrollViewer scroller) {
      scroller.PreviewMouseWheel += (_, e) => {
        scroller.ScrollToVerticalOffset(
            scroller.VerticalOffset - (e.Delta / (double)Mouse.MouseWheelDeltaForOneLine
                * _pixelsPerNotch));
        e.Handled = true;
      };
    }
  }
}
