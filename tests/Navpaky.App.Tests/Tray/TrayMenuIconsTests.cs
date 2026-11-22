using System.Windows.Controls;
using H.NotifyIcon;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Xunit;

namespace Navpaky.App.Tests.Tray {
  // Owner request (fix-redesign-findings): every tray menu item carries an icon.
  [Collection(nameof(Sta))]
  public sealed class TrayMenuIconsTests {
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryItemHasAnIcon(bool paused) {
      List<string> missing = [];
      int items = 0;
      Sta.Run(() => {
        Sta.EnsureApplication();
        using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
        var model = new TrayViewModel(hub, new ManualClock(), () => { });
        if (paused) {
          model.Pause(PausePreset.FifteenMinutes);
        }

        var icon = new TaskbarIcon();
        var view = new TrayIconView(icon, model, new TrayCommands(
            () => { }, () => { }, () => { }, () => { }, () => { }, () => { }, () => { }));
        view.Refresh();
        foreach (MenuItem item in All(icon.ContextMenu.Items)) {
          items++;
          if (item.Icon is null) {
            missing.Add((string)item.Header);
          }
        }
      });

      Assert.True(items >= 8);
      Assert.Empty(missing);
    }

    private static IEnumerable<MenuItem> All(ItemCollection items) {
      foreach (MenuItem item in items.OfType<MenuItem>()) {
        yield return item;
        foreach (MenuItem child in All(item.Items)) {
          yield return child;
        }
      }
    }
  }
}
