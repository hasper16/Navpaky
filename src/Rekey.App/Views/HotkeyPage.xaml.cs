using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rekey.App.Localization;
using Rekey.App.ViewModels;
using Rekey.Core;

namespace Rekey.App.Views {
  // Hotkey: the presets, a recorder for a custom combination, and the conflict warning.
  internal sealed partial class HotkeyPage {
    private readonly HotkeyViewModel _model;
    private readonly Dictionary<RadioButton, Hotkey> _presets = [];
    private readonly RadioButton _custom;
    private readonly ChordRecorder? _chords;
    private bool _recording;

    // Set while the page checks the radio button of the current Hotkey itself.
    private bool _showing;

    public HotkeyPage(HotkeyViewModel model, ChordRecorder? chords = null) {
      InitializeComponent();
      _model = model;
      _chords = chords;
      foreach (Hotkey preset in HotkeyViewModel.Presets) {
        var option = new RadioButton { GroupName = "Hotkey" };
        option.Checked += (_, _) => Choose(preset);
        _presets[option] = preset;
        Options.Children.Add(option);
      }

      _custom = new RadioButton { GroupName = "Hotkey" };
      _custom.SetBinding(ContentProperty, LocExtension.Binding("HotkeyCustom"));
      _custom.Checked += (_, _) => {
        if (!_showing) {
          StartRecording();
        }
      };

      // A custom Hotkey keeps the option checked, so a click on it raises no Checked.
      _custom.Click += (_, _) => StartRecording();
      Options.Children.Add(_custom);
      Record.Click += (_, _) => StartRecording();
      Recorder.PreviewKeyDown += OnRecorderKey;

      // Another app's global hotkey may activate that app; the hook still brings its keys, so
      // only a focus move inside Rekey ends recording.
      Recorder.LostKeyboardFocus += (_, e) => {
        if (e.NewFocus is not null) {
          StopRecording();
        }
      };
      UseAnyway.Click += (_, _) => _model.ConfirmPending();
      PickAnother.Click += (_, _) => _model.CancelPending();
      _model.PropertyChanged += OnModelChanged;
      ShowCurrent();
      Localizer.Follow(this, ShowTexts);
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
      if (!_recording && _chords is not null) {
        _chords.Pressed += OnChord;
        _chords.Start();
      }

      _recording = true;
      Recorder.Text = Strings.HotkeyRecording;
      Recorder.Focus();
    }

    private void StopRecording() {
      if (_recording) {
        EndRecording();
        ShowCurrent();
      }
    }

    private void EndRecording() {
      _recording = false;
      if (_chords is not null) {
        _chords.Pressed -= OnChord;
        _chords.Stop();
      }
    }

    // The same combination may come from the window and from the hook; the first one counts.
    private void TakeChord(KeyModifiers modifiers, ushort virtualKey) {
      if (_recording && HotkeyViewModel.Recorded(modifiers, virtualKey) is { } hotkey) {
        EndRecording();
        _model.Choose(hotkey);
        ShowCurrent();
      }
    }

    private void OnChord(KeyModifiers modifiers, ushort virtualKey) {
      TakeChord(modifiers, virtualKey);
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
      TakeChord(HeldModifiers(), (ushort)KeyInterop.VirtualKeyFromKey(key));
    }

    private void OnModelChanged(object? sender, PropertyChangedEventArgs e) {
      ShowCurrent();
    }

    // Hotkey names are built from UI strings, so they follow the interface language.
    private void ShowTexts() {
      foreach ((RadioButton option, Hotkey preset) in _presets) {
        option.Content = HotkeyViewModel.Describe(preset);
      }

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
