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

    private readonly Dictionary<string, (short[] Samples, TimeSpan Length)> _sounds = [];
    private readonly SoundOutput _output = new(_sampleRate);
    private readonly SoundScheduler _scheduler;

    public SwitchSoundPlayer() {
      IEnumerable<string> clips = Enum.GetValues<SwitchSound>()
          .SelectMany(sound => new[] { Clip(sound, false), Clip(sound, true) })
          .Distinct();
      foreach (string clip in clips) {
        if (Read(clip) is { } wave && WaveFile.SampleRate(wave) == _sampleRate) {
          _sounds[clip] = (WaveFile.Samples(wave), WaveFile.Length(wave));
        }
      }

      _scheduler = new SoundScheduler(
          _output.Open,
          _output.Close,
          (clip, _) => _output.Play(_sounds[clip].Samples),
          Later);
    }

    // For the desktop checks: how many sounds played since start.
    public int Played { get; private set; }

    // The file a sound plays; Navpaky has a rising and a falling variant (design D8).
    public static string Clip(SwitchSound sound, bool towardEnglish) {
      return sound switch {
        SwitchSound.KeyClick => "key-click",
        SwitchSound.SoftTick => "soft-tick",
        SwitchSound.Chime => "chime",
        SwitchSound.Bandura => "bandura",
        SwitchSound.WoodenSpoons => "wooden-spoons",
        SwitchSound.Drop => "drop",
        SwitchSound.Glass => "glass",
        SwitchSound.Cymbals => "cymbals",
        SwitchSound.Sopilka => "sopilka",
        SwitchSound.JawHarp => "jaw-harp",
        SwitchSound.FrameDrum => "frame-drum",
        _ => towardEnglish ? "navpaky-down" : "navpaky-up",
      };
    }

    public static Stream? Open(string clip) {
      return typeof(SwitchSoundPlayer).Assembly
          .GetManifestResourceStream($"Navpaky.Sounds.{clip}.wav");
    }

    public void Play(SwitchSound sound, bool towardEnglish) {
      string clip = Clip(sound, towardEnglish);
      if (_sounds.TryGetValue(clip, out (short[] Samples, TimeSpan Length) loaded)) {
        Played++;
        _scheduler.Play(clip, loaded.Length);
      }
    }

    // The user is typing, so a Correction and its sound may come soon.
    public void KeepAwake() {
      _scheduler.KeepAwake();
    }

    public void Dispose() {
      _output.Dispose();
    }

    private static byte[]? Read(string clip) {
      using Stream? wave = Open(clip);
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
