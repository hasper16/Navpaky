using System.Globalization;

namespace Navpaky.App.Localization {
  // CLDR plural rules for whole numbers in the interface languages (en, uk).
  internal static class Plural {
    public static PluralCategory Select(CultureInfo culture, long count) {
      if (culture.TwoLetterISOLanguageName != "uk") {
        return count == 1 ? PluralCategory.One : PluralCategory.Other;
      }

      long ones = Math.Abs(count) % 10;
      long tens = Math.Abs(count) % 100;
      if (ones == 1 && tens != 11) {
        return PluralCategory.One;
      }

      return ones is >= 2 and <= 4 && tens is < 12 or > 14
          ? PluralCategory.Few
          : PluralCategory.Many;
    }
  }
}
