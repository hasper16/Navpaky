using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text;
using Rekey.App.Localization;
using Xunit;

namespace Rekey.App.Tests.Localization {
  // Every interface language has every string with the same placeholders (spec: Complete
  // translations); CI runs this, so a missing translation fails the build.
  public sealed class TranslationCompletenessTests {
    [Fact]
    public void EveryLanguageHasEveryStringWithItsPlaceholders() {
      Dictionary<string, Dictionary<string, string>> languages = UiCulture.All.ToDictionary(
          culture => culture.Name, StringsOf);

      Assert.Empty(Problems(languages));
    }

    [Fact]
    public void NamesAMissingKeyAndAPlaceholderMismatch() {
      Dictionary<string, Dictionary<string, string>> languages = new() {
        ["en"] = new() { ["Hello"] = "Hello {0}", ["Bye"] = "Bye" },
        ["uk"] = new() { ["Hello"] = "Привіт" },
      };

      Assert.Equal(
          ["uk: missing Bye", "uk: Hello has 0 placeholders, en has 1"], Problems(languages));
    }

    private static Dictionary<string, string> StringsOf(CultureInfo culture) {
      ResourceSet set = Strings.ResourceManager.GetResourceSet(culture, true, false)
          ?? throw new InvalidOperationException($"No resources for {culture.Name}.");
      return set.Cast<DictionaryEntry>()
          .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    private static List<string> Problems(
        Dictionary<string, Dictionary<string, string>> languages) {
      Dictionary<string, string> english = languages["en"];
      List<string> problems = [];
      foreach ((string name, Dictionary<string, string> strings) in languages) {
        problems.AddRange(english.Keys.Except(strings.Keys).Order()
            .Select(key => $"{name}: missing {key}"));
        problems.AddRange(strings.Keys.Except(english.Keys).Order()
            .Select(key => $"{name}: extra {key}"));
        foreach (string key in english.Keys.Intersect(strings.Keys).Order()) {
          int expected = Placeholders(english[key]);
          int actual = Placeholders(strings[key]);
          if (actual != expected) {
            problems.Add($"{name}: {key} has {actual} placeholders, en has {expected}");
          }
        }
      }

      return problems;
    }

    // The arguments a string needs: one more than its highest placeholder index.
    private static int Placeholders(string text) {
      return CompositeFormat.Parse(text).MinimumArgumentCount;
    }
  }
}
