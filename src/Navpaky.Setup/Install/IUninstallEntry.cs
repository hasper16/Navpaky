namespace Navpaky.Setup.Install {
  /// <summary>
  /// Navpaky's entry in Windows "Installed apps" for the current user, as Velopack writes it.
  /// </summary>
  internal interface IUninstallEntry {
    /// <summary>Returns the folder the entry says Navpaky is installed in.</summary>
    /// <returns>The folder, or <see langword="null" /> when there is no entry.</returns>
    string? InstallLocation();
  }
}
