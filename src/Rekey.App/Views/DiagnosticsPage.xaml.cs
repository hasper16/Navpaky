using Rekey.App.ViewModels;

namespace Rekey.App.Views {
  // Diagnostics and privacy: the watchdogs and what Rekey keeps; statistics join it in
  // add-statistics-and-diagnostics.
  internal sealed partial class DiagnosticsPage {
    public DiagnosticsPage(SettingsViewModel model) {
      InitializeComponent();
      DataContext = model;
    }
  }
}
