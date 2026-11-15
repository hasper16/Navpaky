using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Rekey.Win32;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Rekey.DesktopChecks {
  // Reads and changes the user settings the checks need, and how to put them back.
  [SuppressMessage(
      "Security",
      "S6640:Avoid using unsafe code blocks",
      Justification = "SystemParametersInfo takes the setting through a pointer.")]
  internal static class SystemState {
    // SPI_GETTHREADLOCALINPUTSETTINGS / SPI_SET...: "Let me use a different input method for
    // each app window".
    private const uint _getPerWindowInput = 0x104E;
    private const uint _setPerWindowInput = 0x104F;

    // The value name SignInLaunch uses for Rekey's entries.
    private const string _signInName = "Rekey";

    public static string DataFolder { get; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Rekey");

    // Where the language list is saved before the checks change it, so a crashed run can still
    // be undone with the restore-languages command.
    public static string LanguageSnapshot { get; } =
        Path.Join(Path.GetTempPath(), "rekey-desktop-checks-languages.txt");

    public static LanguageList ReadLanguages() {
      return LanguageList.Parse(PowerShell(LanguageList.ReadScript));
    }

    public static void AddLanguage(string tag) {
      PowerShell(
          $"$l = Get-WinUserLanguageList; $l.Add({Quote(tag)}); Set-WinUserLanguageList $l -Force");
    }

    // Removes the language the checks added and leaves every other language object, with all
    // its settings, as it is. It never rebuilds the list: new language objects could reset settings
    // PowerShell does not show, such as text prediction, so a remaining difference is reported.
    public static void RestoreLanguages(LanguageSnapshot snapshot) {
      IReadOnlyList<string> added = snapshot.ToRemoveFrom(ReadLanguages());
      if (added.Count > 0) {
        PowerShell(LanguageList.RemoveScript(added));
      }

      if (!ReadLanguages().SameAs(snapshot.List)) {
        throw new RecoveryException(
            $"The language list still differs from the one saved in {LanguageSnapshot}; "
            + "compare them in Settings > Time & language > Language & region, fix the list "
            + "there, then delete the saved file.");
      }
    }

    public static unsafe bool PerWindowInput() {
      int value = 0;
      if (!PInvoke.SystemParametersInfo(
          (SYSTEM_PARAMETERS_INFO_ACTION)_getPerWindowInput, 0, &value, 0)) {
        throw new Win32Exception(Marshal.GetLastPInvokeError());
      }

      return value != 0;
    }

    public static unsafe void SetPerWindowInput(bool on) {
      const SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS save =
          SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_UPDATEINIFILE
          | SYSTEM_PARAMETERS_INFO_UPDATE_FLAGS.SPIF_SENDCHANGE;
      if (!PInvoke.SystemParametersInfo(
          (SYSTEM_PARAMETERS_INFO_ACTION)_setPerWindowInput, 0, (void*)(on ? 1 : 0), save)) {
        throw new Win32Exception(Marshal.GetLastPInvokeError());
      }
    }

    public static bool CapsLockOn() {
      return (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_CAPITAL) & 1) != 0;
    }

    public static Point Cursor() {
      PInvoke.GetCursorPos(out Point point);
      return point;
    }

    // Returns how to put a Rekey data file back as it was, present or absent.
    public static Action KeepFile(string name) {
      string path = Path.Join(DataFolder, name);
      byte[]? before = File.Exists(path) ? File.ReadAllBytes(path) : null;
      return () => {
        if (before is null) {
          File.Delete(path);
        } else {
          File.WriteAllBytes(path, before);
        }
      };
    }

    // Default settings with the welcome already completed, so the welcome window does not take
    // the front from the checks' test windows.
    public static void UseDefaultSettings() {
      Directory.CreateDirectory(DataFolder);
      File.WriteAllText(Path.Join(DataFolder, "settings.json"), "{ \"welcomeCompleted\": true }");
    }

    // Returns how to put Rekey's start-at-sign-in entries back as they were: a first start
    // turns starting at sign-in on for the Debug build under test.
    public static Action KeepSignInLaunch() {
      var launch = new SignInLaunch();
      (string Key, (object Data, RegistryValueKind Kind)? Value)[] before = [
          (launch.RunKey, ReadValue(launch.RunKey)),
          (launch.ApprovedKey, ReadValue(launch.ApprovedKey))];
      return () => {
        foreach ((string key, (object Data, RegistryValueKind Kind)? value) in before) {
          if (value is not { } kept) {
            using RegistryKey? entries = Registry.CurrentUser.OpenSubKey(key, writable: true);
            entries?.DeleteValue(_signInName, throwOnMissingValue: false);
          } else {
            using RegistryKey entries = Registry.CurrentUser.CreateSubKey(key);
            entries.SetValue(_signInName, kept.Data, kept.Kind);
          }
        }
      };
    }

    // The value as stored, with its kind and without expanding environment variables.
    private static (object Data, RegistryValueKind Kind)? ReadValue(string key) {
      using RegistryKey? entries = Registry.CurrentUser.OpenSubKey(key);
      object? data = entries?.GetValue(
          _signInName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
      return data is null ? null : (data, entries!.GetValueKind(_signInName));
    }

    private static string Quote(string value) {
      return "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    private static string PowerShell(string script) {
      string shell = Path.Join(
          Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
      var start = new ProcessStartInfo(shell) {
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
      };
      foreach (string arg in new[] { "-NoProfile", "-NonInteractive", "-Command", script }) {
        start.ArgumentList.Add(arg);
      }

      using Process process = Process.Start(start)
          ?? throw new InvalidOperationException("PowerShell did not start.");
      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();
      if (process.ExitCode != 0) {
        throw new InvalidOperationException($"PowerShell exited with {process.ExitCode}.");
      }

      return output;
    }
  }
}
