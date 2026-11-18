using Navpaky.App.ViewModels;

namespace Navpaky.App.Views {
  // Correction: Auto Correction, Sensitivity, Trigger Keys and the replacement rules.
  internal sealed partial class CorrectionPage {
    public CorrectionPage(SettingsViewModel model) {
      InitializeComponent();
      DataContext = model;
    }
  }
}
