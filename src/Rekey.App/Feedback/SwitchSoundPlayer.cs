using System.IO;
using Rekey.Core;
using Rekey.Win32;

namespace Rekey.App.Feedback {
  // Plays the switch sounds Rekey ships (design D6), loaded once so a Correction never waits
  // for the disk. The sounds are Rekey's own (ADR 0003).
  internal sealed class SwitchSoundPlayer {
    // Monitor audio drops the start of a sound after a pause. The chime, 350 ms long, was
    // partly heard there, so less than 350 ms goes missing; 300 ms of lead-in covers that while
    // keeping the delay after a Correction below a third of a second.
    private static readonly TimeSpan _leadIn = TimeSpan.FromMilliseconds(300);

    private readonly Dictionary<SwitchSound, WaveSound> _players = [];

    public SwitchSoundPlayer() {
      foreach (SwitchSound sound in Enum.GetValues<SwitchSound>()) {
        using Stream? wave = Open(sound);
        if (wave is null) {
          continue;
        }

        using var bytes = new MemoryStream();
        wave.CopyTo(bytes);
        _players[sound] = new WaveSound(WaveLeadIn.Add(bytes.ToArray(), _leadIn));
      }
    }

    public static Stream? Open(SwitchSound sound) {
      string name = sound switch {
        SwitchSound.KeyClick => "key-click",
        SwitchSound.SoftTick => "soft-tick",
        _ => "chime",
      };
      return typeof(SwitchSoundPlayer).Assembly
          .GetManifestResourceStream($"Rekey.Sounds.{name}.wav");
    }

    // For the desktop checks: how many sounds played since start.
    public int Played { get; private set; }

    public void Play(SwitchSound sound) {
      if (_players.TryGetValue(sound, out WaveSound? player)) {
        Played++;
        player.Play();
      }
    }
  }
}
