using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Rekey.App.Views {
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
      Loaded += (_, _) => NameTitleBarButtons();
    }

    public void Show(Type page) {
      Sections.SelectedItem = _sections.FirstOrDefault(section => section.Page.GetType() == page);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent) {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        yield return child;
        foreach (DependencyObject grandchild in Descendants(child)) {
          yield return grandchild;
        }
      }
    }

    // WPF UI's caption buttons carry only glyphs; screen readers need their names.
    private void NameTitleBarButtons() {
      string[] names = [Strings.WindowMinimize, Strings.WindowMaximize, Strings.WindowClose];
      List<Button> buttons = [.. Descendants(TitleBar).OfType<Button>()];
      for (int i = 0; i < buttons.Count && i < names.Length; i++) {
        AutomationProperties.SetName(buttons[buttons.Count - names.Length + i], names[i]);
      }
    }
  }
}
