using System.IO;
using System.Windows.Threading;
using Navpaky.Core;
using Navpaky.Win32;

namespace Navpaky.App.Feedback {
  // Plays the switch sounds Navpaky ships (design D6), loaded once so a Correction never waits
  // for the disk. The sounds are Navpaky's own (ADR 0003). They go into one output stream that
  // silence keeps open while the user types (SoundScheduler), so monitor audio never sees a
  // new stream start for a sound. Used on the UI thread only.
  internal sealed class SwitchSoundPlayer : IDisposable {
    private const int _sampleRate = 44100;

    private readonly Dictionary<SwitchSound, (short[] Samples, TimeSpan Length)> _sounds = [];
    private readonly SoundOutput _output = new(_sampleRate);
    private readonly SoundScheduler _scheduler;

    public SwitchSoundPlayer() {
      foreach (SwitchSound sound in Enum.GetValues<SwitchSound>()) {
        if (Read(sound) is { } wave && WaveFile.SampleRate(wave) == _sampleRate) {
          _sounds[sound] = (WaveFile.Samples(wave), WaveFile.Length(wave));
        }
      }

      _scheduler = new SoundScheduler(
          _output.Open,
          _output.Close,
          (sound, _) => _output.Play(_sounds[sound].Samples),
          Later);
    }

    // For the desktop checks: how many sounds played since start.
    public int Played { get; private set; }

    public static Stream? Open(SwitchSound sound) {
      string name = sound switch {
        SwitchSound.KeyClick => "key-click",
        SwitchSound.SoftTick => "soft-tick",
        _ => "chime",
      };
      return typeof(SwitchSoundPlayer).Assembly
          .GetManifestResourceStream($"Navpaky.Sounds.{name}.wav");
    }

    public void Play(SwitchSound sound) {
      if (_sounds.TryGetValue(sound, out (short[] Samples, TimeSpan Length) loaded)) {
        Played++;
        _scheduler.Play(sound, loaded.Length);
      }
    }

    // The user is typing, so a Correction and its sound may come soon.
    public void KeepAwake() {
      _scheduler.KeepAwake();
    }

    public void Dispose() {
      _output.Dispose();
    }

    private static byte[]? Read(SwitchSound sound) {
      using Stream? wave = Open(sound);
      if (wave is null) {
        return null;
      }

      using var bytes = new MemoryStream();
      wave.CopyTo(bytes);
      return bytes.ToArray();
    }

    private static void Later(TimeSpan delay, Action then) {
      var timer = new DispatcherTimer { Interval = delay };
      timer.Tick += (_, _) => {
        timer.Stop();
        then();
      };
      timer.Start();
    }
  }
}
