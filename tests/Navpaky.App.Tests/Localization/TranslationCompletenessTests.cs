using System.Collections;
using System.Globalization;
using System.Resources;
using Navpaky.App.Localization;
using Navpaky.Ui;
using Xunit;

namespace Navpaky.App.Tests.Localization {
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
        ["en"] = new() { ["Hello"] = "Hello {0}", ["Bye"] = "Bye", ["Pair"] = "{0} and {1}" },
        ["uk"] = new() { ["Hello"] = "Привіт", ["Pair"] = "{1} і {1}" },
      };

      string[] expected = [
          "uk: missing Bye",
          "uk: Hello has placeholders [], en has [0]",
          "uk: Pair has placeholders [1], en has [0, 1]"];
      Assert.Equal(expected, Problems(languages));
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
          string expected = Placeholders(english[key]);
          string actual = Placeholders(strings[key]);
          if (actual != expected) {
            problems.Add($"{name}: {key} has placeholders {actual}, en has {expected}");
          }
        }
      }

      return problems;
    }

    // The placeholder indices a string uses, such as "[0, 1]"; "{{" is an escaped brace.
    private static string Placeholders(string text) {
      SortedSet<int> indices = [];
      int i = 0;
      while (i < text.Length) {
        if (text[i] != '{') {
          i++;
        } else if (i + 1 < text.Length && text[i + 1] == '{') {
          i += 2;
        } else {
          int end = i + 1;
          while (end < text.Length && char.IsAsciiDigit(text[end])) {
            end++;
          }

          indices.Add(int.Parse(text.AsSpan(i + 1, end - i - 1), CultureInfo.InvariantCulture));
          i = end;
        }
      }

      return $"[{string.Join(", ", indices)}]";
    }
  }
}
