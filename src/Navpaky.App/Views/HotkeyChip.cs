using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Navpaky.App.ViewModels;
using Navpaky.Core;

namespace Navpaky.App.Views {
  // A Hotkey drawn as the prototype's keycaps ("Shift ×2", "Ctrl + Space"). Screen readers, the
  // desktop checks and the user guide name it in words, as HotkeyViewModel.Describe does.
  internal static class HotkeyChip {
    public static void Show(ContentControl option, Hotkey hotkey) {
      option.Content = Keys(option, hotkey);
      AutomationProperties.SetName(option, HotkeyViewModel.Describe(hotkey));
    }

    private static StackPanel Keys(FrameworkElement owner, Hotkey hotkey) {
      var line = new StackPanel { Orientation = Orientation.Horizontal };
      if (Tapped(hotkey.Kind) is { } key) {
        line.Children.Add(Keycap(owner, key));
        line.Children.Add(Text(Strings.HotkeyTwice, right: 0));
        return line;
      }

      string[] keys = HotkeyViewModel.Describe(hotkey).Split('+');
      for (int i = 0; i < keys.Length; i++) {
        if (i > 0) {
          line.Children.Add(Text("+", right: 5));
        }

        line.Children.Add(Keycap(owner, keys[i]));
      }

      return line;
    }

    private static string? Tapped(HotkeyKind kind) {
      return kind switch {
        HotkeyKind.DoubleShift => Strings.KeyShift,
        HotkeyKind.DoubleCtrl => Strings.KeyCtrl,
        HotkeyKind.DoubleAlt => Strings.KeyAlt,
        _ => null,
      };
    }

    private static Border Keycap(FrameworkElement owner, string key) {
      return new Border {
        Style = (Style)owner.FindResource("Keycap"),
        Child = new TextBlock { Text = key, FontSize = 12, FontWeight = FontWeights.SemiBold },
      };
    }

    private static TextBlock Text(string text, double right) {
      return new TextBlock {
        Text = text,
        Margin = new Thickness(5, 0, right, 0),
        VerticalAlignment = VerticalAlignment.Center,
      };
    }
  }
}
