namespace Rekey.App.Tray {
  // What the tray menu asks of the app beyond the settings it changes itself. Updates checks
  // for updates, or restarts into one that is ready.
  internal sealed record TrayCommands(
      Action OpenSettings, Action OpenStatistics, Action Updates, Action OpenAbout, Action Quit);
}
