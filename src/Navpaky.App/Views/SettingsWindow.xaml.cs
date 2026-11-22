using System.Windows;
using System.Windows.Controls;
using Navpaky.App.Localization;
using Wpf.Ui.Appearance;

namespace Navpaky.App.Views {
  // The settings window: every section on one scrolling page, each under its heading and in a
  // card; the section list scrolls to a section and follows the scrolling (design D6 of
  // redesign-visual-identity). Every change applies at once.
  internal sealed partial class SettingsWindow {
    private readonly IReadOnlyList<SettingsSection> _sections;
    private readonly List<FrameworkElement> _headings = [];

    // Set while the list follows the scrolling, so the selection does not scroll back.
    private bool _following;

    public SettingsWindow(IReadOnlyList<SettingsSection> sections) {
      _sections = sections;
      SystemThemeWatcher.Watch(this);
      InitializeComponent();
      Sections.ItemsSource = sections;
      foreach (SettingsSection section in sections) {
        AddSection(section);
      }

      OuterWheelScroll.Attach(Scroller);
      Sections.SelectionChanged += (_, _) => {
        if (!_following && Sections.SelectedIndex >= 0) {
          ScrollTo(Sections.SelectedIndex);
        }
      };
      Scroller.ScrollChanged += (_, _) => FollowScroll();
      Sections.SelectedIndex = 0;
      Loaded += (_, _) => TitleBarButtons.Name(TitleBar);
    }

    public void Show(Type page) {
      Sections.SelectedItem = _sections.FirstOrDefault(section => section.Page.GetType() == page);
    }

    private void AddSection(SettingsSection section) {
      var heading = new TextBlock {
        Style = (Style)FindResource("HeadingText"),
        Margin = new Thickness(24, 20, 24, 8),
      };
      Localizer.Follow(heading, () => heading.Text =
          Strings.ResourceManager.GetString(section.TitleKey, Strings.Culture));
      var card = new Border {
        Child = section.Page,
        Margin = new Thickness(16, 0, 24, 0),
        CornerRadius = new CornerRadius(8),
        BorderThickness = new Thickness(1),
      };
      card.SetResourceReference(
          Border.BackgroundProperty, "CardBackgroundFillColorDefaultBrush");
      card.SetResourceReference(Border.BorderBrushProperty, "CardStrokeColorDefaultBrush");
      _headings.Add(heading);
      PageStack.Children.Add(heading);
      PageStack.Children.Add(card);
    }

    private double TopOf(int index) {
      return _headings[index].TranslatePoint(new Point(0, 0), PageStack).Y;
    }

    private void ScrollTo(int index) {
      Scroller.UpdateLayout();
      Scroller.ScrollToVerticalOffset(TopOf(index));
    }

    private void FollowScroll() {
      double[] tops = [.. Enumerable.Range(0, _headings.Count).Select(TopOf)];
      int inView = SectionScroll.InView(
          tops, Scroller.VerticalOffset, Scroller.ViewportHeight, Scroller.ExtentHeight);
      if (inView != Sections.SelectedIndex) {
        _following = true;
        Sections.SelectedIndex = inView;
        _following = false;
      }
    }
  }
}
