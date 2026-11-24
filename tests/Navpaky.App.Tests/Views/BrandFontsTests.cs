using System.Windows;
using System.Windows.Controls;
using Navpaky.App.Views;
using Navpaky.Ui;
using Xunit;

namespace Navpaky.App.Tests.Views {
  [Collection(nameof(Sta))]
  public sealed class BrandFontsTests {
    [Fact]
    public void TextInAWindowUsesInter() {
      string? font = null;
      Sta.Run(() => {
        BrandFonts.Apply();
        var text = new TextBlock { Text = "Navpaky" };
        var window = new Window {
          Content = text,
          Left = -32000,
          Top = -32000,
          ShowActivated = false,
          ShowInTaskbar = false,
        };
        window.Show();
        font = text.FontFamily.Source;
        window.Close();
      });

      Assert.Contains("#Inter", font, StringComparison.Ordinal);
    }

    [Fact]
    public void BothBrandFontsLoad() {
      bool loaded = false;
      Sta.Run(() => loaded = BrandFonts.Check());

      Assert.True(loaded);
    }
  }
}
