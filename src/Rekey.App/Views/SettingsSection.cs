using System.Windows;
using Wpf.Ui.Controls;

namespace Rekey.App.Views {
  // One entry of the settings window's section list, named by the UI string TitleKey, and the
  // page it shows.
  internal sealed record SettingsSection(string TitleKey, SymbolRegular Icon, UIElement Page);
}
