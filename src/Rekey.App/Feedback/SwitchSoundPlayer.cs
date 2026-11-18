using System.IO;
using System.Windows.Threading;
using Rekey.Core;
using Rekey.Win32;

namespace Rekey.App.Feedback {
  // Plays the switch sounds Rekey ships (design D6), loaded once so a Correction never waits
  // for the disk. The sounds are Rekey's own (ADR 0003). Silence keeps monitor audio awake
  // while the user types (SoundScheduler). Used on the UI thread only.
  internal sealed class SwitchSoundPlayer {
    private readonly Dictionary<SwitchSound, (WaveSound Sound, TimeSpan Length)> _players = [];
    private readonly WaveSound? _silence;
    private readonly SoundScheduler _scheduler;

    public SwitchSoundPlayer() {
      foreach (SwitchSound sound in Enum.GetValues<SwitchSound>()) {
        if (Read(sound) is { } wave) {
          _players[sound] = (new WaveSound(wave), WaveFile.Length(wave));
        }
      }

      if (Read(SwitchSound.Chime) is { } template) {
        _silence = new WaveSound(WaveFile.Silence(template, TimeSpan.FromSeconds(1)));
      }

      _scheduler = new SoundScheduler(
          () => _silence?.PlayLooping(),
          WaveSound.StopAll,
          (sound, _) => _players[sound].Sound.Play(),
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
          .GetManifestResourceStream($"Rekey.Sounds.{name}.wav");
    }

    public void Play(SwitchSound sound) {
      if (_players.TryGetValue(sound, out (WaveSound Sound, TimeSpan Length) player)) {
        Played++;
        _scheduler.Play(sound, player.Length);
      }
    }

    // The user is typing, so a Correction and its sound may come soon.
    public void KeepAwake() {
      _scheduler.KeepAwake();
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
