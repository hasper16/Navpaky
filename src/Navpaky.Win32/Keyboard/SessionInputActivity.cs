using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Navpaky.Win32.Keyboard {
  /// <summary>
  /// Reads the session's last input from Windows and the hooks' last input from
  /// <see cref="InputHook" />.
  /// </summary>
  internal sealed class SessionInputActivity : IInputActivity {
    private readonly InputHook _hook;

    public SessionInputActivity(InputHook hook) {
      _hook = hook;
    }

    /// <inheritdoc />
    public long SystemLastInputTick {
      get {
        var info = new LASTINPUTINFO { cbSize = (uint)Marshal.SizeOf<LASTINPUTINFO>() };
        return PInvoke.GetLastInputInfo(ref info) ? InputClock.Widen(info.dwTime) : InputClock.Now;
      }
    }

    /// <inheritdoc />
    public long HookLastInputTick => _hook.LastInputTick;

    /// <inheritdoc />
    public long Reattachments => _hook.Reattachments;
  }
}
