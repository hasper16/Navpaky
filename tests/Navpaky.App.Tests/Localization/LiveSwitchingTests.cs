using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Navpaky.App.Localization;
using Xunit;

namespace Navpaky.App.Tests.Localization {
  // A language change reaches text already on screen: {l:Loc} bindings and LocalizedChoice
  // entries, such as a combo box's items, update without rebuilding the view.
  [Collection(nameof(Sta))]
  public sealed class LiveSwitchingTests {
    [Fact]
    public void BoundTextFollowsTheLanguage() {
      Sta.Run(() => {
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

    // Fix 2.10 of add-final-acceptance: screen readers pick their voice from an element's
    // language, which stayed en-US under a Ukrainian interface, so Narrator skipped Cyrillic.
    [Fact]
    public void BoundLanguageFollowsTheInterfaceLanguage() {
      Sta.Run(() => {
        CultureInfo before = Localizer.Instance.Culture;
        try {
          Localizer.Instance.Use(CultureInfo.GetCultureInfo("en"));
          var view = new TextBlock();
          Localizer.BindLanguage(view);
          Assert.Equal("en", view.Language.IetfLanguageTag);

          Localizer.Instance.Use(CultureInfo.GetCultureInfo("uk"));

          Assert.Equal("uk", view.Language.IetfLanguageTag);
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
  }
}
