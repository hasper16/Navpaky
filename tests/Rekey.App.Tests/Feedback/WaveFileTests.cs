using System.IO;
using Rekey.App.Feedback;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Feedback {
  // Re-check of fix 2.7 (add-final-acceptance): the silence that keeps monitor audio awake is
  // built from a shipped sound's format, and each sound's length tells when silence resumes.
  public sealed class WaveFileTests {
    [Fact]
    public void ChimeLastsItsSamples() {
      Assert.Equal(350, WaveFile.Length(Read(SwitchSound.Chime)).TotalMilliseconds, 0);
    }

    [Fact]
    public void SilenceHasTheFormatAndLengthAskedFor() {
      byte[] chime = Read(SwitchSound.Chime);

      byte[] silence = WaveFile.Silence(chime, TimeSpan.FromSeconds(1));

      Assert.Equal(1000, WaveFile.Length(silence).TotalMilliseconds, 0);
      Assert.Equal(chime[8..36], silence[8..36]);
      Assert.Equal(silence.Length - 8, BitConverter.ToInt32(silence, 4));
      Assert.All(silence[44..], b => Assert.Equal(0, b));
    }

    private static byte[] Read(SwitchSound sound) {
      using Stream wave = SwitchSoundPlayer.Open(sound)!;
      using var bytes = new MemoryStream();
      wave.CopyTo(bytes);
      return bytes.ToArray();
    }
  }
}
