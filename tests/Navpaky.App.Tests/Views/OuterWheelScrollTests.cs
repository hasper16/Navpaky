using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Navpaky.App.Views;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Fix 2.11 of add-final-acceptance: the welcome window scrolled only over its scroll bar, as
  // the Hotkey page inside it is a scroll viewer of its own that takes the wheel.
  [Collection(nameof(Sta))]
  public sealed class OuterWheelScrollTests {
    [Fact]
    public void WheelOverAnInnerScrollViewerScrollsTheOuterOne() {
      Sta.Run(() => {
        var inner = new ScrollViewer { Content = new Border { Height = 50 } };
        var outer = new ScrollViewer {
          Content = new StackPanel { Children = { inner, new Border { Height = 1000 } } },
        };
        OuterWheelScroll.Attach(outer);
        outer.Measure(new Size(200, 200));
        outer.Arrange(new Rect(0, 0, 200, 200));

        inner.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120) {
          RoutedEvent = UIElement.PreviewMouseWheelEvent,
        });
        outer.UpdateLayout();

        Assert.True(outer.VerticalOffset > 0);
      });
    }
  }
}
