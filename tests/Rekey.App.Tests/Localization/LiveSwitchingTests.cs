using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Windows.Controls;
using System.Windows.Data;
using Rekey.App.Localization;
using Xunit;

namespace Rekey.App.Tests.Localization {
  // A language change reaches text already on screen: {l:Loc} bindings and LocalizedChoice
  // entries, such as a combo box's items, update without rebuilding the view.
  public sealed class LiveSwitchingTests {
    [Fact]
    public void BoundTextFollowsTheLanguage() {
      RunOnSta(() => {
        CultureInfo before = Localizer.Instance.Culture;
        try {
          Localizer.Instance.Use(CultureInfo.GetCultureInfo("en"));
          var label = new TextBlock();
          label.SetBinding(TextBlock.TextProperty, LocExtension.Binding("MenuSettings"));
          var choice = new TextBlock { DataContext = new LocalizedChoice(1, "MenuStatistics") };
          choice.SetBinding(TextBlock.TextProperty, ChoiceBinding());
          Assert.Equal("Settings", label.Text);
          Assert.Equal("Statistics", choice.Text);

          Localizer.Instance.Use(CultureInfo.GetCultureInfo("uk"));

          Assert.Equal("Налаштування", label.Text);
          Assert.Equal("Статистика", choice.Text);
        } finally {
          Localizer.Instance.Use(before);
        }
      });
    }

    // The binding App.xaml's LocalizedChoice template uses.
    private static MultiBinding ChoiceBinding() {
      var binding = new MultiBinding { Converter = new KeyTextConverter() };
      binding.Bindings.Add(new Binding(nameof(LocalizedChoice.Key)));
      binding.Bindings.Add(
          new Binding(nameof(Localizer.Culture)) { Source = Localizer.Instance });
      return binding;
    }

    // WPF objects need an STA thread; a failure there is rethrown on the test's thread with its
    // original stack.
    private static void RunOnSta(Action test) {
      ExceptionDispatchInfo? failure = null;
      var thread = new Thread(() => {
        try {
          test();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          failure = ExceptionDispatchInfo.Capture(e);
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
      failure?.Throw();
    }
  }
}
