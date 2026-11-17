using System.Diagnostics;
using System.IO;
using System.Text;

namespace Rekey.App.Updates {
  // Asks whether to delete Rekey's data after uninstall, in a process of its own: Velopack's
  // uninstall hook must show no UI and ends after 30 seconds, while the user may take longer
  // (design R11 of add-auto-update-and-installer). Windows PowerShell ships with every supported
  // Windows and runs from System32, outside the install root Velopack deletes.
  internal static class DataQuestion {
    // Deletes only after Yes and once Velopack has removed the old installation; an Update.exe
    // still there after 30 s means Rekey was installed again, which keeps the data (fix 2.12 of
    // add-final-acceptance). Retries while the closing Rekey still holds a file, then names the
    // folder for the user to delete. Every value comes from the environment, never the script.
    private const string _script = """
        Add-Type -AssemblyName PresentationFramework
        $answer = [System.Windows.MessageBox]::Show(
            $env:REKEY_QUESTION, $env:REKEY_TITLE, 'YesNo', 'Question', 'No')
        if ($answer -ne 'Yes') { exit }
        $installed = Join-Path $env:REKEY_INSTALL 'Update.exe'
        for ($i = 0; $i -lt 60 -and (Test-Path -LiteralPath $installed); $i++) {
          Start-Sleep -Milliseconds 500
        }
        if (Test-Path -LiteralPath $installed) {
          [void][System.Windows.MessageBox]::Show(
              $env:REKEY_KEPT, $env:REKEY_TITLE, 'OK', 'Information')
          exit
        }
        for ($i = 0; $i -lt 10; $i++) {
          try {
            Remove-Item -LiteralPath $env:REKEY_DATA -Recurse -Force -ErrorAction Stop
            exit
          } catch {
            Start-Sleep -Milliseconds 500
          }
        }
        [void][System.Windows.MessageBox]::Show(
            ($env:REKEY_NOT_DELETED -f $env:REKEY_DATA), $env:REKEY_TITLE, 'OK', 'Warning')
        """;

    public static ProcessStartInfo StartInfo(string dataFolder, string installRoot) {
      var start = new ProcessStartInfo(
          Path.Join(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe")) {
        // Not the install folder the hook runs in, which an open handle would keep on disk.
        WorkingDirectory = Environment.SystemDirectory,
        UseShellExecute = false,
        CreateNoWindow = true,
      };
      foreach (string argument in new[] {
          "-NoProfile", "-NonInteractive", "-EncodedCommand",
          Convert.ToBase64String(Encoding.Unicode.GetBytes(_script)) }) {
        start.ArgumentList.Add(argument);
      }

      start.Environment["REKEY_DATA"] = dataFolder;
      start.Environment["REKEY_INSTALL"] = installRoot;
      start.Environment["REKEY_KEPT"] = Strings.UninstallDataKept;
      start.Environment["REKEY_QUESTION"] = Strings.UninstallDeleteData;
      start.Environment["REKEY_TITLE"] = Strings.UninstallTitle;
      start.Environment["REKEY_NOT_DELETED"] = Strings.UninstallDataNotDeleted;
      return start;
    }
  }
}
