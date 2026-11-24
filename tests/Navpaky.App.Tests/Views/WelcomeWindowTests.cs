using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Navpaky.App.Onboarding;
using Navpaky.App.ViewModels;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Navpaky.Ui;
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
    public void LastStepChoosesTheHotkeyAndOneTelemetrySwitch() {
      Hotkey chosen = Hotkey.DoubleShift;
      List<string> switches = [];
      string? telemetry = null;
      Sta.Run(() => {
        telemetry = Strings.ResourceManager.GetString(
            "WelcomeTelemetry", Navpaky.Ui.Localizer.Instance.Culture);
        Sta.EnsureApplication();
        Open((window, hub) => {
          Option(window, Hotkey.DoubleCtrl).IsChecked = true;
          chosen = hub.Current.Hotkey;
          switches.AddRange(Switches(window));
        });
      });

      Assert.Equal(Hotkey.DoubleCtrl, chosen);
      Assert.Equal([telemetry!], switches);
    }

    // A screen reader selects a radio button without a click.
    [Fact]
    public void ScreenReaderSelectionChoosesTheHotkey() {
      Hotkey chosen = Hotkey.DoubleShift;
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open((window, hub) => {
          ISelectionItemProvider selection =
              new RadioButtonAutomationPeer(Option(window, Hotkey.WinSpace));
          selection.Select();
          chosen = hub.Current.Hotkey;
        });
      });

      Assert.Equal(Hotkey.WinSpace, chosen);
    }

    // Owner finding on 0.4.9: the welcome window offers every option Settings offers.
    [Fact]
    public void LastStepOffersEveryHotkeyOfSettings() {
      List<string> names = [];
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open((window, _) => names.AddRange(Options(window).Select(
            option => System.Windows.Automation.AutomationProperties.GetName(option))));
      });

      Assert.Equal(HotkeyViewModel.Presets.Count + 1, names.Count);
      Assert.Equal(
          HotkeyViewModel.Presets.Select(HotkeyViewModel.Describe),
          names.Take(HotkeyViewModel.Presets.Count));
    }

    [Fact]
    public void WithoutTheWizardTheInstallQuestionsAreAsked() {
      bool languageShown = false;
      List<string> switches = [];
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open(
            (window, _) => {
              languageShown = VisibleInTree((DependencyObject)window.FindName("LanguageChoice"));
              switches.AddRange(Switches(window));
            },
            new Settings(InstallQuestionsAnswered: false));
      });

      Assert.True(languageShown);
      Assert.Equal(
          [Text("WelcomeTelemetry"), Text("LaunchAtSignIn"), Text("WelcomeShortcut")],
          switches);
    }

    [Fact]
    public void AfterTheWizardTheLanguageIsNotAskedAgain() {
      Visibility language = Visibility.Visible;
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open(
            (window, _) => language =
                ((FrameworkElement)window.FindName("LanguageChoice")).Visibility,
            new Settings(InstallQuestionsAnswered: true));
      });

      Assert.Equal(Visibility.Collapsed, language);
    }

    private static IEnumerable<RadioButton> Options(WelcomeWindow window) {
      return Find<RadioButton>((DependencyObject)window.FindName("Step2"));
    }

    private static RadioButton Option(WelcomeWindow window, Hotkey hotkey) {
      string name = HotkeyViewModel.Describe(hotkey);
      return Options(window).Single(
          option => System.Windows.Automation.AutomationProperties.GetName(option) == name);
    }

    private static string Privacy(string language) {
      return Strings.ResourceManager.GetString(
          "WelcomePrivacyField", System.Globalization.CultureInfo.GetCultureInfo(language))!;
    }

    private static string Text(string key) {
      return Strings.ResourceManager.GetString(key, Localizer.Instance.Culture)!;
    }

    private static IEnumerable<string> Switches(WelcomeWindow window) {
      return Find<ToggleButton>((DependencyObject)window.FindName("Step2"))
          .Where(toggle => toggle is not RadioButton && VisibleInTree(toggle))
          .Select(toggle => (string)toggle.GetValue(
              System.Windows.Automation.AutomationProperties.NameProperty));
    }

    // Whether an element shows when its step does: the window is never shown in tests, and
    // only one step panel is visible at a time.
    private static bool VisibleInTree(DependencyObject node) {
      for (DependencyObject? at = node;
          at is not null and not FrameworkElement { Name: "Step0" or "Step1" or "Step2" };
          at = LogicalTreeHelper.GetParent(at)) {
        if (at is UIElement { Visibility: not Visibility.Visible }) {
          return false;
        }
      }

      return true;
    }

    private static void Open(Action<WelcomeWindow, SettingsHub> check) {
      Open(check, new Settings(InstallQuestionsAnswered: true));
    }

    private static void Open(Action<WelcomeWindow, SettingsHub> check, Settings settings) {
      using var hub = new SettingsHub(settings, _ => { }, new ManualClock());
      using var model = new WelcomeViewModel(hub, []);
      using var hotkey = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
      var window = new WelcomeWindow(model, hotkey, chords: null, () => { });
      Sta.Flush();
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
