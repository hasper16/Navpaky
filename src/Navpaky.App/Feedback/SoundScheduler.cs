using Navpaky.Core;

namespace Navpaky.App.Feedback {
  // Decides when the switch sounds play and when silence keeps the audio link awake (re-check
  // of fix 2.7, add-final-acceptance). Monitor audio over DisplayPort or HDMI sleeps after a
  // pause and drops a sound that starts then; a 300 ms lead-in was not enough on the owner's
  // monitor. Silence runs while the user types and for a few seconds after, so a Correction's
  // sound finds the link awake; a sound that finds it asleep waits for silence to wake it.
  // Used on the UI thread only.
  internal sealed class SoundScheduler {
    // Design decisions: HDMI and DisplayPort sinks take up to about a second to wake, and
    // corrections come within seconds of typing.
    public static readonly TimeSpan WakeUp = TimeSpan.FromSeconds(1);
    public static readonly TimeSpan KeepAwakeFor = TimeSpan.FromSeconds(5);

    private readonly Action _startSilence;
    private readonly Action _stopSilence;
    private readonly Action<SwitchSound, TimeSpan> _playNow;
    private readonly Action<TimeSpan, Action> _later;
    private bool _awake;
    private int _keepAwakeCalls;

    public SoundScheduler(
        Action startSilence,
        Action stopSilence,
        Action<SwitchSound, TimeSpan> playNow,
        Action<TimeSpan, Action> later) {
      _startSilence = startSilence;
      _stopSilence = stopSilence;
      _playNow = playNow;
      _later = later;
    }

    public void KeepAwake() {
      if (!_awake) {
        _awake = true;
        _startSilence();
      }

      int call = ++_keepAwakeCalls;
      _later(KeepAwakeFor, () => {
        if (call == _keepAwakeCalls) {
          _awake = false;
          _stopSilence();
        }
      });
    }

    public void Play(SwitchSound sound, TimeSpan length) {
      if (!_awake) {
        KeepAwake();
        _later(WakeUp, () => Play(sound, length));
        return;
      }

      // Windows plays one sound per process: the sound replaces the silence, which comes back
      // once the sound is over.
      _playNow(sound, length);
      _later(length, () => {
        if (_awake) {
          _startSilence();
        }
      });
    }
  }
}
