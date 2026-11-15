using Microsoft.Win32;

namespace Rekey.Win32 {
  /// <summary>
  /// Turns starting Rekey at sign-in on and off through the user's Run key, and reads it back
  /// the way Task Manager's Startup apps page shows it (design D7 of add-tray-and-settings-ui).
  /// </summary>
  public sealed class SignInLaunch {
    private const string _windowsRoot = @"Software\Microsoft\Windows\CurrentVersion";

    // Task Manager marks a disabled entry with an odd first byte (3) and an enabled one with 2.
    private const byte _disabledBit = 0x01;

    private readonly RegistryKey _hive;
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInLaunch" /> class for the current
    /// user's real keys.
    /// </summary>
    public SignInLaunch()
        : this(Registry.CurrentUser, _windowsRoot, "Rekey") {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SignInLaunch" /> class for keys under a
    /// given root, as tests need.
    /// </summary>
    /// <param name="hive">The registry hive, such as <see cref="Registry.CurrentUser" />.</param>
    /// <param name="root">
    /// The key that stands for <c>Software\Microsoft\Windows\CurrentVersion</c>.
    /// </param>
    /// <param name="name">The name of Rekey's entry.</param>
    public SignInLaunch(RegistryKey hive, string root, string name) {
      _hive = hive;
      _name = name;
      RunKey = $@"{root}\Run";
      ApprovedKey = $@"{root}\Explorer\StartupApproved\Run";
    }

    /// <summary>
    /// Gets the path of the Run key under the hive.
    /// </summary>
    public string RunKey { get; }

    /// <summary>
    /// Gets the path of the key where Task Manager records disabled startup entries.
    /// </summary>
    public string ApprovedKey { get; }

    /// <summary>
    /// Returns whether Rekey starts at sign-in now.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the Run entry exists and Task Manager has not disabled it;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public bool IsOn() {
      using RegistryKey? run = _hive.OpenSubKey(RunKey);
      if (run?.GetValue(_name) is not string) {
        return false;
      }

      using RegistryKey? approved = _hive.OpenSubKey(ApprovedKey);
      return approved?.GetValue(_name) is not byte[] { Length: > 0 } state
          || (state[0] & _disabledBit) == 0;
    }

    /// <summary>
    /// Turns starting at sign-in on or off.
    /// </summary>
    /// <param name="on">
    /// <see langword="true" /> to start Rekey at sign-in; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="exePath">The program Windows starts.</param>
    public void Set(bool on, string exePath) {
      using RegistryKey run = _hive.CreateSubKey(RunKey);
      if (on) {
        run.SetValue(_name, $"\"{exePath}\"", RegistryValueKind.String);
      } else {
        run.DeleteValue(_name, throwOnMissingValue: false);
      }

      // Turning it on in Rekey overrides an earlier disable in Task Manager.
      using RegistryKey? approved = _hive.OpenSubKey(ApprovedKey, writable: true);
      approved?.DeleteValue(_name, throwOnMissingValue: false);
    }
  }
}
