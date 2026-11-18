using System.IO;
using Rekey.App.Feedback;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Feedback {
  // Re-check of fix 2.7 (add-final-acceptance): the shipped sounds are read into samples for
  // the one open output stream, and each sound's length tells when silence follows.
  public sealed class WaveFileTests {
    [Fact]
    public void ChimeLastsItsSamples() {
      byte[] chime = Read(SwitchSound.Chime);

      Assert.Equal(44100, WaveFile.SampleRate(chime));
      Assert.Equal(15434, WaveFile.Samples(chime).Length);
      Assert.Equal(350, WaveFile.Length(chime).TotalMilliseconds, 0);
    }

    [Theory]
    [InlineData(SwitchSound.KeyClick)]
    [InlineData(SwitchSound.SoftTick)]
    [InlineData(SwitchSound.Chime)]
    public void EverySoundHasTheSameRateAndIsNotSilent(SwitchSound sound) {
      byte[] wave = Read(sound);

      Assert.Equal(44100, WaveFile.SampleRate(wave));
      Assert.Contains(WaveFile.Samples(wave), sample => sample != 0);
    }

    private static byte[] Read(SwitchSound sound) {
      using Stream wave = SwitchSoundPlayer.Open(sound)!;
      using var bytes = new MemoryStream();
      wave.CopyTo(bytes);
      return bytes.ToArray();
    }
  }
}
