using System.Globalization;
using Rekey.Core;

namespace Rekey.App.Localization {
  // Picks the culture of the interface from the user's choice and the Windows display language.
  internal static class UiCulture {
    public static IReadOnlyList<CultureInfo> All { get; } =
        [.. new[] { "en", "uk" }.Select(CultureInfo.GetCultureInfo)];

    public static CultureInfo For(UiLanguage choice, CultureInfo windows) {
      string name = choice switch {
        UiLanguage.English => "en",
        UiLanguage.Ukrainian => "uk",

        // The interface has no Russian; Russian Windows gets the Ukrainian one (owner decision).
        _ => windows.TwoLetterISOLanguageName is "uk" or "ru" ? "uk" : "en",
      };
      return CultureInfo.GetCultureInfo(name);
    }
  }
}
