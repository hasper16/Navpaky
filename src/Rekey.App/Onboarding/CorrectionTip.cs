using Rekey.Core;

namespace Rekey.App.Onboarding {
  // Design risk of add-onboarding: users who skipped the welcome learn about Undo from a
  // one-time notice after their first real Correction; the try-it field does not count. While
  // Windows shows no notifications (Do Not Disturb, a full-screen app) the tip waits.
  internal static class CorrectionTip {
    public static bool IsDue(Settings settings, bool inTryIt, bool canNotify) {
      return !inTryIt && canNotify && !settings.CorrectionTipShown;
    }

    public static Settings Shown(Settings settings) {
      return settings with { CorrectionTipShown = true };
    }
  }
}
