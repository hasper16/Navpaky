using Microsoft.Win32;

namespace Navpaky.Setup.Install {
  // Reads Navpaky's Installed apps entries: Velopack's per-user one in HKCU, key named after the
  // pack id (Velopack 1.2.161 registry.rs), and the all-users one in HKLM under the same name
  // (design D3 of add-machine-wide-install).
  internal sealed class UninstallEntry : IUninstallEntry {
    private const string _key = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\navpaky-app";

    public string? InstallLocation() {
      return LocationIn(Registry.CurrentUser);
    }

    public string? MachineInstallLocation() {
      return LocationIn(Registry.LocalMachine);
    }

    private static string? LocationIn(RegistryKey hive) {
      using RegistryKey? entry = hive.OpenSubKey(_key);
      return entry?.GetValue("InstallLocation") as string;
    }
  }
}
