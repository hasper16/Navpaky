using Navpaky.Core.Languages;
using Xunit;

namespace Navpaky.Core.Tests {
  public sealed class SwitchFeedbackTests {
    private static readonly LayoutId _russian = new(0x00000419);
    private static readonly TimeSpan _now = TimeSpan.FromSeconds(1);

    [Fact]
    public void SwitchAfterAnAppliedReplacementIsACorrection() {
      SwitchFeedback? feedback = SwitchFeedback.Of(
          new ReplacementFinished(_now, Applied: true), [new SwitchLayout(_russian)]);

      Assert.Equal(new SwitchFeedback(_russian, SwitchCause.Correction), feedback);
    }

    [Fact]
    public void SwitchWithAnUndoIsAnUndo() {
      SwitchFeedback? feedback = SwitchFeedback.Of(
          new ReplacementFinished(_now, Applied: true),
          [new SwitchLayout(_russian), new UndoPerformed("vexo", "мучо")]);

      Assert.Equal(SwitchCause.Undo, feedback?.Cause);
    }

    [Fact]
    public void HotkeyWithNothingToCorrectOnlySwitchesTheLayout() {
      SwitchFeedback? feedback = SwitchFeedback.Of(
          new ManualCorrectionRequested(_now), [new SwitchLayout(_russian)]);

      Assert.Equal(new SwitchFeedback(_russian, SwitchCause.LayoutSwitch), feedback);
    }

    [Fact]
    public void NoSwitchNoFeedback() {
      Assert.Null(SwitchFeedback.Of(
          new ReplacementFinished(_now, Applied: true), [new WordListsChanged()]));
    }
  }
}
