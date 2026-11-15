namespace Rekey.App.Updates {
  // What the tray's update item shows.
  internal enum UpdateStatus {
    // No check has run yet in this session.
    Idle,

    // Rekey was not installed by its installer, so it cannot update itself.
    Unavailable,
    Checking,
    UpToDate,

    // A verified newer version is downloaded and applies on restart.
    ReadyToRestart,

    // The feed could not be read, or the download was discarded.
    Failed,
  }
}
