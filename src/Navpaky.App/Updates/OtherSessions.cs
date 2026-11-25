using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Navpaky.App.Updates {
  // Navpaky processes started from the install root, in every session, except this one. Only the
  // elevated hooks use it: reading another user's process path needs administrator rights.
  internal sealed class OtherSessions : IOtherSessions {
    private readonly string _root;

    public OtherSessions(string root) {
      _root = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
    }

    public bool WaitForExit(TimeSpan timeout) {
      var deadline = Stopwatch.StartNew();
      foreach (Process process in Find()) {
        using (process) {
          TimeSpan left = timeout - deadline.Elapsed;
          if (left <= TimeSpan.Zero || !process.WaitForExit(left)) {
            return false;
          }
        }
      }

      return true;
    }

    public void Stop() {
      foreach (Process process in Find()) {
        using (process) {
          try {
            process.Kill();
          } catch (Exception e) when (e is Win32Exception or InvalidOperationException) {
            Trace.WriteLine($"Navpaky not stopped: {e.GetType().Name}");
          }
        }
      }
    }

    // A process whose path can't be read is left alone: it may not be ours.
    private List<Process> Find() {
      int self = Environment.ProcessId;
      var found = new List<Process>();
      foreach (Process process in Process.GetProcessesByName("Navpaky.App")) {
        string? path = null;
        try {
          path = process.Id == self ? null : process.MainModule?.FileName;
        } catch (Exception e) when (e is Win32Exception or InvalidOperationException) {
          Trace.WriteLine($"Navpaky process not read: {e.GetType().Name}");
        }

        if (path?.StartsWith(_root, StringComparison.OrdinalIgnoreCase) == true) {
          found.Add(process);
        } else {
          process.Dispose();
        }
      }

      return found;
    }
  }
}
