using System.Globalization;
using Microsoft.Win32;
using Navpaky.Core.Languages;

namespace Navpaky.Win32.Layouts {
  /// <summary>
  /// Reads the order in which Windows switches Layouts: the user's language list.
  /// </summary>
  /// <remarks>
  /// <c>HKCU\Control Panel\International\User Profile</c> lists the languages in
  /// <c>Languages</c>; each language's key names its keyboards as <c>LLLL:KLID</c> values whose
  /// number is their place within the language. Input methods carry a GUID instead of a KLID
  /// and are left out.
  /// </remarks>
  internal static class UserLayoutOrder {
    private const string _userProfile = @"Control Panel\International\User Profile";

    /// <summary>Reads the user's Layouts in switching order; empty when unreadable.</summary>
    public static IReadOnlyList<LayoutId> Read() {
      using RegistryKey? profile = Registry.CurrentUser.OpenSubKey(_userProfile);
      if (profile?.GetValue("Languages") is not string[] languages) {
        return [];
      }

      return Parse(languages, language => {
        using RegistryKey? key = profile.OpenSubKey(language);
        return key is null
            ? []
            : [.. key.GetValueNames().Select(name => (name, key.GetValue(name) as int?))];
      });
    }

    /// <summary>Puts the keyboards of each language, in list order, into one order.</summary>
    /// <param name="languages">The language tags in the user's order.</param>
    /// <param name="valuesOf">The value names and numbers under a language's key.</param>
    public static IReadOnlyList<LayoutId> Parse(
        IEnumerable<string> languages,
        Func<string, IReadOnlyList<(string Name, int? Place)>> valuesOf) {
      return [.. languages
          .SelectMany(language => valuesOf(language)
              .Where(value => value.Place is not null)
              .OrderBy(value => value.Place)
              .Select(value => KlidOf(value.Name))
              .OfType<LayoutId>())
          .Distinct()];
    }

    private static LayoutId? KlidOf(string name) {
      int colon = name.IndexOf(':', StringComparison.Ordinal);
      string klid = colon < 0 ? string.Empty : name[(colon + 1)..];
      if (klid.Length != 8 || !uint.TryParse(
          klid, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint value)) {
        return null;
      }

      return new LayoutId(value);
    }
  }
}
