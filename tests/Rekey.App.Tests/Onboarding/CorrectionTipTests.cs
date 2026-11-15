using Rekey.App.Onboarding;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
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
