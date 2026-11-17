using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Rekey.App.ViewModels;
using Rekey.App.Views;
using Rekey.Core;
using Rekey.Core.Tests;
using Rekey.Win32.Hotkeys;
using Xunit;

namespace Rekey.App.Tests.Views {
  // Fix 2.3 of add-final-acceptance: with a custom Hotkey already chosen, its option is already
  // checked, so a click on it must still start recording.
  [Collection(nameof(Sta))]
  public sealed class HotkeyPageTests {
    [Fact]
    public void ClickOnTheCheckedCustomOptionStartsRecording() {
      Sta.Run(() => {
        var settings = new Settings {
          Hotkey = Hotkey.Custom(KeyModifiers.Control | KeyModifiers.Shift, 0x6B),
        };
        using var hub = new SettingsHub(settings, _ => { }, new ManualClock());
        using var model = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
        var page = new HotkeyPage(model);
        var options = (StackPanel)page.FindName("Options");
        RadioButton custom = options.Children.OfType<RadioButton>().Last();
        Assert.True(custom.IsChecked);

        custom.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        Assert.Equal(Strings.HotkeyRecording, ((TextBox)page.FindName("Recorder")).Text);
      });
    }
  }
}
