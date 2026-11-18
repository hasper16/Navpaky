using Navpaky.App.Onboarding;
using Navpaky.Core;
using Xunit;

namespace Navpaky.App.Tests.Onboarding {
  public sealed class CorrectionTipTests {
    [Fact]
    public void FirstRealCorrectionShowsTheTip() {
      Assert.True(CorrectionTip.IsDue(new Settings(), inTryIt: false));
    }

    [Fact]
    public void TipShowsOnlyOnce() {
      Assert.False(CorrectionTip.IsDue(new Settings(CorrectionTipShown: true), inTryIt: false));
    }

    [Fact]
    public void CorrectionInTheTryItFieldShowsNoTip() {
      Assert.False(CorrectionTip.IsDue(new Settings(), inTryIt: true));
    }

    [Fact]
    public void ShowingTheTipRemembersIt() {
      Settings after = CorrectionTip.Shown(new Settings());

      Assert.False(CorrectionTip.IsDue(after, inTryIt: false));
    }
  }
}
