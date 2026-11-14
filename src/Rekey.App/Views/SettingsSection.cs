using System.Windows;
using Wpf.Ui.Controls;

namespace Rekey.App.Views {
  // One entry of the settings window's section list and the page it shows.
  internal sealed record SettingsSection(string Title, SymbolRegular Icon, UIElement Page);
}
