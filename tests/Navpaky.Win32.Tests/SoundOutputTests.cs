using Xunit;

namespace Navpaky.Win32.Tests {
  // Re-check of fix 2.7 (add-final-acceptance), on the real audio device: one stream stays
  // open in real time while silence is wanted, and a sound goes into it without a new open.
  // Skipped where Windows has no output device, as on build servers.
  public sealed class SoundOutputTests {
    private const int _sampleRate = 44100;

    [Fact]
    public async Task SoundPlaysInsideTheOneOpenStream() {
      using var output = new SoundOutput(_sampleRate);
      output.Open();
      await Task.Delay(500, TestContext.Current.CancellationToken);
      Assert.SkipWhen(output.Opens == 0, "No audio output device.");

      output.Play(new short[_sampleRate / 10]);
      await Task.Delay(500, TestContext.Current.CancellationToken);
      output.Close();
      await Task.Delay(300, TestContext.Current.CancellationToken);

      Assert.Equal(1, output.Opens);

      // About a second of 40 ms buffers, give or take the device's timing.
      Assert.InRange(output.BuffersQueued, 20, 40);
    }
  }
}
