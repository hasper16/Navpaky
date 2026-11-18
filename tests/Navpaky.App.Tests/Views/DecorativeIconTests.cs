using System.Windows.Automation.Peers;
using Navpaky.App.Views;
using Wpf.Ui.Controls;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Fix 2.5 of add-final-acceptance: Narrator read the section icons' glyphs as "////".
  [Collection(nameof(Sta))]
  public sealed class DecorativeIconTests {
    [Fact]
    public void ScreenReadersSeeNothingOfTheIcon() {
      Sta.Run(() => {
        var icon = new DecorativeIcon { Symbol = SymbolRegular.Settings24 };
        icon.Measure(new System.Windows.Size(100, 100));

        AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(icon);

        Assert.False(peer.IsControlElement());
        Assert.False(peer.IsContentElement());
        Assert.Empty(peer.GetChildren() ?? []);
      });
    }
  }
}
