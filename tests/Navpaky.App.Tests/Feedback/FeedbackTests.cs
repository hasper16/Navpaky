using Navpaky.App.Feedback;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Xunit;

namespace Navpaky.App.Tests.Feedback {
  public sealed class FeedbackTests {
    private static readonly LayoutId _russian = new(0x00000419);
    private static readonly Settings _soundOn = new(PlaySwitchSound: true);

    [Theory]
    [InlineData(SwitchCause.Correction)]
    [InlineData(SwitchCause.Undo)]
    [InlineData(SwitchCause.LayoutSwitch)]
    public void IndicatorFollowsEverySwitchByDefault(SwitchCause cause) {
      var feedback = new SwitchFeedback(_russian, cause);

      Assert.True(SwitchResponse.For(feedback, new Settings()).Indicator);
    }

    [Fact]
    public void IndicatorCanBeTurnedOff() {
      var settings = new Settings(ShowCursorIndicator: false);

      Assert.False(SwitchResponse.For(Correction(), settings).Indicator);
    }

    [Fact]
    public void SoundIsOffByDefault() {
      Assert.False(SwitchResponse.For(Correction(), new Settings()).Sound);
    }

    [Fact]
    public void SoundFollowsACorrection() {
      Assert.True(SwitchResponse.For(Correction(), _soundOn).Sound);
    }

    [Theory]
    [InlineData(SwitchCause.Undo)]
    [InlineData(SwitchCause.LayoutSwitch)]
    public void NoSoundWithoutACorrection(SwitchCause cause) {
      Assert.False(SwitchResponse.For(new SwitchFeedback(_russian, cause), _soundOn).Sound);
    }

    private static SwitchFeedback Correction() {
      return new SwitchFeedback(_russian, SwitchCause.Correction);
    }
  }
}
