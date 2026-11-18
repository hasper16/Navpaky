using Navpaky.Core;

namespace Navpaky.Win32.Keyboard {
  /// <summary>
  /// Picks out the key presses the Hotkey recorder may take: a key that is not a modifier,
  /// pressed with at least one modifier.
  /// </summary>
  internal static class Chords {
    /// <summary>Returns the combination a key press makes, or <see langword="null" />.</summary>
    /// <param name="raw">The key event as the hook read it.</param>
    /// <param name="inputEvent">The event the translator made of it.</param>
    /// <returns>The modifiers and the virtual-key code, or <see langword="null" />.</returns>
    public static (KeyModifiers Modifiers, ushort VirtualKey)? Of(
        in RawInput raw, InputEvent? inputEvent) {
      if (raw.Kind != RawInputKind.KeyDown
          || inputEvent is not KeyDown { Kind: not KeyKind.Modifier } key
          || key.Modifiers == KeyModifiers.None) {
        return null;
      }

      return (key.Modifiers, raw.VirtualKey);
    }
  }
}
