using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;
using Xunit;

namespace Navpaky.App.Tests.Tray {
  [Collection(nameof(Sta))]
  public sealed class KeycapTests {
    private static readonly int[] _sizes = [16, 20, 24, 32];

    [Theory]
    [InlineData(16)]
    [InlineData(20)]
    [InlineData(24)]
    [InlineData(32)]
    public void RendersAtTheAskedSize(int size) {
      BitmapSource image = OnSta(() => Keycap.Render("EN", TrayState.Active, dark: false, size));

      Assert.Equal(size, image.PixelWidth);
      Assert.Equal(size, image.PixelHeight);
    }

    [Fact]
    public void LegendChangesThePicture() {
      byte[] english = OnSta(() => Pixels(Keycap.Render("EN", TrayState.Active, false, 32)));
      byte[] russian = OnSta(() => Pixels(Keycap.Render("RU", TrayState.Active, false, 32)));

      Assert.NotEqual(english, russian);
    }

    [Fact]
    public void EveryStateLooksDifferent() {
      TrayState[] states = Enum.GetValues<TrayState>();

      byte[][] pictures = OnSta(() => states
          .Select(state => Pixels(Keycap.Render("EN", state, false, 32)))
          .ToArray());

      Assert.Equal(states.Length, pictures.Select(Convert.ToBase64String).Distinct().Count());
    }

    [Fact]
    public void ActiveLipIsRedOnALightTaskbar() {
      byte[] pixels = OnSta(() => Pixels(Keycap.Render("EN", TrayState.Active, false, 32)));

      Assert.True(Contains(pixels, Color.FromRgb(0xC8, 0x10, 0x2E)));
    }

    [Fact]
    public void ActiveLipIsGoldOnADarkTaskbar() {
      byte[] pixels = OnSta(() => Pixels(Keycap.Render("EN", TrayState.Active, true, 32)));

      Assert.True(Contains(pixels, Color.FromRgb(0xFD, 0xE6, 0x8A)));
    }

    [Fact]
    public void PausedLipIsAmber() {
      byte[] pixels = OnSta(() => Pixels(Keycap.Render("EN", TrayState.Paused, true, 32)));

      Assert.True(Contains(pixels, Color.FromRgb(0xFD, 0xE0, 0x47)));
    }

    // Writes every variant at 100 %, 125 %, 150 % and 200 % for a look by hand
    // (docs/manual-tests/deferred-by-hand.md).
    [Fact]
    public void WritesTheVariantSheet() {
      string path = Path.Join(AppContext.BaseDirectory, "keycap-sheet.png");

      OnSta(() => {
        Keycap.WriteSheet(path, _sizes);
        return 0;
      });

      Assert.True(new FileInfo(path).Length > 0);
    }

    private static byte[] Pixels(BitmapSource image) {
      var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);
      byte[] pixels = new byte[converted.PixelWidth * converted.PixelHeight * 4];
      converted.CopyPixels(pixels, converted.PixelWidth * 4, 0);
      return pixels;
    }

    private static bool Contains(byte[] bgra, Color color) {
      for (int i = 0; i < bgra.Length; i += 4) {
        if (bgra[i + 3] == 255 && bgra[i] == color.B && bgra[i + 1] == color.G
            && bgra[i + 2] == color.R) {
          return true;
        }
      }

      return false;
    }

    private static T OnSta<T>(Func<T> work) {
      T result = default!;
      Exception? error = null;
      var thread = new Thread(() => {
        try {
          result = work();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          error = e;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
      return error is null ? result : throw new InvalidOperationException("STA work failed", error);
    }
  }
}
