using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Navpaky.Core;
using Navpaky.Win32.Hotkeys;

namespace Navpaky.App.ViewModels {
  // The Hotkey page: a preset or a recorded combination becomes the Hotkey at once, unless it
  // conflicts; then the previous Hotkey stays until the user confirms (spec manual-correction,
  // Conflict warning).
  internal sealed class HotkeyViewModel : ObservableObject, IDisposable {
    private const ushort _space = 0x20;
    private const ushort _f1 = 0x70;
    private const ushort _f24 = 0x87;

    private readonly SettingsHub _hub;
    private readonly Func<Hotkey, HotkeyConflict> _check;
    private Hotkey? _pending;
    private HotkeyConflict _pendingConflict;

    public HotkeyViewModel(SettingsHub hub, Func<Hotkey, HotkeyConflict> check) {
      _hub = hub;
      _check = check;
      _hub.Changed += OnHubChanged;
    }

    public static IReadOnlyList<Hotkey> Presets { get; } = [
        Hotkey.DoubleShift, Hotkey.DoubleCtrl, Hotkey.DoubleAlt, Hotkey.CtrlSpace,
        Hotkey.WinSpace];

    public Hotkey Current => _hub.Current.Hotkey;

    public string? ConflictText {
      get {
        if (_pending is not { } pending) {
          return null;
        }

        string format = _pendingConflict == HotkeyConflict.WindowsLayoutSwitch
            ? Strings.HotkeyConflictWindows
            : Strings.HotkeyConflictTaken;
        return UiText.Format(format, Describe(pending));
      }
    }

    // A key pressed while recording, with the modifiers held; null until it makes a whole
    // combination.
    public static Hotkey? Recorded(KeyModifiers modifiers, ushort virtualKey) {
      try {
        return Hotkey.Custom(modifiers, virtualKey);
      } catch (ArgumentException) {
        return null;
      }
    }

    public static string Describe(Hotkey hotkey) {
      switch (hotkey.Kind) {
        case HotkeyKind.DoubleShift:
          return Strings.HotkeyDoubleShift;
        case HotkeyKind.DoubleCtrl:
          return Strings.HotkeyDoubleCtrl;
        case HotkeyKind.DoubleAlt:
          return Strings.HotkeyDoubleAlt;
        default:
          break;
      }

      var parts = new List<string>();
      (KeyModifiers Modifier, string Name)[] names = [
          (KeyModifiers.Control, Strings.KeyCtrl), (KeyModifiers.Shift, Strings.KeyShift),
          (KeyModifiers.Alt, Strings.KeyAlt), (KeyModifiers.Windows, Strings.KeyWin)];
      foreach ((KeyModifiers modifier, string name) in names) {
        if ((hotkey.Modifiers & modifier) != 0) {
          parts.Add(name);
        }
      }

      parts.Add(KeyName(hotkey.VirtualKey));
      return string.Join('+', parts);
    }

    public void Dispose() {
      _hub.Changed -= OnHubChanged;
    }

    public void Choose(Hotkey hotkey) {
      HotkeyConflict conflict = _check(hotkey);
      if (conflict == HotkeyConflict.None) {
        Apply(hotkey);
        return;
      }

      _pending = hotkey;
      _pendingConflict = conflict;
      OnPropertyChanged(nameof(ConflictText));
    }

    public void ConfirmPending() {
      if (_pending is { } pending) {
        Apply(pending);
      }
    }

    public void CancelPending() {
      _pending = null;
      OnPropertyChanged(nameof(ConflictText));
      OnPropertyChanged(nameof(Current));
    }

    // Letters and digits name themselves; other keys go by the name Windows gives them.
    private static string KeyName(ushort virtualKey) {
      if (virtualKey == _space) {
        return Strings.KeySpace;
      }

      if (virtualKey is (>= 0x30 and <= 0x39) or (>= 0x41 and <= 0x5A)) {
        return ((char)virtualKey).ToString();
      }

      if (virtualKey is >= _f1 and <= _f24) {
        return "F" + (virtualKey - _f1 + 1).ToString(CultureInfo.InvariantCulture);
      }

      return KeyNames.Of(virtualKey);
    }

    private void OnHubChanged(Settings settings) {
      OnPropertyChanged(nameof(Current));
    }

    private void Apply(Hotkey hotkey) {
      _pending = null;
      _hub.Update(settings => settings with { Hotkey = hotkey });
      OnPropertyChanged(nameof(ConflictText));
    }
  }
}
