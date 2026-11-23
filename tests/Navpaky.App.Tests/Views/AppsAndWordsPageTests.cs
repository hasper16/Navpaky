using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Navpaky.App.ViewModels;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Owner decision (fix-redesign-findings): words show as chips, as in prototype v24, and the
  // field that adds a word opens from the row's Add button.
  [Collection(nameof(Sta))]
  public sealed class AppsAndWordsPageTests {
    [Theory]
    [InlineData("ProtectedList")]
    [InlineData("LearnedList")]
    public void WordsAreChips(string list) {
      bool chips = false;
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open(page => {
          var items = (ItemsControl)page.FindName(list);
          chips = items.ItemsPanel.LoadContent() is WrapPanel;
        });
      });

      Assert.True(chips);
    }

    [Theory]
    [InlineData("OpenProtected", "ProtectedEditor")]
    [InlineData("OpenLearned", "LearnedEditor")]
    public void AddOpensTheField(string open, string editor) {
      var before = Visibility.Visible;
      var after = Visibility.Collapsed;
      Sta.Run(() => {
        Sta.EnsureApplication();
        Open(page => {
          var field = (FrameworkElement)page.FindName(editor);
          before = field.Visibility;
          ((ButtonBase)page.FindName(open)).RaiseEvent(
              new RoutedEventArgs(ButtonBase.ClickEvent));
          after = field.Visibility;
        });
      });

      Assert.Equal(Visibility.Collapsed, before);
      Assert.Equal(Visibility.Visible, after);
    }

    private static void Open(Action<AppsAndWordsPage> check) {
      using var hub = new SettingsHub(new Settings(), _ => { }, new ManualClock());
      using var apps = new ExcludedAppsViewModel(hub, key => key);
      var words = new WordsViewModel(_ => { }, () => TimeSpan.Zero);
      check(new AppsAndWordsPage(apps, words));
    }
  }
}
