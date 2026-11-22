using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Navpaky.App.Views;
using Xunit;

namespace Navpaky.App.Tests.Views {
  // Owner's check of the 0.4.5 pack (fix-redesign-findings): sound names are names only, and
  // About carries no font-licence line.
  [Collection(nameof(Sta))]
  public sealed class OwnerTextsTests {
    [Theory]
    [InlineData("en")]
    [InlineData("uk")]
    public void SoundNamesHaveNoExplanation(string language) {
      CultureInfo culture = CultureInfo.GetCultureInfo(language);
      string[] keys = [
          "SoundKeyClick", "SoundSoftTick", "SoundChime", "SoundBandura", "SoundWoodenSpoons",
          "SoundDrop", "SoundGlass", "SoundCymbals", "SoundSopilka", "SoundJawHarp",
          "SoundFrameDrum", "SoundNavpaky"];

      foreach (string key in keys) {
        string name = Strings.ResourceManager.GetString(key, culture)!;
        Assert.DoesNotContain("(", name, StringComparison.Ordinal);
      }
    }

    [Fact]
    public void AboutHasNoFontLicenceLine() {
      List<string> texts = [];
      Sta.Run(() => {
        var about = new AboutPage();
        Collect(about, texts);
      });

      Assert.DoesNotContain(
          texts, text => text.Contains("Open Font License", StringComparison.Ordinal));
    }

    private static void Collect(DependencyObject node, List<string> texts) {
      if (node is TextBlock block) {
        texts.Add(block.Text);
      }

      foreach (object child in LogicalTreeHelper.GetChildren(node)) {
        if (child is DependencyObject element) {
          Collect(element, texts);
        }
      }
    }
  }
}
