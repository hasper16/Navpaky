using System.Windows;
using Navpaky.App.Localization;
using Navpaky.App.ViewModels;
using Navpaky.Ui;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The statistics window opened from the tray (spec statistics-and-diagnostics).
  internal sealed partial class StatisticsWindow {
    public StatisticsWindow(StatisticsViewModel model) {
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      MaxHeight = SystemParameters.WorkArea.Height;
      DataContext = model;
      RefreshButton.Click += (_, _) => model.Refresh();
      Activated += (_, _) => model.Refresh();
      Loaded += (_, _) => TitleBarButtons.Name(TitleBar);
      Localizer.Follow(this, model.Refresh);
    }
  }
}
