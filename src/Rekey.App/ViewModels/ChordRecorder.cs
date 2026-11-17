using Rekey.Core;

namespace Rekey.App.ViewModels {
  // Brings the Hotkey page the keys Rekey's keyboard hook sees while it records. A combination
  // another app registered as a global hotkey never reaches the window, yet passes the hook
  // first (re-check of fix 2.3, add-final-acceptance). Used on the UI thread only.
  internal sealed class ChordRecorder {
    private readonly Action<bool> _listen;

    public ChordRecorder(Action<bool> listen) {
      _listen = listen;
    }

    // A key pressed with modifiers, as the hook saw it.
    public event Action<KeyModifiers, ushort>? Pressed;

    public void Start() {
      _listen(true);
    }

    public void Stop() {
      _listen(false);
    }

    public void Offer(KeyModifiers modifiers, ushort virtualKey) {
      Pressed?.Invoke(modifiers, virtualKey);
    }
  }
}
