using Rekey.App.ViewModels;
using Rekey.Core;
using Rekey.Core.Tests;
using Rekey.Win32.Hotkeys;
using Xunit;

namespace Rekey.App.Tests.ViewModels {
  public sealed class HotkeyViewModelTests : IDisposable {
    private const ushort _space = 0x20;
    private const ushort _shiftKey = 0x10;

    private readonly SettingsHub _hub = new(new Settings(), _ => { }, new ManualClock());
    private readonly Dictionary<Hotkey, HotkeyConflict> _conflicts = [];
    private readonly HotkeyViewModel _model;

    public HotkeyViewModelTests() {
      _model = Create();
    }

    public void Dispose() {
      _hub.Dispose();
    }

    [Fact]
    public void AFreePresetBecomesTheHotkey() {
      _model.Choose(Hotkey.DoubleCtrl);

      Assert.Equal(Hotkey.DoubleCtrl, _hub.Current.Hotkey);
      Assert.Null(_model.ConflictText);
    }

    [Fact]
    public void AFreeRecordedCombinationBecomesTheHotkey() {
      Hotkey? recorded = HotkeyViewModel.Recorded(
          KeyModifiers.Control | KeyModifiers.Alt, _space);

      _model.Choose(recorded!.Value);

      Assert.Equal(
          Hotkey.Custom(KeyModifiers.Control | KeyModifiers.Alt, _space), _hub.Current.Hotkey);
    }

    [Fact]
    public void AConflictingCombinationWaitsForConfirmation() {
      Hotkey winSpace = Hotkey.Custom(KeyModifiers.Windows, _space);
      _conflicts[winSpace] = HotkeyConflict.WindowsLayoutSwitch;

      _model.Choose(winSpace);

      Assert.Equal(Hotkey.DoubleShift, _hub.Current.Hotkey);
      Assert.Equal(
          "Win+Space switches the keyboard layout in Windows. As the Hotkey, Rekey will take it "
              + "over. Use it anyway or pick another.",
          _model.ConflictText);
    }

    [Fact]
    public void ConfirmingAppliesTheConflictingCombination() {
      Hotkey winSpace = Hotkey.Custom(KeyModifiers.Windows, _space);
      _conflicts[winSpace] = HotkeyConflict.WindowsLayoutSwitch;
      _model.Choose(winSpace);

      _model.ConfirmPending();

      Assert.Equal(winSpace, _hub.Current.Hotkey);
      Assert.Null(_model.ConflictText);
    }

    [Fact]
    public void PickingAnotherKeepsThePreviousHotkey() {
      Hotkey taken = Hotkey.Custom(KeyModifiers.Control | KeyModifiers.Alt, 0x4B);
      _conflicts[taken] = HotkeyConflict.TakenByAnotherApp;
      _model.Choose(taken);
      Assert.Equal(
          "Ctrl+Alt+K is already used by another app. Use it anyway or pick another.",
          _model.ConflictText);

      _model.CancelPending();

      Assert.Equal(Hotkey.DoubleShift, _hub.Current.Hotkey);
      Assert.Null(_model.ConflictText);
    }

    [Theory]
    [InlineData(KeyModifiers.None, _space)]
    [InlineData(KeyModifiers.Control, _shiftKey)]
    public void ARecordingWithoutAModifierAndAKeyIsIncomplete(KeyModifiers modifiers, ushort key) {
      Assert.Null(HotkeyViewModel.Recorded(modifiers, key));
    }

    [Theory]
    [InlineData(HotkeyKind.DoubleShift, "Double Shift")]
    [InlineData(HotkeyKind.CtrlSpace, "Ctrl+Space")]
    [InlineData(HotkeyKind.WinSpace, "Win+Space")]
    public void PresetsHaveNames(HotkeyKind kind, string name) {
      Hotkey preset = HotkeyViewModel.Presets.Single(hotkey => hotkey.Kind == kind);

      Assert.Equal(name, HotkeyViewModel.Describe(preset));
    }

    [Fact]
    public void CustomCombinationNamesItsKeys() {
      Hotkey hotkey = Hotkey.Custom(KeyModifiers.Shift | KeyModifiers.Control, 0x70);

      Assert.Equal("Ctrl+Shift+F1", HotkeyViewModel.Describe(hotkey));
    }

    // Fix 2.3 of add-final-acceptance: a key without a letter shows its name, not its code.
    [Fact]
    public void NumpadKeyIsNamedNotCoded() {
      Hotkey hotkey = Hotkey.Custom(KeyModifiers.Shift | KeyModifiers.Control, 0x6B);

      string text = HotkeyViewModel.Describe(hotkey);

      Assert.StartsWith("Ctrl+Shift+", text, StringComparison.Ordinal);
      Assert.DoesNotContain("0x", text, StringComparison.Ordinal);
      Assert.NotEqual("Ctrl+Shift+", text);
    }

    [Fact]
    public void ADisposedModelNoLongerFollowsTheHub() {
      using HotkeyViewModel model = Create();
      var announced = new List<string?>();
      model.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

      model.Dispose();
      _hub.Update(settings => settings with { AutoCorrectionEnabled = false });

      Assert.Empty(announced);
    }

    private HotkeyViewModel Create() {
      return new HotkeyViewModel(_hub, hotkey => _conflicts.GetValueOrDefault(hotkey));
    }
  }
}
