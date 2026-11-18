using Rekey.Core.Languages;
using Rekey.Win32.Layouts;
using Xunit;

namespace Rekey.Win32.Tests.Layouts {
  // Fix 2.1 of add-final-acceptance: the switching order comes from the user's language list.
  public sealed class UserLayoutOrderTests {
    [Fact]
    public void KeyboardsFollowTheLanguagesAndTheirPlaces() {
      var values = new Dictionary<string, IReadOnlyList<(string Name, int? Place)>> {
        ["en-US"] = [("CachedLanguageName", null), ("0409:00000409", 1)],
        ["uk"] = [("0422:00000419", 2), ("0422:00020422", 1), ("FeaturesToInstall", 231)],
      };

      IReadOnlyList<LayoutId> order = UserLayoutOrder.Parse(["en-US", "uk"], tag => values[tag]);

      Assert.Equal(
          [new LayoutId(0x409), new LayoutId(0x20422), new LayoutId(0x419)], order);
    }

    [Fact]
    public void InputMethodsAreLeftOut() {
      const string inputMethod =
          "0804:{81D4E9C9-1D3B-41BC-9E6C-4B40BF79E35E}{FA550B04-5AD7-411F-A5AC-CA038EC515D7}";

      IReadOnlyList<LayoutId> order = UserLayoutOrder.Parse(["zh-CN"], _ => [(inputMethod, 1)]);

      Assert.Empty(order);
    }
  }
}
