using Rekey.App.Telemetry;
using Rekey.Core.Tests;
using Xunit;

namespace Rekey.App.Tests.Telemetry {
  // Spec telemetry: "Crash and freeze reports"; design D1 (500 ms ping, 2 s threshold).
  public sealed class DispatcherFreezeWatchTests {
    private readonly ManualClock _clock = new();
    private readonly Queue<Action> _dispatcher = new();
    private int _freezes;

    [Fact]
    public void DispatcherBlockedForThreeSecondsIsOneFreeze() {
      using DispatcherFreezeWatch watch = Started();

      Run(TimeSpan.FromSeconds(2));
      _clock.Advance(TimeSpan.FromSeconds(3));
      Run(TimeSpan.FromSeconds(5));

      Assert.Equal(1, _freezes);
    }

    [Fact]
    public void ResponsiveDispatcherIsNoFreeze() {
      using DispatcherFreezeWatch watch = Started();

      Run(TimeSpan.FromSeconds(30));

      Assert.Equal(0, _freezes);
    }

    [Fact]
    public void BlockOfTwoSecondsIsNoFreeze() {
      using DispatcherFreezeWatch watch = Started();

      Run(TimeSpan.FromSeconds(1));
      _clock.Advance(TimeSpan.FromSeconds(2));
      Run(TimeSpan.FromSeconds(2));

      Assert.Equal(0, _freezes);
    }

    [Fact]
    public void TwoSeparateBlocksAreTwoFreezes() {
      using DispatcherFreezeWatch watch = Started();

      _clock.Advance(TimeSpan.FromSeconds(3));
      Run(TimeSpan.FromSeconds(2));
      _clock.Advance(TimeSpan.FromSeconds(3));
      Run(TimeSpan.FromSeconds(2));

      Assert.Equal(2, _freezes);
    }

    private DispatcherFreezeWatch Started() {
      var watch = new DispatcherFreezeWatch(_clock, _dispatcher.Enqueue, () => _freezes++);
      watch.Start();
      return watch;
    }

    // A responsive dispatcher: it answers each ping right after the watch sends it.
    private void Run(TimeSpan duration) {
      DateTimeOffset end = _clock.Now + duration;
      while (_clock.Now < end) {
        _clock.Advance(TimeSpan.FromMilliseconds(100));
        while (_dispatcher.TryDequeue(out Action? ping)) {
          ping();
        }
      }
    }
  }
}
