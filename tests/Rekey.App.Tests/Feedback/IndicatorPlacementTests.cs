using Rekey.App.Feedback;
using Xunit;

namespace Rekey.App.Tests.Feedback {
  public sealed class IndicatorPlacementTests {
    private const int _size = 32;
    private static readonly PixelRect _screen = new(0, 0, 1920, 1040);

    [Fact]
    public void BadgeSitsBelowTheCaret() {
      var caret = new PixelRect(500, 300, 502, 320);

      PixelPoint at = IndicatorPlacement.Place(caret, new PixelPoint(10, 10), _size, _screen);

      Assert.Equal(new PixelPoint(500, 324), at);
    }

    [Fact]
    public void BadgeGoesAboveACaretAtTheBottomOfTheScreen() {
      var caret = new PixelRect(500, 1010, 502, 1030);

      PixelPoint at = IndicatorPlacement.Place(caret, new PixelPoint(10, 10), _size, _screen);

      Assert.Equal(new PixelPoint(500, 1010 - 4 - _size), at);
    }

    [Fact]
    public void BadgeStaysOnScreenAtTheRightEdge() {
      var caret = new PixelRect(1915, 300, 1917, 320);

      PixelPoint at = IndicatorPlacement.Place(caret, new PixelPoint(10, 10), _size, _screen);

      Assert.Equal(1920 - _size, at.X);
    }

    [Fact]
    public void WithoutACaretTheBadgeFollowsTheMousePointer() {
      PixelPoint at = IndicatorPlacement.Place(null, new PixelPoint(800, 600), _size, _screen);

      Assert.Equal(new PixelPoint(816, 620), at);
    }
  }
}
