using System.Diagnostics;
using System.IO;
using System.Text;

namespace Navpaky.App.Updates {
  // Asks whether to delete Navpaky's data after uninstall, in a process of its own: Velopack's
  // uninstall hook must show no UI and ends after 30 seconds, while the user may take longer
  // (design R11 of add-auto-update-and-installer). Windows PowerShell ships with every supported
  // Windows and runs from System32, outside the install root Velopack deletes.
  internal static class DataQuestion {
    // Deletes only after Yes and once Velopack has removed the old installation; an Update.exe
    // still there after 30 s means Navpaky was installed again, which keeps the data (fix 2.12 of
    // add-final-acceptance). Deletes both data folders, retrying while the closing Navpaky still
    // holds a file, then names any folder left for the user to delete. Every value comes from
    // the environment, never the script.
    private const string _script = """
        Add-Type -AssemblyName PresentationFramework
        $answer = [System.Windows.MessageBox]::Show(
            $env:NAVPAKY_QUESTION, $env:NAVPAKY_TITLE, 'YesNo', 'Question', 'No')
        if ($answer -ne 'Yes') { exit }
        $installed = Join-Path $env:NAVPAKY_INSTALL 'Update.exe'
        for ($i = 0; $i -lt 60 -and (Test-Path -LiteralPath $installed); $i++) {
          Start-Sleep -Milliseconds 500
        }
        if (Test-Path -LiteralPath $installed) {
          [void][System.Windows.MessageBox]::Show(
              $env:NAVPAKY_KEPT, $env:NAVPAKY_TITLE, 'OK', 'Information')
          exit
        }
        $left = @()
        foreach ($folder in @($env:NAVPAKY_SETTINGS, $env:NAVPAKY_LOCAL)) {
          for ($i = 0; $i -lt 10 -and (Test-Path -LiteralPath $folder); $i++) {
            try {
              Remove-Item -LiteralPath $folder -Recurse -Force -ErrorAction Stop
            } catch {
              Start-Sleep -Milliseconds 500
            }
          }
          if (Test-Path -LiteralPath $folder) { $left += $folder }
        }
        if ($left.Count -eq 0) { exit }
        [void][System.Windows.MessageBox]::Show(
            ($env:NAVPAKY_NOT_DELETED -f ($left -join "`n")), $env:NAVPAKY_TITLE, 'OK', 'Warning')
        """;

    public static ProcessStartInfo StartInfo(DataFolders data, string installRoot) {
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

      start.Environment["NAVPAKY_SETTINGS"] = data.Settings;
      start.Environment["NAVPAKY_LOCAL"] = data.Local;
      start.Environment["NAVPAKY_INSTALL"] = installRoot;
      start.Environment["NAVPAKY_KEPT"] = Strings.UninstallDataKept;
      start.Environment["NAVPAKY_QUESTION"] = Strings.UninstallDeleteData;
      start.Environment["NAVPAKY_TITLE"] = Strings.UninstallTitle;
      start.Environment["NAVPAKY_NOT_DELETED"] = Strings.UninstallDataNotDeleted;
      return start;
    }
  }
}
