using System.IO;
using Navpaky.Core;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class SetupRunnerTests : IDisposable {
    private static readonly Guid _attempt = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-runner").FullName;
    private readonly DataFolderPair _data;
    private readonly InstallChoices _choices;
    private readonly string _log;
    private readonly List<string> _extractedTo = [];
    private readonly string _self;

    public SetupRunnerTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
      _choices = new InstallChoices(
          _attempt, UiLanguage.English, LaunchAtSignIn: true, DesktopShortcut: false,
          KeptData.New, Path.Join(_root, "Programs", "Navpaky"));
      _log = Path.Join(_root, "setup.log");
      _self = Path.Join(_root, "Downloads", "NavpakySetup.exe");
    }

    // A stopped cmd.exe child can hold seen-args.txt for a moment after the kill (a CI flake).
    public void Dispose() {
      for (int attempt = 1; attempt < 10; attempt++) {
        try {
          Directory.Delete(_root, recursive: true);
          return;
        } catch (IOException) {
          Thread.Sleep(200);
        }
      }

      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task ASetupThatAppliedTheChoicesSucceeds() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));
      List<SetupPhase> phases = [];

      SetupOutcome outcome = await runner.RunAsync(
          _choices, forAllUsers: false, _data, _log, new Collect(phases));

      Assert.Equal(SetupOutcome.Succeeded, outcome);
      Assert.Equal([SetupPhase.Copying, SetupPhase.Applying, SetupPhase.Finishing], phases);
    }

    [Fact]
    public async Task TheChoicesAreWrittenBeforeSetupStarts() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await ForMe(runner);

      string seen = await File.ReadAllTextAsync(
          Path.Join(_root, "seen-choices.txt"), TestContext.Current.CancellationToken);
      Assert.Equal(_choices, InstallChoices.Parse(seen));
    }

    [Fact]
    public async Task SetupRunsSilentlyIntoTheFolderWithoutExtraArguments() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await ForMe(runner);

      string[] args = await File.ReadAllLinesAsync(
          Path.Join(_root, "seen-args.txt"), TestContext.Current.CancellationToken);
      Assert.Equal(["--silent", "--log", _log, "--installto", _choices.InstallFolder], args);
    }

    [Fact]
    public async Task ExitZeroWithoutTheAcknowledgementFails() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: false));

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
    }

    [Fact]
    public async Task AnErrorExitFails() {
      SetupRunner runner = Runner(Stub(exitCode: 3, acknowledge: true));

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
    }

    [Fact]
    public async Task ASetupThatRunsTooLongIsStoppedAndReportedAsFailed() {
      var runner = new SetupRunner(
          Extract(Stub(exitCode: 0, acknowledge: true, sleepSeconds: 60)),
          new ProcessSetupLauncher()) {
        Timeout = TimeSpan.FromSeconds(1),
      };

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
      Assert.False(File.Exists(Path.Join(_root, "finished.txt")));
    }

    [Fact]
    public async Task ASetupThatSurvivesTheStopMayStillBeRunning() {
      var stubborn = new StubbornLauncher();
      SetupRunner runner = new SetupRunner(Extract(Stub(0, acknowledge: true)), stubborn) {
        Timeout = TimeSpan.FromMilliseconds(200),
        KillWait = TimeSpan.FromMilliseconds(200),
      };

      SetupOutcome outcome = await ForMe(runner);

      Assert.Equal(SetupOutcome.MayStillRun, outcome);
      Assert.True(stubborn.Killed);
    }

    // Stopping can fail (access denied, part of the tree gone); the wait afterwards decides.
    [Fact]
    public async Task AStopThatThrowsStillEndsAsMayStillRun() {
      var stubborn = new StubbornLauncher { KillFails = true };
      var runner = new SetupRunner(Extract(Stub(0, acknowledge: true)), stubborn) {
        Timeout = TimeSpan.FromMilliseconds(200),
        KillWait = TimeSpan.FromMilliseconds(200),
      };

      SetupOutcome outcome = await ForMe(runner);

      Assert.Equal(SetupOutcome.MayStillRun, outcome);
    }

    // Task 9.1, design D3: Velopack stops a hook that overruns its 30 s, for example while an
    // antivirus scans the new app on its first start, and still exits 0.
    [Fact]
    public async Task AStoppedHookIsRunAgainAndTheInstallSucceeds() {
      var hook = new HookLauncher(Acknowledgement());
      SetupRunner runner = new(Extract(Stub(exitCode: 0, acknowledge: false)), hook);
      Installed();

      SetupOutcome outcome = await ForMe(runner);

      Assert.Equal(SetupOutcome.Succeeded, outcome);
      Assert.Equal(
          [(AppExe(), "--veloapp-install|0.1.0")],
          hook.Runs.Select(run => (run.Exe, string.Join('|', run.Args))));
    }

    [Fact]
    public async Task AHookThatFailsAgainFails() {
      var hook = new HookLauncher(null);
      SetupRunner runner = new(Extract(Stub(exitCode: 0, acknowledge: false)), hook);
      Installed();

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
      Assert.Single(hook.Runs);
    }

    [Fact]
    public async Task WithoutTheInstalledAppTheHookIsNotRunAgain() {
      var hook = new HookLauncher(Acknowledgement());
      SetupRunner runner = new(Extract(Stub(exitCode: 0, acknowledge: false)), hook);

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
      Assert.Empty(hook.Runs);
    }

    [Fact]
    public async Task WithoutTheVersionInTheLogTheHookIsNotRunAgain() {
      var hook = new HookLauncher(Acknowledgement());
      SetupRunner runner = new(
          Extract(Stub(exitCode: 0, acknowledge: false, version: null)), hook);
      Installed();

      Assert.Equal(
          SetupOutcome.Failed, await ForMe(runner));
      Assert.Empty(hook.Runs);
    }

    [Fact]
    public async Task AnAcknowledgedInstallRunsNoHookAgain() {
      var hook = new HookLauncher(Acknowledgement());
      SetupRunner runner = new(Extract(Stub(exitCode: 0, acknowledge: true)), hook);
      Installed();

      Assert.Equal(
          SetupOutcome.Succeeded, await ForMe(runner));
      Assert.Empty(hook.Runs);
    }

    // A poll after the log is complete must not report the phases a second time (a CI flake).
    [Fact]
    public async Task EachPhaseIsReportedOnceInOrder() {
      CancellationToken cancel = TestContext.Current.CancellationToken;
      string[] log = [
          "[INFO] Extracting Update.exe...",
          "[INFO] Starting process install hook",
          "[INFO] Installation completed successfully!"];
      await File.WriteAllLinesAsync(_log, log, cancel);
      Directory.CreateDirectory(_data.Local);
      await File.WriteAllTextAsync(Acknowledgement(), string.Empty, cancel);
      SetupRunner runner = new(Extract(Stub(0, acknowledge: true)), new LateExit());
      var phases = new Recorded();

      await runner.RunAsync(_choices, forAllUsers: false, _data, _log, phases);

      Assert.Equal([SetupPhase.Copying, SetupPhase.Applying, SetupPhase.Finishing], phases.All);
    }

    // Task 5.1 of add-machine-wide-install: only the attempt and a flag cross the elevation.
    [Fact]
    public async Task ForAllUsersTheWizardStartsItselfElevated() {
      var elevated = new ElevatedFake(0, Acknowledgement());
      SetupRunner runner = new(Extract(Stub(0, acknowledge: true)), new ProcessSetupLauncher()) {
        Elevated = elevated,
        Self = _self,
      };

      SetupOutcome outcome = await runner.RunAsync(
          _choices, forAllUsers: true, _data, _log, progress: null);

      Assert.Equal(SetupOutcome.Succeeded, outcome);
      Assert.Equal(
          [(_self, "--all-users-setup|6f9619ff8b86d011b42d00c04fc964ff|on")],
          elevated.Runs.Select(run => (run.Exe, string.Join('|', run.Args))));
      Assert.Empty(_extractedTo);
    }

    [Fact]
    public async Task ForAllUsersAFailedElevatedSetupFails() {
      SetupRunner runner = new(Extract(Stub(0, acknowledge: true)), new ProcessSetupLauncher()) {
        Elevated = new ElevatedFake(1, Acknowledgement()),
        Self = _self,
      };

      Assert.Equal(
          SetupOutcome.Failed,
          await runner.RunAsync(_choices, forAllUsers: true, _data, _log, progress: null));
    }

    // An administrator's own account ran the elevated hook without this user's choices.
    [Fact]
    public async Task ForAllUsersTheHookRunsAgainAsThisUserWithTheInstalledVersion() {
      var hook = new HookLauncher(Acknowledgement());
      SetupRunner runner = new(Extract(Stub(0, acknowledge: true)), hook) {
        Elevated = new ElevatedFake(0, acknowledgement: null),
        Self = _self,
      };
      Installed("0.3.0");

      SetupOutcome outcome = await runner.RunAsync(
          _choices, forAllUsers: true, _data, _log, progress: null);

      Assert.Equal(SetupOutcome.Succeeded, outcome);
      Assert.Equal(
          [(AppExe(), "--veloapp-install|0.3.0")],
          hook.Runs.Select(run => (run.Exe, string.Join('|', run.Args))));
    }

    [Fact]
    public async Task TheTemporaryCopyOfSetupIsDeleted() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await ForMe(runner);

      Assert.Single(_extractedTo);
      Assert.False(Directory.Exists(_extractedTo[0]));
    }

    private Task<SetupOutcome> ForMe(SetupRunner runner) {
      return runner.RunAsync(_choices, forAllUsers: false, _data, _log, progress: null);
    }

    private SetupRunner Runner(string stub) {
      return new SetupRunner(Extract(stub), new ProcessSetupLauncher());
    }

    // Puts the stub where the embedded setup would go.
    private Func<string, string> Extract(string stub) {
      return folder => {
        _extractedTo.Add(folder);
        string exe = Path.Join(folder, "setup.cmd");
        File.WriteAllText(exe, stub);
        return exe;
      };
    }

    // A batch file standing in for Velopack's setup: it records what it saw, writes the log
    // lines the runner follows, and acknowledges like the install hook when asked to.
    private string Stub(
        int exitCode, bool acknowledge, int sleepSeconds = 0, string? version = "0.1.0") {
      string choices = Path.Join(_data.Local, InstallChoices.FileName);
      string ack = Path.Join(_data.Local, _choices.AcknowledgementName);
      return string.Join(
          "\r\n",
          "@echo off",
          ":next",
          "if \"%~1\"==\"\" goto done",
          $"echo %~1>> \"{Path.Join(_root, "seen-args.txt")}\"",
          "shift",
          "goto next",
          ":done",
          $"copy /y \"{choices}\" \"{Path.Join(_root, "seen-choices.txt")}\" >nul",
          version is null ? "rem" : $"echo [INFO]     Package Version: {version}>> \"{_log}\"",
          $"echo [INFO] Extracting Update.exe...>> \"{_log}\"",
          sleepSeconds > 0 ? $"ping -n {sleepSeconds + 1} 127.0.0.1 >nul" : "rem",
          $"echo [INFO] Starting process install hook>> \"{_log}\"",
          acknowledge ? $"type nul > \"{ack}\"" : "rem",
          $"echo [INFO] Installation completed successfully!>> \"{_log}\"",
          $"type nul > \"{Path.Join(_root, "finished.txt")}\"",
          $"exit /b {exitCode}");
    }

    private string Acknowledgement() {
      return Path.Join(_data.Local, _choices.AcknowledgementName);
    }

    private string AppExe() {
      return Path.Join(_choices.InstallFolder, "current", "Navpaky.App.exe");
    }

    // What Velopack's setup leaves behind even when it stopped the hook; the version file is
    // the package manifest Velopack keeps beside the app.
    private void Installed(string? version = null) {
      Directory.CreateDirectory(Path.GetDirectoryName(AppExe())!);
      File.WriteAllText(AppExe(), string.Empty);
      if (version is not null) {
        string manifest = $"<package><metadata><version>{version}</version></metadata></package>";
        File.WriteAllText(Path.Join(Path.GetDirectoryName(AppExe())!, "sq.version"), manifest);
      }
    }

    private sealed class Collect(List<SetupPhase> phases) : IProgress<SetupPhase> {
      public void Report(SetupPhase value) {
        if (phases.Count == 0 || phases[^1] != value) {
          phases.Add(value);
        }
      }
    }

    // Runs the setup stub for real and stands in for the installed app when the runner starts
    // its install hook again, acknowledging like the hook when asked to.
    private sealed class HookLauncher(string? acknowledgement) : ISetupLauncher {
      private readonly ProcessSetupLauncher _setup = new();

      public List<(string Exe, IReadOnlyList<string> Args)> Runs { get; } = [];

      public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
        if (!exe.EndsWith("Navpaky.App.exe", StringComparison.Ordinal)) {
          return _setup.Start(exe, args);
        }

        Runs.Add((exe, args));
        if (acknowledgement is not null) {
          File.WriteAllText(acknowledgement, string.Empty);
        }

        return new Exited();
      }

      private sealed class Exited : ISetupProcess {
        public int ExitCode => 0;

        public Task<bool> WaitForExitAsync(TimeSpan timeout) {
          return Task.FromResult(true);
        }

        public void KillTree() {
        }

        public void Dispose() {
        }
      }
    }

    // Every phase as reported, without dropping repeats.
    private sealed class Recorded : IProgress<SetupPhase> {
      public List<SetupPhase> All { get; } = [];

      public void Report(SetupPhase value) {
        All.Add(value);
      }
    }

    // A setup that exits only after two more polls of its finished log.
    private sealed class LateExit : ISetupLauncher {
      public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
        return new Process();
      }

      private sealed class Process : ISetupProcess {
        private int _waits;

        public int ExitCode => 0;

        public Task<bool> WaitForExitAsync(TimeSpan timeout) {
          return Task.FromResult(++_waits > 2);
        }

        public void KillTree() {
        }

        public void Dispose() {
        }
      }
    }

    // The elevated wizard: records how it was started and acknowledges like the hook when asked.
    private sealed class ElevatedFake(int exitCode, string? acknowledgement) : ISetupLauncher {
      public List<(string Exe, IReadOnlyList<string> Args)> Runs { get; } = [];

      public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
        Runs.Add((exe, args));
        if (acknowledgement is not null) {
          File.WriteAllText(acknowledgement, string.Empty);
        }

        return new Done(exitCode);
      }

      private sealed class Done(int exitCode) : ISetupProcess {
        public int ExitCode => exitCode;

        public Task<bool> WaitForExitAsync(TimeSpan timeout) {
          return Task.FromResult(true);
        }

        public void KillTree() {
        }

        public void Dispose() {
        }
      }
    }

    // A setup process that never exits, even when stopped.
    private sealed class StubbornLauncher : ISetupLauncher {
      public bool Killed { get; private set; }

      public bool KillFails { get; init; }

      public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
        return new Stubborn(this);
      }

      private sealed class Stubborn(StubbornLauncher owner) : ISetupProcess {
        public int ExitCode => -1;

        public async Task<bool> WaitForExitAsync(TimeSpan timeout) {
          await Task.Delay(timeout);
          return false;
        }

        public void KillTree() {
          owner.Killed = true;
          if (owner.KillFails) {
            throw new AggregateException(new System.ComponentModel.Win32Exception(5));
          }
        }

        public void Dispose() {
        }
      }
    }
  }
}
