using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Automation;
using Rekey.Core.Languages;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rekey.DesktopChecks {
  // The checks of change add-tray-and-settings-ui against the Debug app started with
  // --check-ui: the tray's Language, the settings window applying a change at once and opening
  // once, the cursor indicator, and the switch sound. The checks' own worker only watches; the
  // app corrects. Only the spec's example Word is typed; reports carry no text.
  internal sealed partial class Checks {
#if DEBUG
    private const string _settingsTitle = "Rekey settings";
    private const string _indicatorTitle = "Rekey indicator";
    private static readonly TimeSpan _uiWait = TimeSpan.FromSeconds(5);

    private Process? _app;

    public int RunTrayAndSettings() {
      Begin(minutes: 2);
      LayoutId[] installed = [.. InstalledLayouts()];
      if (!installed.Contains(SpecWords.EnglishUs) || !installed.Contains(SpecWords.Russian)) {
        Expect(
            "prerequisites",
            false,
            "install the English (US) and Russian Layouts, then run again");
        return End();
      }

      if (!StartUiApp()) {
        return End();
      }

      Step("2.2 the tray shows the Language", CheckTrayLanguage);
      Step("4.1 the cursor indicator", CheckIndicator);
      Step("3.2 a setting applies at once", CheckSettingApplies);
      Step("3.1 Settings opens one window", CheckOneSettingsWindow);
      Step("4.2 the switch sound", CheckSwitchSound);
      Command("quit");
      bool quit = _app!.WaitForExit(TimeSpan.FromSeconds(10));
      Expect("2.2 Quit ends the app", quit, quit ? "the process exited" : "still running");
      return End();
    }

    private static string UiStatePath() {
      return Path.Join(SystemState.DataFolder, "check-ui.txt");
    }

    // One field of the app's check file, such as "language" or "corrections".
    private static string? UiState(string field) {
      string? state = ReadState(UiStatePath());
      return state?.Split(' ')
          .Select(pair => pair.Split('=', 2))
          .FirstOrDefault(pair => pair.Length == 2 && pair[0] == field)?[1];
    }

    private static int UiCount(string field) {
      return int.TryParse(UiState(field), out int count) ? count : -1;
    }

    // A command the app takes in place of a click on its tray menu.
    private static void Command(string command) {
      // Written aside and moved in, so the app never reads half a command.
      string path = Path.Join(SystemState.DataFolder, "check-ui-command.txt");
      string draft = path + ".tmp";
      File.WriteAllText(draft, command);
      File.Move(draft, path, overwrite: true);
    }

    private static AutomationElement? SettingsWindow() {
      return Desktop.FindWindow(_settingsTitle, _uiWait) is { } window
          ? AutomationElement.FromHandle(window)
          : null;
    }

    private static AutomationElement? Named(
        AutomationElement root, string name, ControlType type) {
      return root.FindFirst(
          TreeScope.Descendants,
          new AndCondition(
              new PropertyCondition(AutomationElement.NameProperty, name),
              new PropertyCondition(AutomationElement.ControlTypeProperty, type)));
    }

    // Selects a section of the settings window and sets one of its switches, as a person
    // would with the mouse, without moving the mouse.
    private static bool SetSwitch(string section, string label, bool on) {
      if (SettingsWindow() is not { } window
          || Named(window, section, ControlType.ListItem) is not { } item) {
        return false;
      }

      ((SelectionItemPattern)item.GetCurrentPattern(SelectionItemPattern.Pattern)).Select();
      Thread.Sleep(TimeSpan.FromMilliseconds(500));
      if (Named(window, label, ControlType.Button) is not { } control) {
        return false;
      }

      var toggle = (TogglePattern)control.GetCurrentPattern(TogglePattern.Pattern);
      if ((toggle.Current.ToggleState == ToggleState.On) != on) {
        toggle.Toggle();
      }

      Thread.Sleep(TimeSpan.FromMilliseconds(300));
      return (toggle.Current.ToggleState == ToggleState.On) == on;
    }

    private static bool IndicatorVisible() {
      return Desktop.FindWindow(_indicatorTitle, TimeSpan.Zero) is not null;
    }

    private bool StartUiApp() {
      if (Process.GetProcessesByName(_appName).Length > 0) {
        Expect("prerequisites", false, "close the running Rekey first");
        return false;
      }

      // Defaults: Auto Correction on, the indicator on, the sound off; no learned words.
      _restorer.Defer("Rekey settings file", SystemState.KeepFile("settings.json"));
      _restorer.Defer("Rekey word lists", SystemState.KeepFile("words.json"));
      _restorer.Defer("Rekey UI check file", SystemState.KeepFile("check-ui.txt"));
      _restorer.Defer("Rekey UI command file", SystemState.KeepFile("check-ui-command.txt"));
      _restorer.Defer("test Rekey processes", StopTestApps);
      File.Delete(Path.Join(SystemState.DataFolder, "settings.json"));
      File.Delete(Path.Join(SystemState.DataFolder, "words.json"));
      File.Delete(UiStatePath());

      var start = new ProcessStartInfo(AppPath());
      start.ArgumentList.Add("--check-ui");
      _app = Process.Start(start)
          ?? throw new InvalidOperationException("The app did not start.");
      _restorer.Defer("test Rekey process handle", _app.Dispose);
      bool ready = EventLog.WaitFor(() => File.Exists(UiStatePath()), TimeSpan.FromSeconds(15));
      Expect("prerequisites", ready, ready ? "the app runs" : "the app wrote no check file");
      return ready;
    }

    private void CheckTrayLanguage() {
      Point box = First().Ordinary;
      bool russian = SwitchIn(box, SpecWords.Russian)
          && EventLog.WaitFor(() => UiState("language") == "RU", _uiWait);
      Expect(
          "2.2 the tray shows RU after switching to Russian",
          russian,
          $"tray {UiState("language")}");
      bool english = SwitchIn(box, SpecWords.EnglishUs)
          && EventLog.WaitFor(() => UiState("language") == "EN", _uiWait);
      Expect(
          "2.2 the tray shows EN after switching back", english, $"tray {UiState("language")}");
    }

    // Types the spec's Word and Space in the emptied box in English (US); returns whether the
    // app corrected it.
    private bool TypeWord(Point box) {
      SwitchIn(box, SpecWords.EnglishUs);
      Input.ClearBox();
      int before = UiCount("corrections");
      Input.Type("ghbdtn");
      Input.Space();
      return EventLog.WaitFor(() => UiCount("corrections") > before, TimeSpan.FromSeconds(2));
    }

    private void CheckIndicator() {
      Point box = First().Ordinary;
      bool corrected = TypeWord(box);
      bool shown = corrected && EventLog.WaitFor(IndicatorVisible, TimeSpan.FromSeconds(1));
      HWND front = PInvoke.GetForegroundWindow();
      bool keptFront = PInvoke.GetAncestor(
          PInvoke.WindowFromPoint(box), GET_ANCESTOR_FLAGS.GA_ROOT) == front;
      int length = FocusedText()?.Length ?? -1;
      Input.Type("q");
      Thread.Sleep(TimeSpan.FromMilliseconds(300));
      bool typingGoesOn = (FocusedText()?.Length ?? -1) == length + 1;
      string detail = $"corrected {corrected}, badge {shown}, window kept in front {keptFront}, "
          + $"next key typed {typingGoesOn}";
      Expect(
          "4.1 a badge appears after a Correction while typing goes on in the same window",
          corrected && shown && keptFront && typingGoesOn,
          detail);
      Thread.Sleep(TimeSpan.FromSeconds(1.5));
      bool gone = !IndicatorVisible();
      Expect("4.1 the badge disappears after about a second", gone, $"badge hidden {gone}");
    }

    private void CheckSettingApplies() {
      Point box = First().Ordinary;
      Command("open-settings");
      bool off = SetSwitch("Correction", "Correct wrong-layout words automatically", on: false);
      bool notCorrected = off && !TypeWord(box) && FocusedText() == "ghbdtn ";
      Expect(
          "3.2 Auto Correction turned off in the settings window stops the next Correction",
          notCorrected,
          $"switch set {off}, {Compare(FocusedText(), "ghbdtn ")}");
      bool on = SetSwitch("Correction", "Correct wrong-layout words automatically", on: true);
      bool corrected = on && TypeWord(box);
      Expect(
          "3.2 turned on again, the next Word is corrected",
          corrected,
          $"switch set {on}, corrected {corrected}");
    }

    private void CheckOneSettingsWindow() {
      // The settings window is open; the test window takes the front, then Settings again.
      Focus(First().Ordinary);
      int opened = UiCount("settings-opened");
      PInvoke.AllowSetForegroundWindow((uint)_app!.Id);
      Command("open-settings");
      HWND? settings = Desktop.FindWindow(_settingsTitle, _uiWait);
      bool front = EventLog.WaitFor(
          () => settings is { } window && PInvoke.GetForegroundWindow() == window, _uiWait);
      int windows = Desktop.CountWindows(_settingsTitle);
      bool once = UiCount("settings-opened") == opened;
      Expect(
          "3.1 Settings again brings the open window to the front instead of a second one",
          front && windows == 1 && once,
          $"in front {front}, windows {windows}, created again {!once}");
    }

    private void CheckSwitchSound() {
      Point box = First().Ordinary;
      bool on = SetSwitch("General", "Play a sound after a correction", on: true);
      int before = UiCount("sounds");
      bool corrected = TypeWord(box);
      bool once = EventLog.WaitFor(
          () => UiCount("sounds") == before + 1, TimeSpan.FromSeconds(1));
      Input.Control('z');
      Thread.Sleep(TimeSpan.FromSeconds(1));
      bool undone = FocusedText() == "ghbdtn ";
      bool silent = UiCount("sounds") == before + 1;
      string detail = $"switch set {on}, corrected {corrected}, "
          + $"sounds {UiCount("sounds") - before}, undone {undone}";
      Expect(
          "4.2 the sound plays once per Correction and not on Undo",
          on && corrected && once && undone && silent,
          detail);
      SetSwitch("General", "Play a sound after a correction", on: false);
    }
#else
    public int RunTrayAndSettings() {
      Expect("tray and settings", false, "run the checks from a Debug build");
      return End();
    }
#endif
  }
}
