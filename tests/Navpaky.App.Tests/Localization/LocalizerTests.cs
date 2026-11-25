using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Navpaky.Ui;
using Xunit;

namespace Navpaky.App.Tests.Localization {
  public sealed class LocalizerTests {
    private readonly Localizer _localizer = new(CultureInfo.GetCultureInfo("en"));

    public LocalizerTests() {
      _localizer.AddStrings(Strings.ResourceManager);
    }

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

    [Fact]
    public void AKeyNoStringsHaveIsAnError() {
      var empty = new Localizer(CultureInfo.GetCultureInfo("en"));

      Assert.Throws<KeyNotFoundException>(() => empty["MenuSettings"]);
    }

    [Fact]
    public void ServesStringsFromEveryExecutableThatAddedThem() {
      _localizer.AddStrings(new OneString("WizardOnly", "Install"));

      Assert.Equal("Settings", _localizer["MenuSettings"]);
      Assert.Equal("Install", _localizer["WizardOnly"]);
    }

    [Fact]
    public void AddingTheSameStringsTwiceKeepsOneSource() {
      var source = new OneString("WizardOnly", "Install");

      _localizer.AddStrings(source);
      _localizer.AddStrings(source);

      // A key no source has asks every source once.
      Assert.Equal(1, source.MissesFor("NotInAnyStrings", _localizer));
    }

    [Fact]
    public void TheAppAddsItsStringsToTheSharedInstance() {
      Assert.Equal(
          Strings.ResourceManager.GetString("MenuSettings", Localizer.Instance.Culture),
          Localizer.Instance["MenuSettings"]);
    }

    // A second executable's strings: one key, counting how often it is asked.
    private sealed class OneString(string key, string text) : ResourceManager {
      private int _asked;

      public int MissesFor(string probe, Localizer localizer) {
        _asked = 0;
        Assert.Throws<KeyNotFoundException>(() => localizer[probe]);
        return _asked;
      }

      public override string? GetString(string name, CultureInfo? culture) {
        _asked++;
        return name == key ? text : null;
      }
    }
  }
}
