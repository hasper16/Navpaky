using System.ComponentModel;
using System.Globalization;
using Rekey.App.Localization;
using Xunit;

namespace Rekey.App.Tests.Localization {
  public sealed class LocalizerTests {
    private readonly Localizer _localizer = new(CultureInfo.GetCultureInfo("en"));

    [Fact]
    public void ReadsTheStringInItsLanguage() {
      Assert.Equal("Settings", _localizer["MenuSettings"]);
    }

    [Fact]
    public void ChangingTheLanguageRefreshesEveryBinding() {
      List<string?> changed = [];
      _localizer.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

      _localizer.Use(CultureInfo.GetCultureInfo("uk"));

      Assert.Equal("Налаштування", _localizer["MenuSettings"]);
      Assert.Equal(["Item[]", nameof(Localizer.Culture)], changed);
    }

    [Fact]
    public void TheSameLanguageChangesNothing() {
      List<PropertyChangedEventArgs> changed = [];
      _localizer.PropertyChanged += (_, e) => changed.Add(e);

      _localizer.Use(CultureInfo.GetCultureInfo("en"));

      Assert.Empty(changed);
    }
  }
}
