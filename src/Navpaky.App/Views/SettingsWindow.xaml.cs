using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The settings window (design D3): one page per section, every change applied at once.
  internal sealed partial class SettingsWindow {
    private readonly IReadOnlyList<SettingsSection> _sections;

    public SettingsWindow(IReadOnlyList<SettingsSection> sections) {
      _sections = sections;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      Sections.ItemsSource = sections;
      Sections.SelectionChanged += (_, _) => {
        if (Sections.SelectedItem is SettingsSection section) {
          Page.Content = section.Page;
        }
      };
      Sections.SelectedIndex = 0;
      Loaded += (_, _) => TitleBarButtons.Name(TitleBar);
    }

    public void Show(Type page) {
      Sections.SelectedItem = _sections.FirstOrDefault(section => section.Page.GetType() == page);
    }
  }
}
