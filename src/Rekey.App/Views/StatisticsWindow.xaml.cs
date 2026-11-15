using Rekey.App.ViewModels;
using Wpf.Ui.Appearance;

namespace Rekey.App.Views {
  // The statistics window opened from the tray (spec statistics-and-diagnostics).
  internal sealed partial class StatisticsWindow {
    public StatisticsWindow(StatisticsViewModel model) {
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      DataContext = model;
      RefreshButton.Click += (_, _) => model.Refresh();
      Activated += (_, _) => model.Refresh();
      Loaded += (_, _) => TitleBarButtons.Name(TitleBar);
    }
  }
}
