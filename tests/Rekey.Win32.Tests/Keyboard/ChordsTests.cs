using Rekey.Core;
using Rekey.Core.Detection;
using Rekey.Win32.Keyboard;
using Xunit;

namespace Rekey.Win32.Tests.Keyboard {
  // Re-check of fix 2.3 (add-final-acceptance): the Hotkey recorder takes combinations from the
  // keyboard hook, which sees those another app registered as global hotkeys.
  public sealed class ChordsTests {
    private const ushort _space = 0x20;

    [Fact]
    public void KeyWithModifiersIsAChord() {
      var raw = new RawInput(RawInputKind.KeyDown, _space, 0x39, false, 0);

      Assert.Equal(
          (KeyModifiers.Control | KeyModifiers.Alt, _space),
          Chords.Of(raw, Down(KeyKind.Space, KeyModifiers.Control | KeyModifiers.Alt)));
    }

    [Fact]
    public void KeyWithoutModifiersIsNoChord() {
      var raw = new RawInput(RawInputKind.KeyDown, _space, 0x39, false, 0);

      Assert.Null(Chords.Of(raw, Down(KeyKind.Space, KeyModifiers.None)));
    }

    [Fact]
    public void ModifierAloneIsNoChord() {
      var raw = new RawInput(RawInputKind.KeyDown, 0xA2, 0x1D, false, 0);

      Assert.Null(Chords.Of(raw, Down(KeyKind.Modifier, KeyModifiers.Control)));
    }

    private static KeyDown Down(KeyKind kind, KeyModifiers modifiers) {
      return new KeyDown(TimeSpan.Zero, new Keystroke(0x39, false, false), kind, modifiers);
    }
  }
}
