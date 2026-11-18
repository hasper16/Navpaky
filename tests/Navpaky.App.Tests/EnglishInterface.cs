using System.Globalization;
using System.Runtime.CompilerServices;

namespace Navpaky.App.Tests {
  // Tests assert English texts, so they must not follow the test machine's display language.
  internal static class EnglishInterface {
    [ModuleInitializer]
    internal static void Use() {
      Strings.Culture = CultureInfo.GetCultureInfo("en");
    }
  }
}
