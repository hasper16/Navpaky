using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace Navpaky.App.Views {
  // WPF UI draws its icons, also inside its check boxes and expanders, as a text block holding a
  // private-use character, which screen readers read out as noise such as "////" (re-check of
  // fix 2.5, add-final-acceptance). Each icon's text is marked off screen, which screen readers
  // skip, so the control around it is read by its own name. The icon builds its text after it
  // loads, and WPF raises Loaded only where an element asked for it, so the text is marked on
  // its first layout (SizeChanged), which WPF raises for every element.
  internal static class IconText {
    // An icon's text sits at most this many levels below the icon.
    private const int _iconDepth = 2;

    private static bool _registered;

    // Once per process, before any window opens.
    public static void HideFromScreenReaders() {
      if (_registered) {
        return;
      }

      _registered = true;
      EventManager.RegisterClassHandler(
          typeof(TextBlock),
          FrameworkElement.SizeChangedEvent,
          new SizeChangedEventHandler((text, _) => {
            if (InIcon((DependencyObject)text)) {
              AutomationProperties.SetIsOffscreenBehavior(
                  (TextBlock)text, IsOffscreenBehavior.Offscreen);
            }
          }));
    }

    private static bool InIcon(DependencyObject text) {
      DependencyObject? parent = VisualTreeHelper.GetParent(text);
      for (int level = 0; level < _iconDepth && parent is not null; level++) {
        if (parent is IconElement) {
          return true;
        }

        parent = VisualTreeHelper.GetParent(parent);
      }

      return false;
    }
  }
}
