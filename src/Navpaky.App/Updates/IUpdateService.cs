namespace Navpaky.App.Updates {
  /// <summary>
  /// Finds, downloads and applies updates; <see cref="UpdateScheduler" /> decides when.
  /// </summary>
  internal interface IUpdateService {
    /// <summary>
    /// Gets a value indicating whether Navpaky was installed by its installer and so can update
    /// itself.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Checks the release feed and downloads a newer, verified version.
    /// </summary>
    /// <returns>
    /// <see cref="UpdateStatus.UpToDate" />, <see cref="UpdateStatus.ReadyToRestart" />, or
    /// <see cref="UpdateStatus.Failed" /> when the feed could not be read or the download was
    /// discarded; the task never faults.
    /// </returns>
    Task<UpdateStatus> CheckAndDownloadAsync();

    /// <summary>
    /// Exits Navpaky, applies the downloaded update and starts the new version.
    /// </summary>
    void RestartToUpdate();
  }
}
