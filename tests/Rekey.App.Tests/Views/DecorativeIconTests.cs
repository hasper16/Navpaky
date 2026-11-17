using System.Runtime.ExceptionServices;
using System.Windows.Automation.Peers;
using Rekey.App.Views;
using Wpf.Ui.Controls;
using Xunit;

namespace Rekey.App.Tests.Views {
  // Fix 2.5 of add-final-acceptance: Narrator read the section icons' glyphs as "////".
  public sealed class DecorativeIconTests {
    [Fact]
    public void ScreenReadersSeeNothingOfTheIcon() {
      RunOnSta(() => {
        var icon = new DecorativeIcon { Symbol = SymbolRegular.Settings24 };
        icon.Measure(new System.Windows.Size(100, 100));

        AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(icon);

        Assert.False(peer.IsControlElement());
        Assert.False(peer.IsContentElement());
        Assert.Empty(peer.GetChildren() ?? []);
      });
    }

    private static void RunOnSta(Action test) {
      ExceptionDispatchInfo? failure = null;
      var thread = new Thread(() => {
        try {
          test();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          failure = ExceptionDispatchInfo.Capture(e);
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
      failure?.Throw();
    }
  }
}
