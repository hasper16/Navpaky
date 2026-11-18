using System.Windows;
using System.Windows.Automation.Peers;
using Wpf.Ui.Controls;

namespace Rekey.App.Views {
  // An icon next to text that already names it: screen readers skip it, as its glyph is a
  // private-use character they would read as noise.
  internal sealed class DecorativeIcon : SymbolIcon {
    protected override AutomationPeer OnCreateAutomationPeer() {
      return new HiddenPeer(this);
    }

    private sealed class HiddenPeer(FrameworkElement owner)
        : FrameworkElementAutomationPeer(owner) {
      protected override bool IsControlElementCore() {
        return false;
      }

      protected override bool IsContentElementCore() {
        return false;
      }

      protected override List<AutomationPeer>? GetChildrenCore() {
        return null;
      }
    }
  }
}
