namespace Rekey.App.Feedback {
  // A rectangle on the screen, in physical pixels; Right and Bottom lie outside it.
  internal readonly record struct PixelRect(int Left, int Top, int Right, int Bottom);
}
