namespace Navpaky.Setup.Install {
  /// <summary>
  /// Navpaky's entries in Windows "Installed apps": Velopack's for the current user, and the
  /// machine-wide one of an all-users install.
  /// </summary>
  internal interface IUninstallEntry {
    /// <summary>Returns the folder the current user's entry says Navpaky is installed in.</summary>
    /// <returns>The folder, or <see langword="null" /> when there is no entry.</returns>
    string? InstallLocation();

    /// <summary>Returns the folder the machine-wide entry says Navpaky is installed in.</summary>
    /// <returns>The folder, or <see langword="null" /> when there is no entry.</returns>
    string? MachineInstallLocation();
  }
}
