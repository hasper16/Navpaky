using System.Globalization;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Rekey.Win32.Hotkeys {
  /// <summary>
  /// Names keys the way Windows does, such as "Num +" for the numeric keypad's plus.
  /// </summary>
  public static class KeyNames {
    // Keys whose scan code needs the extended flag to get the right name.
    private static readonly HashSet<ushort> _extended = [
        0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x2D, 0x2E, 0x6F, 0x90];

    /// <summary>
    /// Returns the name of a key, or its code as <c>0xNN</c> when Windows has none.
    /// </summary>
    /// <param name="virtualKey">The virtual-key code.</param>
    /// <returns>The key's name.</returns>
    public static string Of(ushort virtualKey) {
      uint scanCode = PInvoke.MapVirtualKey(virtualKey, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC);
      int lParam = (int)(scanCode << 16) | (_extended.Contains(virtualKey) ? 1 << 24 : 0);
      Span<char> name = stackalloc char[64];
      int length = PInvoke.GetKeyNameText(lParam, name);

      return scanCode != 0 && length > 0
          ? name[..length].ToString()
          : "0x" + virtualKey.ToString("X2", CultureInfo.InvariantCulture);
    }
  }
}
