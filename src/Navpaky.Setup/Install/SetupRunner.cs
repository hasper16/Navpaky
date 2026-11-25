using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
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

    // How long the wizard's own run of the install hook may take (design D3).
    public TimeSpan HookWait { get; init; } = TimeSpan.FromMinutes(2);

    // Starts the wizard itself elevated for an all-users install (design D2 of
    // add-machine-wide-install).
    public ISetupLauncher Elevated { get; init; } = new ProcessSetupLauncher(elevate: true);

    public string Self { get; init; } = Environment.ProcessPath ?? string.Empty;

    public async Task<SetupOutcome> RunAsync(
        InstallChoices choices,
        bool forAllUsers,
        DataFolderPair data,
        string logPath,
        IProgress<SetupPhase>? progress) {
      choices.WriteTo(data.Local);
      if (forAllUsers) {
        using ISetupProcess elevated = Elevated.Start(
            Self, MachineSetup.Arguments(choices.Attempt, choices.LaunchAtSignIn));

        // The administrator's own account may have run the install hook: the wizard user's
        // choices then come from running it again below.
        return await Finish(elevated, choices, data, logPath, progress).ConfigureAwait(false);
      }

      DirectoryInfo temp = Directory.CreateTempSubdirectory("navpaky-setup-");
      try {
        string exe = _extractSetup(temp.FullName);
        using ISetupProcess setup = _launcher.Start(
            exe, ["--silent", "--log", logPath, "--installto", choices.InstallFolder]);
        return await Finish(setup, choices, data, logPath, progress).ConfigureAwait(false);
      } finally {
        TryDelete(temp);
      }
    }

    // The version Velopack passes to the hook, as the setup logged it.
    private static string? PackageVersion(string logPath) {
      const string label = "Package Version: ";
      string? line = ReadShared(logPath)?.Split('\n')
          .FirstOrDefault(entry => entry.Contains(label, StringComparison.Ordinal));
      return line?[(line.IndexOf(label, StringComparison.Ordinal) + label.Length)..].Trim();
    }

    // Reports the phases the log shows that were not reported yet, in order: the log only grows,
    // and every poll reads it whole.
    private static int Report(string logPath, IProgress<SetupPhase>? progress, int reported) {
      if (progress is null || ReadShared(logPath) is not { } log) {
        return reported;
      }

      for (int next = reported; next < _phases.Length; next++) {
        (string line, SetupPhase phase) = _phases[next];
        if (!log.Contains(line, StringComparison.Ordinal)) {
          break;
        }

        progress.Report(phase);
        reported = next + 1;
      }

      return reported;
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

    // Velopack 1.2.161 stops a hook after 30 s and still exits 0; a never-scanned app can take
    // that long to start. The installed app runs the same hook without that limit.
    private async Task RunHookAgain(string installFolder, string logPath) {
      string app = Path.Join(installFolder, "current", "Navpaky.App.exe");
      string? version = PackageVersion(logPath) ?? PackageManifest.VersionIn(installFolder);
      if (!File.Exists(app) || version is not { Length: > 0 }) {
        return;
      }

      using ISetupProcess hook = _launcher.Start(app, ["--veloapp-install", version]);
      if (!await hook.WaitForExitAsync(HookWait).ConfigureAwait(false)) {
        TryStop(hook);
        await hook.WaitForExitAsync(KillWait).ConfigureAwait(false);
      }
    }

    private async Task<SetupOutcome> Finish(
        ISetupProcess setup,
        InstallChoices choices,
        DataFolderPair data,
        string logPath,
        IProgress<SetupPhase>? progress) {
      var reported = new StrongBox<int>();
      bool exited = await WaitWhileReporting(setup, logPath, progress, reported)
          .ConfigureAwait(false);
      if (!exited) {
        TryStop(setup);
        return await setup.WaitForExitAsync(KillWait).ConfigureAwait(false)
            ? SetupOutcome.Failed
            : SetupOutcome.MayStillRun;
      }

      Report(logPath, progress, reported.Value);
      if (setup.ExitCode != 0) {
        return SetupOutcome.Failed;
      }

      string acknowledgement = Path.Join(data.Local, choices.AcknowledgementName);
      if (!File.Exists(acknowledgement)) {
        await RunHookAgain(choices.InstallFolder, logPath).ConfigureAwait(false);
      }

      return File.Exists(acknowledgement) ? SetupOutcome.Succeeded : SetupOutcome.Failed;
    }

    private async Task<bool> WaitWhileReporting(
        ISetupProcess setup,
        string logPath,
        IProgress<SetupPhase>? progress,
        StrongBox<int> reported) {
      DateTime deadline = DateTime.UtcNow + Timeout;
      while (DateTime.UtcNow < deadline) {
        TimeSpan left = deadline - DateTime.UtcNow;
        if (await setup.WaitForExitAsync(left < _poll ? left : _poll).ConfigureAwait(false)) {
          return true;
        }

        reported.Value = Report(logPath, progress, reported.Value);
      }

      return false;
    }
  }
}
