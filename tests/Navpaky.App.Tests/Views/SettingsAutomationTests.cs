using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Threading;
using Navpaky.App.Localization;
using Navpaky.App.Telemetry;
using Navpaky.App.ViewModels;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Navpaky.Win32.Hotkeys;
using Wpf.Ui.Controls;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Re-check of fixes 2.5 and 2.10 (add-final-acceptance): what a screen reader gets from each
  // Settings page in Ukrainian. Every control a user can reach has a name, and no name holds an
  // icon glyph (a private-use character). The tree is written to the test output.
  [Collection(nameof(Sta))]
  public sealed class SettingsAutomationTests {
    private static readonly ControlType[] _named = [
        ControlType.Button, ControlType.CheckBox, ControlType.ComboBox, ControlType.Edit,
        ControlType.Hyperlink, ControlType.ListItem, ControlType.RadioButton, ControlType.Slider];

    private static bool _windowsBound;

    private readonly ITestOutputHelper _output;

    public SettingsAutomationTests(ITestOutputHelper output) {
      _output = output;
    }

    [Fact]
    public void EveryControlHasANameWithoutIconGlyphs() {
      var problems = new List<string>();
      Sta.Run(() => {
        CultureInfo before = Localizer.Instance.Culture;
        try {
          // The pages take styles and templates from Resources.xaml. A plain Application holds
          // them: the real App would start Navpaky once the dispatcher runs.
          if (Application.Current is null) {
            var host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            host.Resources.MergedDictionaries.Add(new ResourceDictionary {
              Source = new Uri("pack://application:,,,/Navpaky.App;component/Resources.xaml"),
            });
          }

          IconText.HideFromScreenReaders();
          if (!_windowsBound) {
            _windowsBound = true;
            Localizer.BindEveryWindow();
          }

          Localizer.Instance.Use(CultureInfo.GetCultureInfo("uk"));
          Strings.Culture = CultureInfo.GetCultureInfo("uk");
          using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
          using var settings = new SettingsViewModel(hub);
          using var hotkey = new HotkeyViewModel(hub, _ => HotkeyConflict.None);
          using var apps = new ExcludedAppsViewModel(hub, key => key);
          var words = new WordsViewModel(_ => { }, () => TimeSpan.Zero);
          var signUp = new SignUpViewModel(_ => Task.FromResult(true));
          Func<SettingsSection>[] sections = [
              () => new(
                  "PageGeneral",
                  SymbolRegular.Settings24,
                  new GeneralPage(settings, _ => { }, dark: false)),
              () => new("PageCorrection", SymbolRegular.Book24, new CorrectionPage(settings)),
              () => new("PageHotkey", SymbolRegular.Keyboard24, new HotkeyPage(hotkey)),
              () => new(
                  "PageAppsAndWords", SymbolRegular.AppsList24, new AppsAndWordsPage(apps, words)),
              () => new(
                  "PageDiagnostics",
                  SymbolRegular.ShieldCheckmark24,
                  new DiagnosticsPage(settings, signUp, () => [], () => { }, () => { })),
              () => new("PageAbout", SymbolRegular.Info24, new AboutPage()),
          ];

          // Screen readers see only what is on screen, so each page is shown first in a window
          // of its own, off screen and without taking focus.
          for (int first = 0; first < sections.Length; first++) {
            SettingsSection[] order = [.. sections.Skip(first).Concat(sections.Take(first))
                .Select(make => make())];
            var window = new SettingsWindow(order) {
              Left = -32000,
              Top = -32000,
              ShowActivated = false,
              ShowInTaskbar = false,
            };
            window.Show();
            try {
              Render();
              string page = order[0].TitleKey;
              if (window.Language.IetfLanguageTag != "uk") {
                problems.Add($"{page}: window language {window.Language.IetfLanguageTag}");
              }

              _output.WriteLine($"== {page}");
              AutomationPeer root = UIElementAutomationPeer.CreatePeerForElement(window);
              int seen = Walk(root, 0, page, problems);
              if (seen == 0) {
                problems.Add($"{page}: no control reached the automation tree");
              }
            } finally {
              window.Close();
            }
          }
        } finally {
          Localizer.Instance.Use(before);
          Strings.Culture = before;
        }
      });

      Assert.Empty(problems);
    }

    // Lets the window lay out and load the page it switched to.
    private static void Render() {
      var frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(
          DispatcherPriority.ContextIdle, () => frame.Continue = false);
      Dispatcher.PushFrame(frame);
    }

    private static bool HasIconGlyph(string text) {
      return text.Any(c => c is >= '' and <= '');
    }

    // Screen readers skip what is off screen; a scroll bar's own buttons are reached through its
    // scroll pattern, not by name.
    private int Walk(AutomationPeer peer, int depth, string page, List<string> problems) {
      int seen = 0;
      string name = peer.GetName();
      AutomationControlType type = peer.GetAutomationControlType();
      if (peer.IsOffscreen() || type == AutomationControlType.ScrollBar) {
        return seen;
      }

      if (peer.IsControlElement()) {
        seen++;
        _output.WriteLine($"{new string(' ', depth * 2)}{type} \"{name}\"");
        if (_named.Any(c => c.ProgrammaticName.EndsWith($".{type}", StringComparison.Ordinal))
            && string.IsNullOrWhiteSpace(name)) {
          problems.Add($"{page}: {type} without a name");
        }

        if (HasIconGlyph(name)) {
          problems.Add($"{page}: {type} named with an icon glyph");
        }
      }

      foreach (AutomationPeer child in peer.GetChildren() ?? []) {
        seen += Walk(child, depth + 1, page, problems);
      }

      return seen;
    }
  }
}
