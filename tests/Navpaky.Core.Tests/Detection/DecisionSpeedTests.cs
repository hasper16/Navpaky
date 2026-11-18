using System.Diagnostics;
using Navpaky.Core.Detection;
using Xunit;
using static Navpaky.Core.Tests.Detection.DetectionWorld;

namespace Navpaky.Core.Tests.Detection {
  public sealed class DecisionSpeedTests {
    private const int _runs = 1000;

    [Fact]
    public void ThirtyKeystrokeWordIsDecidedWithinFiveMilliseconds() {
      // 30 keys, three active Languages (spec: Decision speed).
      IReadOnlyList<Keystroke> word =
          Keys(KeyMapFixtures.EnglishUs, "ghbdtnvbhghbdtnvbhghbdtnvbhghb");
      Assert.Equal(30, word.Count);
      Detector.Decide(word, KeyMapFixtures.EnglishUs.Layout, Sensitivity.Balanced);

      var times = new List<TimeSpan>(_runs);
      for (int i = 0; i < _runs; i++) {
        long start = Stopwatch.GetTimestamp();
        Detector.Decide(word, KeyMapFixtures.EnglishUs.Layout, Sensitivity.Balanced);
        times.Add(Stopwatch.GetElapsedTime(start));
      }

      times.Sort();
      Assert.True(
          times[(_runs * 99) / 100] < TimeSpan.FromMilliseconds(5),
          $"p99 was {times[(_runs * 99) / 100].TotalMilliseconds} ms");
    }
  }
}
