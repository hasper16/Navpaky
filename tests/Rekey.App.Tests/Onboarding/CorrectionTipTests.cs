using Rekey.App.Onboarding;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class CorrectionTipTests {
    [Fact]
    public void FirstRealCorrectionShowsTheTip() {
      Assert.True(CorrectionTip.IsDue(new Settings(), inTryIt: false, canNotify: true));
    }

    [Fact]
    public void TipShowsOnlyOnce() {
      Assert.False(CorrectionTip.IsDue(
          new Settings(CorrectionTipShown: true), inTryIt: false, canNotify: true));
    }

    [Fact]
    public void CorrectionInTheTryItFieldShowsNoTip() {
      Assert.False(CorrectionTip.IsDue(new Settings(), inTryIt: true, canNotify: true));
    }

    // Fix 2.11 of add-final-acceptance: in Do Not Disturb Windows showed nothing, yet the tip
    // counted as shown and never came again.
    [Fact]
    public void TipWaitsWhileWindowsShowsNoNotifications() {
      Assert.False(CorrectionTip.IsDue(new Settings(), inTryIt: false, canNotify: false));
    }

    [Fact]
    public void ShowingTheTipRemembersIt() {
      Settings after = CorrectionTip.Shown(new Settings());

      Assert.False(CorrectionTip.IsDue(after, inTryIt: false, canNotify: true));
    }
  }
}
