namespace Rekey.App.Feedback {
  // Where the cursor indicator appears (design D5): under the caret, or above it at the bottom
  // of the screen, or next to the mouse pointer when the caret is unknown; always on screen.
  internal static class IndicatorPlacement {
    private const int _gap = 4;

    // Clear of the arrow pointer, which hangs down and right from its hot spot.
    private const int _pointerWidth = 16;
    private const int _pointerHeight = 20;

    public static PixelPoint Place(PixelRect? caret, PixelPoint mouse, int size, PixelRect screen) {
      int x;
      int y;
      if (caret is { } at) {
        x = at.Left;
        y = at.Bottom + _gap + size <= screen.Bottom ? at.Bottom + _gap : at.Top - _gap - size;
      } else {
        x = mouse.X + _pointerWidth;
        y = mouse.Y + _pointerHeight;
      }

      return new PixelPoint(
          Math.Clamp(x, screen.Left, screen.Right - size),
          Math.Clamp(y, screen.Top, screen.Bottom - size));
    }
  }
}
