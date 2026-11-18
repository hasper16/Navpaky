using Navpaky.Core;

namespace Navpaky.App.Feedback {
  // What the user sees and hears after Navpaky switched the Layout: the cursor indicator after
  // every switch it made, the sound after a Correction only (spec tray-and-settings).
  internal readonly record struct SwitchResponse(bool Indicator, bool Sound) {
    public static SwitchResponse For(SwitchFeedback feedback, Settings settings) {
      return new SwitchResponse(
          settings.ShowCursorIndicator,
          settings.PlaySwitchSound && feedback.Cause == SwitchCause.Correction);
    }
  }
}
