using System.Globalization;
using Rekey.App.Localization;
using Xunit;

namespace Rekey.App.Tests.Localization {
  public sealed class PluralTests {
    private static readonly CultureInfo _english = new("en");

    public static TheoryData<int, string> Ukrainian => new() {
      { 0, "Many" }, { 1, "One" }, { 2, "Few" },
      { 3, "Few" }, { 4, "Few" }, { 5, "Many" },
      { 6, "Many" }, { 7, "Many" }, { 8, "Many" },
      { 9, "Many" }, { 10, "Many" }, { 11, "Many" },
      { 12, "Many" }, { 13, "Many" }, { 14, "Many" },
      { 15, "Many" }, { 16, "Many" }, { 17, "Many" },
      { 18, "Many" }, { 19, "Many" }, { 20, "Many" },
      { 21, "One" }, { 22, "Few" }, { 23, "Few" },
      { 24, "Few" }, { 25, "Many" }, { 101, "One" },
      { 111, "Many" }, { 121, "One" },
    };

    public static TheoryData<int> Counts =>
        [.. Enumerable.Range(0, 26).Append(101).Append(111).Append(121)];

    [Theory]
    [MemberData(nameof(Ukrainian))]
    public void UkrainianFollowsCldr(int count, string expected) {
      Assert.Equal(expected, Plural.Select(new CultureInfo("uk-UA"), count).ToString());
    }

    [Theory]
    [MemberData(nameof(Counts))]
    public void EnglishHasOneOnlyForOne(int count) {
      string expected = count == 1 ? "One" : "Other";

      Assert.Equal(expected, Plural.Select(_english, count).ToString());
    }

    [Theory]
    [InlineData("uk", 1, "Navpaky на паузі: залишилася 1 хвилина")]
    [InlineData("uk", 2, "Navpaky на паузі: залишилося 2 хвилини")]
    [InlineData("uk", 5, "Navpaky на паузі: залишилося 5 хвилин")]
    [InlineData("en", 1, "Navpaky is paused: 1 minute left")]
    [InlineData("en", 5, "Navpaky is paused: 5 minutes left")]
    public void PausedToolTipUsesThePluralForm(string language, int minutes, string expected) {
      string text = UiText.Plural(new CultureInfo(language), "TrayPausedMinutes", minutes);

      Assert.Equal(expected, text);
    }
  }
}
