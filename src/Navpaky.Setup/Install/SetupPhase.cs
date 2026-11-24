namespace Navpaky.Setup.Install {
  // What the Installing page shows, read from Velopack's log: it reports no progress when silent.
  internal enum SetupPhase {
    Copying,
    Applying,
    Finishing,
  }
}
