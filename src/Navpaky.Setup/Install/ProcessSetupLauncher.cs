using System.ComponentModel;
using System.Diagnostics;

namespace Navpaky.Setup.Install {
  // Runs the extracted setup as a plain process: no shell, no window of its own. Elevated, it goes
  // through the shell's "runas" verb, which shows the UAC prompt and throws Win32Exception 1223
  // when it is declined (design D2 of add-machine-wide-install).
  internal sealed class ProcessSetupLauncher(bool elevate = false) : ISetupLauncher {
    public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
      var start = elevate
          ? new ProcessStartInfo(exe) { UseShellExecute = true, Verb = "runas" }
          : new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true };
      foreach (string arg in args) {
        start.ArgumentList.Add(arg);
      }

      return new Running(Process.Start(start)
          ?? throw new Win32Exception("The setup did not start."));
    }

    private sealed class Running(Process process) : ISetupProcess {
      public int ExitCode => process.ExitCode;

      public async Task<bool> WaitForExitAsync(TimeSpan timeout) {
        using var cancel = new CancellationTokenSource(timeout);
        try {
          await process.WaitForExitAsync(cancel.Token).ConfigureAwait(false);
          return true;
        } catch (OperationCanceledException) {
          return false;
        }
      }

      public void KillTree() {
        process.Kill(entireProcessTree: true);
      }

      public void Dispose() {
        process.Dispose();
      }
    }
  }
}
