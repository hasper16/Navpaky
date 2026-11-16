using System.Globalization;
using Rekey.App.Localization;
using Rekey.Core;
using Xunit;

namespace Rekey.App.Tests.Localization {
  public sealed class UiCultureTests {
    [Theory]
    [InlineData("ru-RU", "uk")]
    [InlineData("uk-UA", "uk")]
    [InlineData("en-GB", "en")]
    [InlineData("de-DE", "en")]
    [InlineData("", "en")]
    public void SystemIsUkrainianOnUkrainianOrRussianWindows(string windows, string expected) {
      CultureInfo culture = UiCulture.For(UiLanguage.System, new CultureInfo(windows));

      Assert.Equal(expected, culture.Name);
    }

    [Theory]
    [InlineData(UiLanguage.English, "en")]
    [InlineData(UiLanguage.Ukrainian, "uk")]
    public void ChoiceOverridesWindows(UiLanguage choice, string expected) {
      Assert.Equal(expected, UiCulture.For(choice, new CultureInfo("de-DE")).Name);
    }

    [Fact]
    public void SettingsFollowWindowsByDefault() {
      Assert.Equal(UiLanguage.System, new Settings().UiLanguage);
    }
  }
}
