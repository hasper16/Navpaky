using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rekey.App.ViewModels;
using Rekey.Core;

namespace Rekey.App.Views {
  // Hotkey: the presets, a recorder for a custom combination, and the conflict warning.
  internal sealed partial class HotkeyPage {
    private readonly HotkeyViewModel _model;
    private readonly Dictionary<RadioButton, Hotkey> _presets = [];
    private readonly RadioButton _custom;
    private bool _recording;

    // Set while the page checks the radio button of the current Hotkey itself.
    private bool _showing;

    public HotkeyPage(HotkeyViewModel model) {
      InitializeComponent();
      _model = model;
      foreach (Hotkey preset in HotkeyViewModel.Presets) {
        var option = new RadioButton {
          GroupName = "Hotkey",
          Content = HotkeyViewModel.Describe(preset),
        };
        option.Checked += (_, _) => Choose(preset);
        _presets[option] = preset;
        Options.Children.Add(option);
      }

      _custom = new RadioButton { GroupName = "Hotkey", Content = Strings.HotkeyCustom };
      _custom.Checked += (_, _) => {
        if (!_showing) {
          StartRecording();
        }
      };
      Options.Children.Add(_custom);
      Record.Click += (_, _) => StartRecording();
      Recorder.PreviewKeyDown += OnRecorderKey;
      Recorder.LostKeyboardFocus += (_, _) => StopRecording();
      UseAnyway.Click += (_, _) => _model.ConfirmPending();
      PickAnother.Click += (_, _) => _model.CancelPending();
      _model.PropertyChanged += OnModelChanged;
      ShowCurrent();
    }

    private static KeyModifiers HeldModifiers() {
      ModifierKeys held = Keyboard.Modifiers;
      KeyModifiers modifiers = KeyModifiers.None;
      if ((held & ModifierKeys.Control) != 0) {
        modifiers |= KeyModifiers.Control;
      }

      if ((held & ModifierKeys.Shift) != 0) {
        modifiers |= KeyModifiers.Shift;
      }

      if ((held & ModifierKeys.Alt) != 0) {
        modifiers |= KeyModifiers.Alt;
      }

      if ((held & ModifierKeys.Windows) != 0) {
        modifiers |= KeyModifiers.Windows;
      }

      return modifiers;
    }

    private void Choose(Hotkey hotkey) {
      if (hotkey != _model.Current) {
        _model.Choose(hotkey);
      }
    }

    private void StartRecording() {
      _recording = true;
      Recorder.Text = Strings.HotkeyRecording;
      Recorder.Focus();
    }

    private void StopRecording() {
      if (_recording) {
        _recording = false;
        ShowCurrent();
      }
    }

    private void OnRecorderKey(object sender, KeyEventArgs e) {
      if (!_recording) {
        return;
      }

      Key key = e.Key == Key.System ? e.SystemKey : e.Key;
      if (key is Key.Tab or Key.Escape) {
        // Tab and Escape keep their usual meaning, so the keyboard never gets stuck here.
        StopRecording();
        return;
      }

      e.Handled = true;
      var virtualKey = (ushort)KeyInterop.VirtualKeyFromKey(key);
      if (HotkeyViewModel.Recorded(HeldModifiers(), virtualKey) is { } hotkey) {
        _recording = false;
        _model.Choose(hotkey);
        ShowCurrent();
      }
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e) {
      ShowCurrent();
    }

    private void ShowCurrent() {
      Hotkey current = _model.Current;
      RadioButton selected = _presets.FirstOrDefault(pair => pair.Value == current).Key
          ?? _custom;
      _showing = true;
      selected.IsChecked = true;
      _showing = false;

      if (!_recording) {
        Recorder.Text = current.Kind == HotkeyKind.Custom
            ? HotkeyViewModel.Describe(current)
            : string.Empty;
      }

      Conflict.Visibility = _model.ConflictText is null ? Visibility.Collapsed : Visibility.Visible;
      ConflictText.Text = _model.ConflictText ?? string.Empty;
      if (_model.ConflictText is not null) {
        UseAnyway.Focus();
      }
    }
  }
}
