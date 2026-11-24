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

    public SetupRunnerTests() {
      _data = new DataFolderPair(
          Path.Join(_root, "Roaming", "Navpaky"), Path.Join(_root, "Local", "Navpaky"));
      _choices = new InstallChoices(
          _attempt, UiLanguage.English, LaunchAtSignIn: true, DesktopShortcut: false,
          KeptData.New, Path.Join(_root, "Programs", "Navpaky"));
      _log = Path.Join(_root, "setup.log");
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task ASetupThatAppliedTheChoicesSucceeds() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));
      List<SetupPhase> phases = [];

      SetupOutcome outcome = await runner.RunAsync(_choices, _data, _log, new Collect(phases));

      Assert.Equal(SetupOutcome.Succeeded, outcome);
      Assert.Equal([SetupPhase.Copying, SetupPhase.Applying, SetupPhase.Finishing], phases);
    }

    [Fact]
    public async Task TheChoicesAreWrittenBeforeSetupStarts() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await runner.RunAsync(_choices, _data, _log, progress: null);

      string seen = await File.ReadAllTextAsync(
          Path.Join(_root, "seen-choices.txt"), TestContext.Current.CancellationToken);
      Assert.Equal(_choices, InstallChoices.Parse(seen));
    }

    [Fact]
    public async Task SetupRunsSilentlyIntoTheFolderWithoutExtraArguments() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await runner.RunAsync(_choices, _data, _log, progress: null);

      string[] args = await File.ReadAllLinesAsync(
          Path.Join(_root, "seen-args.txt"), TestContext.Current.CancellationToken);
      Assert.Equal(["--silent", "--log", _log, "--installto", _choices.InstallFolder], args);
    }

    [Fact]
    public async Task ExitZeroWithoutTheAcknowledgementFails() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: false));

      Assert.Equal(
          SetupOutcome.Failed, await runner.RunAsync(_choices, _data, _log, progress: null));
    }

    [Fact]
    public async Task AnErrorExitFails() {
      SetupRunner runner = Runner(Stub(exitCode: 3, acknowledge: true));

      Assert.Equal(
          SetupOutcome.Failed, await runner.RunAsync(_choices, _data, _log, progress: null));
    }

    [Fact]
    public async Task ASetupThatRunsTooLongIsStoppedAndReportedAsFailed() {
      var runner = new SetupRunner(
          Extract(Stub(exitCode: 0, acknowledge: true, sleepSeconds: 60)),
          new ProcessSetupLauncher()) {
        Timeout = TimeSpan.FromSeconds(1),
      };

      Assert.Equal(
          SetupOutcome.Failed, await runner.RunAsync(_choices, _data, _log, progress: null));
      Assert.False(File.Exists(Path.Join(_root, "finished.txt")));
    }

    [Fact]
    public async Task ASetupThatSurvivesTheStopMayStillBeRunning() {
      var stubborn = new StubbornLauncher();
      SetupRunner runner = new SetupRunner(Extract(Stub(0, acknowledge: true)), stubborn) {
        Timeout = TimeSpan.FromMilliseconds(200),
        KillWait = TimeSpan.FromMilliseconds(200),
      };

      SetupOutcome outcome = await runner.RunAsync(_choices, _data, _log, progress: null);

      Assert.Equal(SetupOutcome.MayStillRun, outcome);
      Assert.True(stubborn.Killed);
    }

    [Fact]
    public async Task TheTemporaryCopyOfSetupIsDeleted() {
      SetupRunner runner = Runner(Stub(exitCode: 0, acknowledge: true));

      await runner.RunAsync(_choices, _data, _log, progress: null);

      Assert.Single(_extractedTo);
      Assert.False(Directory.Exists(_extractedTo[0]));
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
    private string Stub(int exitCode, bool acknowledge, int sleepSeconds = 0) {
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
          $"echo [INFO] Extracting Update.exe...>> \"{_log}\"",
          sleepSeconds > 0 ? $"ping -n {sleepSeconds + 1} 127.0.0.1 >nul" : "rem",
          $"echo [INFO] Starting process install hook>> \"{_log}\"",
          acknowledge ? $"type nul > \"{ack}\"" : "rem",
          $"echo [INFO] Installation completed successfully!>> \"{_log}\"",
          $"type nul > \"{Path.Join(_root, "finished.txt")}\"",
          $"exit /b {exitCode}");
    }

    private sealed class Collect(List<SetupPhase> phases) : IProgress<SetupPhase> {
      public void Report(SetupPhase value) {
        if (phases.Count == 0 || phases[^1] != value) {
          phases.Add(value);
        }
      }
    }

    // A setup process that never exits, even when stopped.
    private sealed class StubbornLauncher : ISetupLauncher {
      public bool Killed { get; private set; }

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
        }

        public void Dispose() {
        }
      }
    }
  }
}
