using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
      Sta.Run(() => {
        if (Application.Current is null) {
          var host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
          host.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new Uri("pack://application:,,,/Navpaky.App;component/Resources.xaml"),
          });
        }
      });
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
  }
}
