using System.Globalization;
using System.Text;
using Rekey.App.Localization;

namespace Rekey.App {
  // Fills the placeholders of a UI string from Strings.resx in the user's culture.
  internal static class UiText {
    public static string Format<T>(string format, T argument) {
      // Parsed per call: the format depends on the UI language, which may change.
      return string.Format(CultureInfo.CurrentCulture, CompositeFormat.Parse(format), argument);
    }

    // Fills the variant of key whose plural category in culture matches count, such as
    // TrayPausedMinutes_Few.
    public static string Plural(CultureInfo culture, string key, int count) {
      string variant = $"{key}_{Localization.Plural.Select(culture, count)}";
      string format = Strings.ResourceManager.GetString(variant, culture)
          ?? throw new KeyNotFoundException(variant);
      return Format(format, count);
    }
  }
}
