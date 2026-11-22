using Navpaky.App.Feedback;
using Xunit;

namespace Navpaky.App.Tests.Feedback {
  // Re-check of fix 2.7 (add-final-acceptance): monitor audio over DisplayPort or HDMI drops a
  // sound that starts while its link sleeps. Silence keeps the link awake while the user types,
  // and a sound that finds it asleep waits until silence has woken it.
  public sealed class SoundSchedulerTests {
    private static readonly TimeSpan _sound = TimeSpan.FromMilliseconds(350);

    private readonly List<string> _log = [];
    private readonly List<(TimeSpan Delay, Action Then)> _later = [];
    private readonly SoundScheduler _scheduler;

    public SoundSchedulerTests() {
      _scheduler = new SoundScheduler(
          () => _log.Add("silence"),
          () => _log.Add("stop"),
          (sound, _) => _log.Add(sound.ToString()),
          (delay, then) => _later.Add((delay, then)));
    }

    [Fact]
    public void SoundAfterTypingPlaysAtOnce() {
      _scheduler.KeepAwake();

      _scheduler.Play("chime", _sound);

      Assert.Equal(["silence", "chime"], _log);
    }

    [Fact]
    public void SoundWhileAsleepWaitsForTheLinkToWake() {
      _scheduler.Play("chime", _sound);

      Assert.Equal(["silence"], _log);
      _later.Single(item => item.Delay == SoundScheduler.WakeUp).Then();
      Assert.Equal(["silence", "chime"], _log);
    }

    [Fact]
    public void SilenceStopsAfterTypingPauses() {
      _scheduler.KeepAwake();

      _later.Single(item => item.Delay == SoundScheduler.KeepAwakeFor).Then();

      Assert.Equal(["silence", "stop"], _log);
    }

    [Fact]
    public void LaterTypingKeepsTheSilenceGoing() {
      _scheduler.KeepAwake();
      Action firstSleep = _later.Single().Then;
      _scheduler.KeepAwake();

      firstSleep();

      Assert.Equal(["silence"], _log);
    }

    [Fact]
    public void SilenceResumesOnceTheSoundIsOver() {
      _scheduler.KeepAwake();
      _scheduler.Play("chime", _sound);

      _later.Single(item => item.Delay == _sound).Then();

      Assert.Equal(["silence", "chime", "silence"], _log);
    }
  }
}
