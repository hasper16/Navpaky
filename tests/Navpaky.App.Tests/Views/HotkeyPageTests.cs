using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Navpaky.App.ViewModels;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Navpaky.Win32.Hotkeys;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Fix 2.3 of add-final-acceptance: with a custom Hotkey already chosen, its option is already
  // checked, so a click on it must still start recording.
  [Collection(nameof(Sta))]
  public sealed class HotkeyPageTests {
    // The page takes its card and pill styles from Resources.xaml.
    public HotkeyPageTests() {
      Sta.Run(Sta.EnsureApplication);
    }

    // Owner decision (fix-redesign-findings): the options are keycaps, as in prototype v24, and
    // keep their names in words for screen readers, the desktop checks and the user guide.
    [Fact]
    public void OptionsAreKeycapsNamedInWords() {
      List<(object Content, string Name)> options = [];
      Sta.Run(() => {
        using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
        using var model = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
        var page = new HotkeyPage(model);
        options.AddRange(((Panel)page.FindName("Options")).Children.OfType<RadioButton>()
            .Select(option => (option.Content, AutomationProperties.GetName(option))));
      });

      Assert.Equal(HotkeyViewModel.Presets.Count + 1, options.Count);
      Assert.All(options.Take(HotkeyViewModel.Presets.Count), option => {
        Assert.IsType<StackPanel>(option.Content);
        Assert.NotEmpty(option.Name);
      });
      Assert.Equal(Strings.HotkeyDoubleShift, options[0].Name);
    }

    [Fact]
    public void ClickOnTheCheckedCustomOptionStartsRecording() {
      Sta.Run(() => {
        var settings = new Settings {
          Hotkey = Hotkey.Custom(KeyModifiers.Control | KeyModifiers.Shift, 0x6B),
        };
        using var hub = new SettingsHub(settings, _ => { }, new ManualClock());
        using var model = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
        var page = new HotkeyPage(model);
        var options = (Panel)page.FindName("Options");
        RadioButton custom = options.Children.OfType<RadioButton>().Last();
        Assert.True(custom.IsChecked);

        custom.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        Assert.Equal(Strings.HotkeyRecording, ((TextBox)page.FindName("Recorder")).Text);
      });
    }

    // Re-check of 2.3: another app had registered Ctrl+Alt+Space as a global hotkey, so Windows
    // never sent it to the recorder; Navpaky's keyboard hook still sees it and offers it.
    [Fact]
    public void ChordSeenByTheKeyboardHookIsRecorded() {
      Sta.Run(() => {
        var listening = new List<bool>();
        var chords = new ChordRecorder(listening.Add);
        using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
        using var model = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
        var page = new HotkeyPage(model, chords);
        ((Button)page.FindName("Record")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        chords.Offer(KeyModifiers.Control | KeyModifiers.Alt, 0x20);

        Assert.Equal(
            Hotkey.Custom(KeyModifiers.Control | KeyModifiers.Alt, 0x20), hub.Current.Hotkey);
        Assert.Equal([true, false], listening);
      });
    }

    // Closing the window mid-recording must release the hook: a later chord in another app
    // would otherwise reach the closed window's disposed model and change the settings.
    [Fact]
    public void ClosingTheWindowEndsRecording() {
      Sta.Run(() => {
        var listening = new List<bool>();
        var chords = new ChordRecorder(listening.Add);
        using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
        using var model = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
        var page = new HotkeyPage(model, chords);
        var window = new Window {
          Content = page,
          Left = -32000,
          Top = -32000,
          ShowActivated = false,
          ShowInTaskbar = false,
        };
        window.Show();
        ((Button)page.FindName("Record")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Hotkey before = hub.Current.Hotkey;

        window.Close();
        Flush();
        chords.Offer(KeyModifiers.Control | KeyModifiers.Alt, 0x20);

        Assert.Equal(before, hub.Current.Hotkey);
        Assert.Equal([true, false], listening);
      });
    }

    // Unloaded arrives through the dispatcher after Close.
    private static void Flush() {
      var frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(
          DispatcherPriority.Background, () => frame.Continue = false);
      Dispatcher.PushFrame(frame);
    }
  }
}
