using System.Runtime.CompilerServices;
using Rekey.App.Telemetry;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Design D1: a freeze report carries the stalled thread's managed stack, never a dump.
  public sealed class ThreadStacksTests {
    [Fact]
    public void ReadsTheStackOfABlockedThread() {
      using var release = new ManualResetEventSlim();
      using var parked = new ManualResetEventSlim();
      var thread = new Thread(() => ParkHere(parked, release)) { IsBackground = true };
      thread.Start();
      parked.Wait(TestContext.Current.CancellationToken);

      var frames = ThreadStacks.Of(thread.ManagedThreadId);
      release.Set();
      thread.Join();

      Assert.Contains(frames, frame => frame.Function?.EndsWith(
          nameof(ParkHere), StringComparison.Ordinal) == true
          && frame.Module == "Rekey.App.Tests");
      Assert.All(frames, frame => Assert.DoesNotContain(
          Environment.UserName, frame.Module ?? string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void UnknownThreadHasNoFrames() {
      Assert.Empty(ThreadStacks.Of(int.MaxValue));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ParkHere(ManualResetEventSlim parked, ManualResetEventSlim release) {
      parked.Set();
      release.Wait();
    }
  }
}
