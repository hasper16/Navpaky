using System.Windows.Media.Imaging;
using Navpaky.App.Views;
using Navpaky.Ui;
using Xunit;

namespace Navpaky.App.Tests.Views {
  [Collection(nameof(Sta))]
  public sealed class BrandMarkTests {
    [Fact]
    public void LoadsTheMarkAtWindowSize() {
      int width = 0;
      Sta.Run(() => width = ((BitmapSource)BrandMark.Large()).PixelWidth);

      Assert.Equal(128, width);
    }
  }
}
