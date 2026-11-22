using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;
using Navpaky.Core;
using Navpaky.Core.Statistics;
using Navpaky.Core.Tests;
using Xunit;
using SymbolIcon = Wpf.Ui.Controls.SymbolIcon;
using SymbolRegular = Wpf.Ui.Controls.SymbolRegular;

namespace Navpaky.App.Tests.Tray {
  // Owner request (fix-redesign-findings): the panel's footer actions carry the tray menu's
  // icons and keep their names for screen readers.
  [Collection(nameof(Sta))]
  public sealed class QuickPanelIconsTests {
    [Theory]
    [InlineData("Settings", SymbolRegular.Settings24)]
    [InlineData("FirstSteps", SymbolRegular.Wand24)]
    [InlineData("Quit", SymbolRegular.Power24)]
    public void FooterActionHasItsIcon(string button, SymbolRegular expected) {
      SymbolRegular? shown = null;
      string name = string.Empty;
      Sta.Run(() => {
        Sta.EnsureApplication();
        using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
        var tray = new TrayViewModel(hub, new ManualClock(), () => { });
        var model = new QuickPanelViewModel(
            hub, tray, () => StatsDocument.Empty, () => new DateOnly(2026, 11, 22));
        var panel = new QuickPanel(model, new QuickPanelActions(
            () => { }, () => { }, () => { }, () => { }, () => { }, () => { }));
        var action = (Button)panel.FindName(button);
        shown = Icons(action).FirstOrDefault()?.Symbol;
        name = AutomationProperties.GetName(action);
        panel.Close();
      });

      Assert.Equal(expected, shown);
      Assert.NotEmpty(name);
    }

    private static IEnumerable<SymbolIcon> Icons(DependencyObject node) {
      if (node is SymbolIcon icon) {
        yield return icon;
      }

      foreach (object child in LogicalTreeHelper.GetChildren(node)) {
        if (child is DependencyObject element) {
          foreach (SymbolIcon found in Icons(element)) {
            yield return found;
          }
        }
      }
    }
  }
}
