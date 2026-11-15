using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rekey.App.Views {
  // WPF UI's caption buttons carry only glyphs; screen readers need their names.
  internal static class TitleBarButtons {
    public static void Name(DependencyObject titleBar) {
      string[] names = [Strings.WindowMinimize, Strings.WindowMaximize, Strings.WindowClose];
      List<Button> buttons = [.. Descendants(titleBar).OfType<Button>()];
      for (int i = 0; i < buttons.Count && i < names.Length; i++) {
        AutomationProperties.SetName(buttons[buttons.Count - names.Length + i], names[i]);
      }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent) {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        yield return child;
        foreach (DependencyObject grandchild in Descendants(child)) {
          yield return grandchild;
        }
      }
    }
  }
}
