using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Navpaky.App.Onboarding;
using Navpaky.App.ViewModels;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Navpaky.Win32.Hotkeys;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // The welcome window follows prototype v24 (owner decision in fix-redesign-findings).
  [Collection(nameof(Sta))]
  public sealed class WelcomeWindowTests {
    [Fact]
    public void FirstStepHasNoPrivacyLine() {
      List<string> texts = [];
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open((window, _) => Collect((DependencyObject)window.FindName("Step0"), texts));
      });

      string[] privacy = [Privacy("en"), Privacy("uk")];
      Assert.Empty(texts.Intersect(privacy));
      Assert.NotEmpty(texts);
    }

    [Fact]
    public void LastStepChoosesTheHotkeyAndCrashReportsOnly() {
      Hotkey chosen = Hotkey.DoubleShift;
      List<string> switches = [];
      string? crashReports = null;
      Sta.Run(() => {
        crashReports = Strings.ResourceManager.GetString(
            "CrashReportsEnabled", Navpaky.App.Localization.Localizer.Instance.Culture);
        Sta.EnsureApplication();
        Open((window, hub) => {
          var ctrl = (ToggleButton)window.FindName("HotkeyCtrl");
          ctrl.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
          chosen = hub.Current.Hotkey;
          switches.AddRange(Find<ToggleButton>((DependencyObject)window.FindName("Step2"))
              .Where(toggle => toggle is not RadioButton)
              .Select(toggle => (string)toggle.GetValue(
                  System.Windows.Automation.AutomationProperties.NameProperty)));
        });
      });

      Assert.Equal(Hotkey.DoubleCtrl, chosen);
      Assert.Equal([crashReports!], switches);
    }

    private static string Privacy(string language) {
      return Strings.ResourceManager.GetString(
          "WelcomePrivacyField", System.Globalization.CultureInfo.GetCultureInfo(language))!;
    }

    private static void Open(Action<WelcomeWindow, SettingsHub> check) {
      using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
      using var model = new WelcomeViewModel(hub, []);
      using var hotkey = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
      var window = new WelcomeWindow(model, hotkey, () => { });
      try {
        check(window, hub);
      } finally {
        window.Close();
      }
    }

    private static IEnumerable<T> Find<T>(DependencyObject node)
        where T : DependencyObject {
      if (node is T match) {
        yield return match;
      }

      foreach (object child in LogicalTreeHelper.GetChildren(node)) {
        if (child is DependencyObject element) {
          foreach (T found in Find<T>(element)) {
            yield return found;
          }
        }
      }
    }

    private static void Collect(DependencyObject node, List<string> texts) {
      texts.AddRange(Find<TextBlock>(node).Select(block => block.Text));
    }
  }
}
