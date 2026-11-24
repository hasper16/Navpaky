namespace Navpaky.Setup.Install {
  /// <summary>
  /// A running setup: only the process the wizard started, never its descendants.
  /// </summary>
  internal interface ISetupProcess : IDisposable {
    /// <summary>Gets the exit code once the process has exited.</summary>
    int ExitCode { get; }

    /// <summary>Waits for the process to exit.</summary>
    /// <param name="timeout">How long to wait.</param>
    /// <returns><see langword="true" /> when it exited within the time.</returns>
    Task<bool> WaitForExitAsync(TimeSpan timeout);

    /// <summary>Stops the process and everything it started.</summary>
    void KillTree();
  }
}
