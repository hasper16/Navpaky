using Windows.Win32;

namespace Navpaky.Win32 {
  /// <summary>
  /// Reads the clock Windows stamps input with: milliseconds since system start, sleep included.
  /// </summary>
  /// <remarks>
  /// Since .NET 11, <see cref="Environment.TickCount64" /> leaves out time spent asleep, so it
  /// drifts from the times Windows reports for keys, WinEvents and the last input after each wake.
  /// </remarks>
  public static class InputClock {
    /// <summary>
    /// Gets the milliseconds since system start, sleep and hibernation included.
    /// </summary>
    public static long Now => (long)PInvoke.GetTickCount64();

    /// <summary>
    /// Widens a 32-bit Windows input time to the 64-bit clock, taking it as at most about 49 days
    /// before now.
    /// </summary>
    /// <param name="time">A time Windows reported in 32-bit <c>GetTickCount</c> ticks.</param>
    /// <returns>The same moment on <see cref="Now" />'s scale.</returns>
    public static long Widen(uint time) {
      long now = Now;
      return now - unchecked((uint)now - time);
    }
  }
}
