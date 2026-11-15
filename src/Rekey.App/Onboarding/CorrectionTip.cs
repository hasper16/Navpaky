using Rekey.Core;

namespace Rekey.App.Onboarding {
  // Design risk of add-onboarding: users who skipped the welcome learn about Undo from a
  // one-time notice after their first real Correction; the try-it field does not count.
  internal static class CorrectionTip {
    public static bool IsDue(Settings settings, bool inTryIt) {
      return !inTryIt && !settings.CorrectionTipShown;
    }

    public static Settings Shown(Settings settings) {
      return settings with { CorrectionTipShown = true };
    }
  }
}
