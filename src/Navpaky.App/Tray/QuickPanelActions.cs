namespace Navpaky.App.Tray {
  // What the quick panel asks of the app beyond the settings it changes itself. Changed
  // redraws the tray after the panel changed something.
  internal sealed record QuickPanelActions(
      Action SwitchLayout,
      Action OpenSettings,
      Action OpenWelcome,
      Action OpenStatistics,
      Action Quit,
      Action Changed);
}
