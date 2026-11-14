using System.IO;
using Rekey.App.Feedback;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Feedback {
  public sealed class SwitchSoundPlayerTests {
    [Theory]
    [InlineData(SwitchSound.KeyClick)]
    [InlineData(SwitchSound.SoftTick)]
    [InlineData(SwitchSound.Chime)]
    public void EverySoundShipsAsAWave(SwitchSound sound) {
      using Stream? wave = SwitchSoundPlayer.Open(sound);

      Assert.NotNull(wave);
      byte[] header = new byte[12];
      wave.ReadExactly(header);
      Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header, 0, 4));
      Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(header, 8, 4));
    }
  }
}
