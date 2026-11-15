using System.IO;
using System.Media;
using Rekey.Core;

namespace Rekey.App.Feedback {
  // Plays the switch sounds Rekey ships (design D6), loaded once so a Correction never waits
  // for the disk. The sounds are Rekey's own (ADR 0003).
  internal sealed class SwitchSoundPlayer : IDisposable {
    private readonly Dictionary<SwitchSound, SoundPlayer> _players = [];

    public SwitchSoundPlayer() {
      foreach (SwitchSound sound in Enum.GetValues<SwitchSound>()) {
        if (Open(sound) is not { } wave) {
          continue;
        }

        var player = new SoundPlayer(wave);
        player.Load();
        _players[sound] = player;
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
      if (_players.TryGetValue(sound, out SoundPlayer? player)) {
        Played++;
        player.Play();
      }
    }

    public void Dispose() {
      foreach (SoundPlayer player in _players.Values) {
        player.Stream?.Dispose();
        player.Dispose();
      }
    }
  }
}
