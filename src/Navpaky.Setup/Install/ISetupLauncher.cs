namespace Navpaky.Setup.Install {
  /// <summary>
  /// Starts Velopack's setup; tests use a process that ignores being stopped.
  /// </summary>
  internal interface ISetupLauncher {
    /// <summary>Starts the setup.</summary>
    /// <param name="exe">The setup executable.</param>
    /// <param name="args">Its arguments, each passed as one argument.</param>
    /// <returns>The running setup.</returns>
    ISetupProcess Start(string exe, IReadOnlyList<string> args);
  }
}
