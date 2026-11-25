using System.IO;
using Microsoft.Win32;

namespace Navpaky.Win32 {
  /// <summary>
  /// What makes an all-users install of Navpaky known to the whole PC: the Installed apps entry,
  /// the shared Start-menu shortcut and the Active Setup sign-in launch (add-machine-wide-install,
  /// design D3).
  /// </summary>
  public sealed class MachineInstall {
    private const string _entryKey = @"Microsoft\Windows\CurrentVersion\Uninstall\navpaky-app";
    private const string _activeSetupKey = @"Microsoft\Active Setup\Installed Components\Navpaky";

    // Never changes: a new version would run the stub again and turn sign-in back on for users who
    // turned it off.
    private const string _activeSetupVersion = "1,0,0,0";

    private static readonly string[] _displayValues = ["DisplayName", "DisplayIcon", "Publisher"];

    private readonly RegistryRoot _machine;
    private readonly RegistryRoot _user;
    private readonly string _commonPrograms;
    private readonly string _userPrograms;

    /// <summary>
    /// Initializes a new instance of the <see cref="MachineInstall" /> class.
    /// </summary>
    /// <param name="machine">Where the machine-wide keys go.</param>
    /// <param name="user">Where the current user's keys are.</param>
    /// <param name="commonPrograms">The Start-menu programs folder shared by all users.</param>
    /// <param name="userPrograms">The current user's Start-menu programs folder.</param>
    public MachineInstall(
        RegistryRoot machine, RegistryRoot user, string commonPrograms, string userPrograms) {
      _machine = machine;
      _user = user;
      _commonPrograms = commonPrograms;
      _userPrograms = userPrograms;
    }

    /// <summary>
    /// Gets the folder an all-users install goes into: <c>%ProgramFiles%\Navpaky</c>.
    /// </summary>
    public static string MachineRoot => Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Navpaky");

    /// <summary>
    /// Returns the real keys and folders of this PC and the current user.
    /// </summary>
    /// <returns>The machine install of this PC.</returns>
    public static MachineInstall ForThisPc() {
      return new MachineInstall(
          new RegistryRoot(Registry.LocalMachine, "SOFTWARE"),
          new RegistryRoot(Registry.CurrentUser, "Software"),
          Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
          Environment.GetFolderPath(Environment.SpecialFolder.Programs));
    }

    /// <summary>
    /// Returns whether an install root is the all-users one.
    /// </summary>
    /// <param name="root">The install root that holds <c>Update.exe</c>.</param>
    /// <returns><see langword="true" /> for <c>%ProgramFiles%\Navpaky</c>.</returns>
    public static bool IsMachineRoot(string root) {
      return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root))
          .Equals(MachineRoot, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers a fresh all-users install: the Installed apps entry, the shared shortcut in place
    /// of Velopack's per-user one, and Active Setup when Navpaky starts at sign-in.
    /// </summary>
    /// <param name="root">The install root.</param>
    /// <param name="version">The installed version.</param>
    /// <param name="signIn">Whether every user's first sign-in turns on launch at sign-in.</param>
    public void Register(string root, string version, bool signIn) {
      RefreshEntry(root, version);
      DesktopShortcut.Remove(Path.Join(_userPrograms, DesktopShortcut.FileName), root);
      DesktopShortcut.Create(Path.Join(_commonPrograms, DesktopShortcut.FileName), root);
      if (signIn) {
        using RegistryKey key = Create(_machine, _activeSetupKey);
        key.SetValue(string.Empty, "Navpaky");
        key.SetValue("Version", _activeSetupVersion);
        key.SetValue("IsInstalled", 1, RegistryValueKind.DWord);

        // reg.exe, not Navpaky: Active Setup runs the stub before the desktop appears. The value
        // is the one SignInLaunch writes.
        string launcher = Path.Join(root, "Navpaky.exe");
        string stub = @"reg.exe add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v Navpaky "
            + $"/t REG_SZ /d \"\\\"{launcher}\\\"\" /f";
        key.SetValue("StubPath", stub);
      } else {
        Delete(_machine, _activeSetupKey);
      }
    }

    /// <summary>
    /// Writes the machine-wide Installed apps entry for a version and deletes the per-user entry
    /// Velopack wrote for the current user, taking over its display values.
    /// </summary>
    /// <param name="root">The install root.</param>
    /// <param name="version">The installed version.</param>
    public void RefreshEntry(string root, string version) {
      using RegistryKey entry = Create(_machine, _entryKey);
      using (RegistryKey? velopack = Open(_user, _entryKey)) {
        foreach (string name in _displayValues) {
          if (velopack?.GetValue(name) is string value) {
            entry.SetValue(name, value);
          }
        }
      }

      if (entry.GetValue("DisplayName") is null) {
        entry.SetValue("DisplayName", "Navpaky");
      }

      entry.SetValue("DisplayVersion", version);
      entry.SetValue("InstallLocation", root);
      string app = Path.Join(root, "current", "Navpaky.App.exe");
      entry.SetValue("UninstallString", $"\"{app}\" --uninstall-all-users");
      entry.SetValue("NoModify", 1, RegistryValueKind.DWord);
      entry.SetValue("NoRepair", 1, RegistryValueKind.DWord);
      Delete(_user, _entryKey);
    }

    /// <summary>
    /// Marks Active Setup as done for the current user, so it never overrides the sign-in choice
    /// this user already made.
    /// </summary>
    public void MarkActiveSetupDone() {
      using RegistryKey key = Create(_user, _activeSetupKey);
      key.SetValue("Version", _activeSetupVersion);
    }

    /// <summary>
    /// Removes the Installed apps entry, the Active Setup key and the shared shortcut.
    /// </summary>
    /// <param name="root">The install root the shortcut points into.</param>
    public void Unregister(string root) {
      Delete(_machine, _entryKey);
      Delete(_machine, _activeSetupKey);
      DesktopShortcut.Remove(Path.Join(_commonPrograms, DesktopShortcut.FileName), root);
    }

    private static RegistryKey Create(RegistryRoot root, string key) {
      return root.Hive.CreateSubKey($@"{root.Software}\{key}");
    }

    private static RegistryKey? Open(RegistryRoot root, string key) {
      return root.Hive.OpenSubKey($@"{root.Software}\{key}");
    }

    private static void Delete(RegistryRoot root, string key) {
      root.Hive.DeleteSubKeyTree($@"{root.Software}\{key}", throwOnMissingSubKey: false);
    }
  }
}
