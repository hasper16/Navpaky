namespace Navpaky.App.Updates {
  /// <summary>
  /// Navpaky's shortcut on the user's desktop (add-install-wizard, design D6).
  /// </summary>
  internal interface IDesktopShortcut {
    /// <summary>Creates or replaces the shortcut to the install's root launcher.</summary>
    /// <param name="installFolder">The install root that holds <c>Navpaky.exe</c>.</param>
    void Create(string installFolder);

    /// <summary>Removes Navpaky's shortcut when it points into the install folder.</summary>
    /// <param name="installFolder">The install root.</param>
    void Remove(string installFolder);
  }
}
