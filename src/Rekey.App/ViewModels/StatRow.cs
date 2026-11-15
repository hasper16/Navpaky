namespace Rekey.App.ViewModels {
  // One line of the statistics window: a counter for today and for the last 30 days.
  internal sealed record StatRow(string Name, string Today, string Month);
}
