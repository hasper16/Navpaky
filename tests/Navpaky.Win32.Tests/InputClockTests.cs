using Windows.Win32;
using Xunit;

namespace Navpaky.Win32.Tests {
  // Windows stamps keys, WinEvents and the last input with 32-bit GetTickCount time, which counts
  // sleep; the clock must count the same way, or every comparison drifts after a wake.
  public sealed class InputClockTests {
    [Fact]
    public void WindowsInputTimeWidensToTheClocksNow() {
      long before = InputClock.Now;
      long widened = InputClock.Widen(PInvoke.GetTickCount());
      long after = InputClock.Now;

      Assert.InRange(widened, before, after);
    }
  }
}
