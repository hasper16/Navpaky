namespace Navpaky.App.Updates {
  /// <summary>
  /// The Navpaky processes of an all-users install other than this one, in any session.
  /// </summary>
  internal interface IOtherSessions {
    /// <summary>Waits for every one of them to exit.</summary>
    /// <param name="timeout">The longest time to wait.</param>
    /// <returns>
    /// <see langword="true" /> if none is running any more; otherwise, <see langword="false" />.
    /// </returns>
    bool WaitForExit(TimeSpan timeout);

    /// <summary>Stops the ones still running.</summary>
    void Stop();
  }
}
