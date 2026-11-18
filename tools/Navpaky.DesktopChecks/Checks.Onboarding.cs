using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Automation;
using Navpaky.Core.Languages;
using Navpaky.Win32;
using Windows.Win32;

namespace Navpaky.DesktopChecks {
  // The checks of change add-onboarding against the Debug app started with --check-ui and no
  // settings, as at a first start: the welcome window opens, the try-it field corrects the
  // spec's Word and confirms, closing completes the welcome with start at sign-in on, the tray
  // command reopens it, the first real Correction shows the Undo tip once, and a later start
  // opens no welcome. Reports carry no text.
  internal sealed partial class Checks {
#if DEBUG
    private const string _welcomeTitle = "Welcome to Navpaky";
    private const string _tryItName = "Try-it field";
    private const string _movedTitle = "Navpaky's files were moved or deleted";
    private const string _tryItWorked =
        "It works: Navpaky corrected the word and switched the layout.";

    public int RunOnboarding() {
      Begin(minutes: 2);
      LayoutId[] installed = [.. InstalledLayouts()];
      if (!installed.Contains(SpecWords.EnglishUs) || !installed.Contains(SpecWords.Russian)) {
        Expect(
            "prerequisites",
            false,
            "install the English (US) and Russian Layouts, then run again");
        return End();
      }

      if (!StartUiApp(firstStart: true)) {
        return End();
      }

      Step("1.1 the welcome window opens at the first start", CheckWelcomeOpens);
      Step("2.2 the try-it field corrects and confirms", CheckTryIt);
      Step("1.1 closing completes the welcome", CheckWelcomeCloses);
      Step("2.4 Welcome and setup reopens the window", CheckWelcomeReopens);
      Step("2.5 the first real Correction shows the tip once", CheckTip);
      Step("1.1 a later start opens no welcome", CheckLaterStart);
      Command("quit");
      _app!.WaitForExit(TimeSpan.FromSeconds(10));
      Step("3.2 renaming the running executable shows the notice", CheckMovedNotice);
      return End();
    }

    private static AutomationElement? WelcomeWindow(TimeSpan wait) {
      return Desktop.FindWindow(_welcomeTitle, wait) is { } window
          ? AutomationElement.FromHandle(window)
          : null;
    }

    private static bool WelcomeGone() {
      return EventLog.WaitFor(() => Desktop.CountWindows(_welcomeTitle) == 0, _uiWait);
    }

    private static bool PressDone(AutomationElement window) {
      if (Named(window, "Done", ControlType.Button) is not { } done) {
        return false;
      }

      ((InvokePattern)done.GetCurrentPattern(InvokePattern.Pattern)).Invoke();
      return WelcomeGone();
    }

    private static bool WelcomeCompletedSaved() {
      string? json = ReadState(Path.Join(SystemState.DataFolder, "settings.json"));
      if (json is null) {
        return false;
      }

      try {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.TryGetProperty("welcomeCompleted", out JsonElement value)
            && value.ValueKind == JsonValueKind.True;
      } catch (JsonException) {
        return false;
      }
    }

    private static void CopyFolder(string from, string to) {
      foreach (string folder in Directory.EnumerateDirectories(
          from, "*", SearchOption.AllDirectories)) {
        Directory.CreateDirectory(Path.Join(to, Path.GetRelativePath(from, folder)));
      }

      Directory.CreateDirectory(to);
      foreach (string file in Directory.EnumerateFiles(from, "*", SearchOption.AllDirectories)) {
        File.Copy(file, Path.Join(to, Path.GetRelativePath(from, file)));
      }
    }

    private void CheckWelcomeOpens() {
      bool open = WelcomeWindow(_uiWait) is not null;
      Expect("1.1 the welcome window opens at the first start", open, $"window found {open}");
    }

    private void CheckTryIt() {
      if (WelcomeWindow(_uiWait) is not { } window
          || Named(window, _tryItName, ControlType.Edit) is not { } field) {
        Expect("2.2 the try-it field corrects and confirms", false, "no try-it field");
        return;
      }

      System.Windows.Point clickable = field.GetClickablePoint();
      var at = new Point((int)clickable.X, (int)clickable.Y);
      bool english = SwitchIn(at, SpecWords.EnglishUs);
      Input.ClearBox();
      int before = UiCount("corrections");
      Input.Type("ghbdtn");
      Input.Space();
      bool corrected = EventLog.WaitFor(
          () => TextOf(field) == "привет ", TimeSpan.FromSeconds(3));
      bool confirmed = EventLog.WaitFor(
          () => Named(window, _tryItWorked, ControlType.Text) is { Current.IsOffscreen: false },
          TimeSpan.FromSeconds(2));
      bool counted = UiCount("corrections") == before + 1;
      bool noTip = UiCount("tips") == 0;
      string detail = $"English set {english}, {Compare(TextOf(field), "привет ")}, "
          + $"confirmed {confirmed}, one Correction {counted}, no tip {noTip}";
      Expect(
          "2.2 \"ghbdtn\" + Space in the try-it field shows \"привет \" and the confirmation",
          english && corrected && confirmed && counted && noTip,
          detail);
    }

    private void CheckWelcomeCloses() {
      bool closed = WelcomeWindow(_uiWait) is { } window && PressDone(window);
      bool saved = closed && EventLog.WaitFor(WelcomeCompletedSaved, TimeSpan.FromSeconds(5));
      bool signIn = new SignInLaunch().IsOn();
      Expect(
          "1.1 Done closes the welcome, saves it as completed and leaves start at sign-in on",
          closed && saved && signIn,
          $"closed {closed}, saved {saved}, start at sign-in {signIn}");
    }

    private void CheckWelcomeReopens() {
      PInvoke.AllowSetForegroundWindow((uint)_app!.Id);
      Command("open-welcome");
      AutomationElement? window = WelcomeWindow(_uiWait);
      bool fresh = window is not null
          && Named(window, _tryItWorked, ControlType.Text) is null;
      bool closed = window is not null && PressDone(window);
      Expect(
          "2.4 Welcome and setup opens the window again",
          window is not null && fresh && closed,
          $"opened {window is not null}, no old confirmation {fresh}, closed {closed}");
    }

    private void CheckTip() {
      Point box = First().Ordinary;
      bool first = TypeWord(box);
      bool shown = EventLog.WaitFor(() => UiCount("tips") == 1, TimeSpan.FromSeconds(2));
      bool second = TypeWord(box);
      Thread.Sleep(TimeSpan.FromSeconds(1));
      bool once = UiCount("tips") == 1;
      Expect(
          "2.5 the first real Correction shows the Undo tip and the next one does not",
          first && shown && second && once,
          $"corrected {first} and {second}, tips {UiCount("tips")}");
    }

    private void CheckLaterStart() {
      Command("quit");
      bool quit = _app!.WaitForExit(TimeSpan.FromSeconds(10));
      bool started = quit && LaunchUiApp();
      bool none = started && WelcomeWindow(TimeSpan.FromSeconds(3)) is null;
      Expect(
          "1.1 a later start opens no welcome window",
          none,
          $"quit {quit}, started {started}, no welcome {none}");
    }

    // A copy of the Debug build, so renaming its executable leaves the build alone. Renaming a
    // running program is allowed where deleting it is not.
    private void CheckMovedNotice() {
      string copy = Path.Join(Path.GetTempPath(), $"navpaky-onboarding-{Guid.NewGuid():N}");
      _restorer.Defer("copied Navpaky build", () => Directory.Delete(copy, recursive: true));
      CopyFolder(Path.GetDirectoryName(AppPath())!, copy);
      string exe = Path.Join(copy, Path.GetFileName(AppPath()));

      // The app starts its executable watch before it writes this file, so a new file means
      // the watch runs.
      File.Delete(UiStatePath());
      using Process app = Process.Start(exe, "--check-ui")
          ?? throw new InvalidOperationException("The copy did not start.");
      bool started = EventLog.WaitFor(() => File.Exists(UiStatePath()), TimeSpan.FromSeconds(15));
      Thread.Sleep(TimeSpan.FromSeconds(1));
      File.Move(exe, exe + ".old");
      bool notice = Desktop.FindWindow(_movedTitle, TimeSpan.FromSeconds(10)) is not null;
      Kill(app);
      app.WaitForExit(TimeSpan.FromSeconds(10));
      Expect(
          "3.2 renaming the running executable shows the notice within a minute",
          started && notice,
          $"copy started {started}, notice shown {notice}");
    }
#else
    public int RunOnboarding() {
      Expect("onboarding", false, "run the checks from a Debug build");
      return End();
    }
#endif
  }
}
