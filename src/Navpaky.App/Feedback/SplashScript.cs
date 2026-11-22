namespace Navpaky.App.Feedback {
  // The launch splash (design D10 of redesign-visual-identity): the caret types "Navpaky" as a
  // wrong word, it turns into the greeting with the Language pill, settles and fades, about
  // three seconds in all (design decision). Without animations it shows the greeting only.
  internal static class SplashScript {
    private const string _name = "Navpaky";
    private static readonly TimeSpan _start = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan _perLetter = TimeSpan.FromMilliseconds(95);
    private static readonly TimeSpan _beforeFlip = TimeSpan.FromMilliseconds(320);
    private static readonly TimeSpan _corrected = TimeSpan.FromMilliseconds(700);
    private static readonly TimeSpan _settled = TimeSpan.FromMilliseconds(700);
    private static readonly TimeSpan _still = TimeSpan.FromMilliseconds(1400);

    public static IReadOnlyList<SplashFrame> Frames(string greeting, bool animate) {
      if (!animate) {
        return [
            new(TimeSpan.Zero, greeting, SplashLook.Settled, true),
            new(_still, greeting, SplashLook.Gone, true)];
      }

      List<SplashFrame> frames = [];
      TimeSpan at = _start;
      for (int length = 1; length <= _name.Length; length++) {
        frames.Add(new(at, _name[..length], SplashLook.Wrong, false));
        at += _perLetter;
      }

      at += _beforeFlip - _perLetter;
      frames.Add(new(at, greeting, SplashLook.Corrected, true));
      at += _corrected;
      frames.Add(new(at, greeting, SplashLook.Settled, true));
      at += _settled;
      frames.Add(new(at, greeting, SplashLook.Gone, true));
      return frames;
    }
  }
}
