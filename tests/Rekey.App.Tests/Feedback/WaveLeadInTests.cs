using System.IO;
using Rekey.App.Feedback;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Feedback {
  // Re-check of fix 2.7 (add-final-acceptance): monitor audio over HDMI or DisplayPort drops
  // the first part of a sound after silence, so each sound starts with silence it can lose.
  public sealed class WaveLeadInTests {
    [Theory]
    [InlineData(SwitchSound.KeyClick)]
    [InlineData(SwitchSound.SoftTick)]
    [InlineData(SwitchSound.Chime)]
    public void SilenceComesBeforeTheUnchangedSound(SwitchSound sound) {
      byte[] original = Read(sound);

      byte[] led = WaveLeadIn.Add(original, TimeSpan.FromMilliseconds(300));

      // 44.1 kHz, 16-bit mono: 300 ms is 13230 samples of 2 bytes.
      const int silence = 26460;
      Assert.Equal(original.Length + silence, led.Length);
      Assert.Equal(led.Length - 8, BitConverter.ToInt32(led, 4));
      int data = DataStart(led);
      Assert.Equal(DataLength(original) + silence, BitConverter.ToInt32(led, data - 4));
      Assert.All(led[data..(data + silence)], b => Assert.Equal(0, b));
      Assert.Equal(original[DataStart(original)..], led[(data + silence)..]);
    }

    private static byte[] Read(SwitchSound sound) {
      using Stream wave = SwitchSoundPlayer.Open(sound)!;
      using var bytes = new MemoryStream();
      wave.CopyTo(bytes);
      return bytes.ToArray();
    }

    private static int DataStart(byte[] wave) {
      int at = 12;
      while (System.Text.Encoding.ASCII.GetString(wave, at, 4) != "data") {
        at += 8 + BitConverter.ToInt32(wave, at + 4);
      }

      return at + 8;
    }

    private static int DataLength(byte[] wave) {
      return BitConverter.ToInt32(wave, DataStart(wave) - 4);
    }
  }
}
