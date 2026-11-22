using Navpaky.App.Feedback;
using Xunit;

namespace Navpaky.App.Tests.Feedback {
  public sealed class SplashScriptTests {
    [Fact]
    public void TypesTheNameAsWrongThenShowsTheGreetingWithThePill() {
      IReadOnlyList<SplashFrame> frames = SplashScript.Frames("Привіт", animate: true);

      SplashFrame[] typed = [.. frames.Where(frame => frame.Look == SplashLook.Wrong)];
      Assert.Equal(["N", "Na", "Nav", "Navp", "Navpa", "Navpak", "Navpaky"],
          typed.Select(frame => frame.Text));
      SplashFrame corrected = frames.Single(frame => frame.Look == SplashLook.Corrected);
      Assert.Equal("Привіт", corrected.Text);
      Assert.True(corrected.Pill);
      Assert.True(corrected.At > typed[^1].At);
    }

    [Fact]
    public void EndsAfterAboutThreeSeconds() {
      SplashFrame last = SplashScript.Frames("Hello", animate: true)[^1];

      Assert.Equal(SplashLook.Gone, last.Look);
      Assert.InRange(last.At.TotalSeconds, 2.5, 3.5);
    }

    [Fact]
    public void WithoutAnimationsShowsTheGreetingThenGoes() {
      IReadOnlyList<SplashFrame> frames = SplashScript.Frames("Hello", animate: false);

      Assert.Equal(2, frames.Count);
      Assert.Equal(("Hello", SplashLook.Settled, true), (frames[0].Text, frames[0].Look,
          frames[0].Pill));
      Assert.Equal(SplashLook.Gone, frames[1].Look);
    }
  }
}
