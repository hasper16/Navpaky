namespace Navpaky.App.Tray {
  // What the tray icon and its menu ask of the app beyond the settings they change themselves.
  // Updates checks for updates, or restarts into one that is ready.
  internal sealed record TrayCommands(
      Action OpenSettings,
      Action OpenStatistics,
      Action Updates,
      Action OpenWelcome,
      Action OpenAbout,
      Action Quit,
      Action OpenQuickPanel);
}
