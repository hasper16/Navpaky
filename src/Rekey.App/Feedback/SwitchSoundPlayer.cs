using System.IO;
using Rekey.Core;
using Rekey.Win32;

namespace Rekey.App.Feedback {
  // Plays the switch sounds Rekey ships (design D6), loaded once so a Correction never waits
  // for the disk. The sounds are Rekey's own (ADR 0003).
  internal sealed class SwitchSoundPlayer {
    private readonly Dictionary<SwitchSound, WaveSound> _players = [];

    public SwitchSoundPlayer() {
      foreach (SwitchSound sound in Enum.GetValues<SwitchSound>()) {
        using Stream? wave = Open(sound);
        if (wave is null) {
          continue;
        }

        using var bytes = new MemoryStream();
        wave.CopyTo(bytes);
        _players[sound] = new WaveSound(bytes.ToArray());
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
