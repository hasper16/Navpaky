using Microsoft.Win32;

namespace Navpaky.Setup.Install {
  // Reads Velopack's per-user Installed apps entry: HKCU, key named after the pack id
  // (Velopack 1.2.161 registry.rs).
  internal sealed class UninstallEntry : IUninstallEntry {
    private const string _key = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\navpaky-app";

    public string? InstallLocation() {
      using RegistryKey? entry = Registry.CurrentUser.OpenSubKey(_key);
      return entry?.GetValue("InstallLocation") as string;
    }
  }
}
