using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Navpaky.Setup.Install {
  // The wizard started elevated for an all-users install (design D2 of add-machine-wide-install).
  // Only an attempt id and a flag come from the unelevated side: the setup, its folder, its log and
  // the target are all derived here, so a standard user whose administrator typed a password can't
  // steer an elevated write or start.
  internal sealed class MachineSetup {
    public const string Argument = "--all-users-setup";

    private readonly bool _elevated;
    private readonly Func<string, string> _extractSetup;
    private readonly ISetupLauncher _launcher;
    private readonly Action<string, string, bool> _register;
    private readonly string _machineRoot;
    private readonly string _tempRoot;

    // register gets the install root, the installed version and the sign-in choice.
    public MachineSetup(
        bool elevated,
        Func<string, string> extractSetup,
        ISetupLauncher launcher,
        Action<string, string, bool> register,
        string machineRoot,
        string tempRoot) {
      _elevated = elevated;
      _extractSetup = extractSetup;
      _launcher = launcher;
      _register = register;
      _machineRoot = machineRoot;
      _tempRoot = tempRoot;
    }

    // The same limit the unelevated wizard gives the whole setup.
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    public static string[] Arguments(Guid attempt, bool signIn) {
      return [Argument, attempt.ToString("N"), signIn ? "on" : "off"];
    }

    public static (Guid Attempt, bool SignIn)? Parse(IReadOnlyList<string> args) {
      if (args is not [Argument, var id, var signIn] || signIn is not ("on" or "off")
          || !Guid.TryParseExact(id, "N", out Guid attempt)) {
        return null;
      }

      return (attempt, signIn == "on");
    }

    // Where the elevated wizard keeps the setup and its log: the elevated account's own temp
    // folder, which the wizard can read for progress when that account is the same user.
    public static string WorkFolder(string tempRoot, Guid attempt) {
      return Path.Join(tempRoot, $"navpaky-setup-{attempt:N}");
    }

    public static string LogOf(string tempRoot, Guid attempt) {
      return Path.Join(WorkFolder(tempRoot, attempt), "setup.log");
    }

    public async Task<int> RunAsync(Guid attempt, bool signIn) {
      string work = WorkFolder(_tempRoot, attempt);

      // A folder that is already there could be someone else's.
      if (!_elevated || Directory.Exists(work)) {
        return 1;
      }

      string? exe = null;
      try {
        Directory.CreateDirectory(work);
        exe = _extractSetup(work);
        using ISetupProcess setup = _launcher.Start(
            exe,
            ["--silent", "--log", LogOf(_tempRoot, attempt), "--installto", _machineRoot]);
        if (!await setup.WaitForExitAsync(Timeout).ConfigureAwait(false)) {
          setup.KillTree();
          return 1;
        }

        if (setup.ExitCode != 0 || PackageManifest.VersionIn(_machineRoot) is not { } version) {
          return 1;
        }

        _register(_machineRoot, version, signIn);
        return 0;
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException
          or Win32Exception or System.Runtime.InteropServices.COMException) {
        Trace.WriteLine($"All-users setup failed: {e.GetType().Name}");
        return 1;
      } finally {
        if (exe is not null) {
          TryDelete(exe);
        }
      }
    }

    private static void TryDelete(string file) {
      try {
        File.Delete(file);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        Trace.WriteLine($"Setup copy not deleted: {e.GetType().Name}");
      }
    }
  }
}
