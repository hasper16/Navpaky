using System.ComponentModel;
using System.IO;
using Navpaky.Core;

namespace Navpaky.Setup.Install {
  // Runs Velopack's setup silently into the chosen folder and decides how it ended (design D3 of
  // add-install-wizard). Velopack 1.2.161 crashes on arguments after "--" and kills an overrunning
  // install hook yet exits 0, so the choices go through the hand-off file and success needs the
  // hook's acknowledgement too.
  internal sealed class SetupRunner {
    private static readonly TimeSpan _poll = TimeSpan.FromMilliseconds(250);

    private static readonly (string Line, SetupPhase Phase)[] _phases = [
        ("Extracting", SetupPhase.Copying),
        ("Starting process install hook", SetupPhase.Applying),
        ("Installation completed", SetupPhase.Finishing)];

    private readonly Func<string, string> _extractSetup;
    private readonly ISetupLauncher _launcher;

    // extractSetup puts the embedded setup into the given empty folder and returns its path.
    public SetupRunner(Func<string, string> extractSetup, ISetupLauncher launcher) {
      _extractSetup = extractSetup;
      _launcher = launcher;
    }

    // The design's limit for the whole setup, and how long a stopped one may take to go.
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan KillWait { get; init; } = TimeSpan.FromSeconds(30);

    public async Task<SetupOutcome> RunAsync(
        InstallChoices choices,
        DataFolderPair data,
        string logPath,
        IProgress<SetupPhase>? progress) {
      choices.WriteTo(data.Local);
      DirectoryInfo temp = Directory.CreateTempSubdirectory("navpaky-setup-");
      try {
        string exe = _extractSetup(temp.FullName);
        using ISetupProcess setup = _launcher.Start(
            exe, ["--silent", "--log", logPath, "--installto", choices.InstallFolder]);
        bool exited = await WaitWhileReporting(setup, logPath, progress).ConfigureAwait(false);
        if (!exited) {
          TryStop(setup);
          return await setup.WaitForExitAsync(KillWait).ConfigureAwait(false)
              ? SetupOutcome.Failed
              : SetupOutcome.MayStillRun;
        }

        Report(logPath, progress);
        bool acknowledged = File.Exists(Path.Join(data.Local, choices.AcknowledgementName));
        return setup.ExitCode == 0 && acknowledged ? SetupOutcome.Succeeded : SetupOutcome.Failed;
      } finally {
        TryDelete(temp);
      }
    }

    private static void Report(string logPath, IProgress<SetupPhase>? progress) {
      if (progress is null || ReadShared(logPath) is not { } log) {
        return;
      }

      foreach ((string line, SetupPhase phase) in _phases) {
        if (log.Contains(line, StringComparison.Ordinal)) {
          progress.Report(phase);
        }
      }
    }

    // Velopack keeps the log open while it writes.
    private static string? ReadShared(string path) {
      try {
        using var stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return null;
      }
    }

    // Stopping can fail (access denied, part of the tree already gone); the wait that follows
    // decides whether the setup is gone.
    private static void TryStop(ISetupProcess setup) {
      try {
        setup.KillTree();
      } catch (Exception e) when (e is Win32Exception or AggregateException
          or InvalidOperationException) {
        System.Diagnostics.Trace.WriteLine($"Setup not stopped: {e.GetType().Name}");
      }
    }

    // A setup that may still run keeps its copy busy; the temp folder then stays behind.
    private static void TryDelete(DirectoryInfo temp) {
      try {
        temp.Delete(recursive: true);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        System.Diagnostics.Trace.WriteLine($"Setup copy not deleted: {e.GetType().Name}");
      }
    }

    private async Task<bool> WaitWhileReporting(
        ISetupProcess setup, string logPath, IProgress<SetupPhase>? progress) {
      DateTime deadline = DateTime.UtcNow + Timeout;
      while (DateTime.UtcNow < deadline) {
        TimeSpan left = deadline - DateTime.UtcNow;
        if (await setup.WaitForExitAsync(left < _poll ? left : _poll).ConfigureAwait(false)) {
          return true;
        }

        Report(logPath, progress);
      }

      return false;
    }
  }
}
