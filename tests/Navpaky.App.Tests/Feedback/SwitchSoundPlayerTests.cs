using System.IO;
using Navpaky.App.Feedback;
using Navpaky.Core;
using Xunit;

namespace Navpaky.App.Tests.Feedback {
  public sealed class SwitchSoundPlayerTests {
    public static TheoryData<SwitchSound> Sounds => [.. Enum.GetValues<SwitchSound>()];

    [Theory]
    [MemberData(nameof(Sounds))]
    public void EverySoundShipsAsAWaveBothWays(SwitchSound sound) {
      foreach (bool towardEnglish in new[] { false, true }) {
        using Stream? wave = SwitchSoundPlayer.Open(SwitchSoundPlayer.Clip(sound, towardEnglish));

        Assert.NotNull(wave);
        byte[] header = new byte[12];
        wave.ReadExactly(header);
        Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header, 0, 4));
        Assert.Equal("WAVE", System.Text.Encoding.ASCII.GetString(header, 8, 4));
      }
    }

    [Fact]
    public void NavpakyRisesAwayFromEnglishAndFallsTowardIt() {
      Assert.Equal("navpaky-up", SwitchSoundPlayer.Clip(SwitchSound.Navpaky, false));
      Assert.Equal("navpaky-down", SwitchSoundPlayer.Clip(SwitchSound.Navpaky, true));
    }

    [Fact]
    public void OtherSoundsIgnoreTheDirection() {
      Assert.Equal(
          SwitchSoundPlayer.Clip(SwitchSound.Bandura, towardEnglish: false),
          SwitchSoundPlayer.Clip(SwitchSound.Bandura, towardEnglish: true));
    }
  }
}
