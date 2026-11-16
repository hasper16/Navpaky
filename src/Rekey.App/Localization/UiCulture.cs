using System.Globalization;
using Rekey.Core;

namespace Rekey.App.Localization {
  // Picks the culture of the interface from the user's choice and the Windows display language.
  internal static class UiCulture {
    public static CultureInfo For(UiLanguage choice, CultureInfo windows) {
      string name = choice switch {
        UiLanguage.English => "en",
        UiLanguage.Russian => "ru",
        UiLanguage.Ukrainian => "uk",
        _ => windows.TwoLetterISOLanguageName is "ru" or "uk"
            ? windows.TwoLetterISOLanguageName
            : "en",
      };
      return CultureInfo.GetCultureInfo(name);
    }
  }
}
