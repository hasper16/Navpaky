using System.ComponentModel;
using System.Diagnostics;

namespace Navpaky.Setup.Install {
  // Runs the extracted setup as a plain process: no shell, no window of its own.
  internal sealed class ProcessSetupLauncher : ISetupLauncher {
    public ISetupProcess Start(string exe, IReadOnlyList<string> args) {
      var start = new ProcessStartInfo(exe) { UseShellExecute = false, CreateNoWindow = true };
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
