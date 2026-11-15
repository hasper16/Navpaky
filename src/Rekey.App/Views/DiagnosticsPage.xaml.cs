using Rekey.App.ViewModels;

namespace Rekey.App.Views {
  // Diagnostics and privacy: the watchdogs, statistics, the word-analysis log and the
  // diagnostics export.
  internal sealed partial class DiagnosticsPage {
    public DiagnosticsPage(SettingsViewModel model, Action openStatistics, Action openExport) {
      InitializeComponent();
      DataContext = model;
      StatisticsButton.Click += (_, _) => openStatistics();
      ExportButton.Click += (_, _) => openExport();
    }
  }
}
