using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Navpaky.App.Views {
  // The Navpaky mark for the About section and the welcome window, built from the icon masters
  // by docs/design/icons/tools/build_icons.py (design D2).
  internal static class BrandMark {
    public static ImageSource Large() {
      using Stream mark = typeof(BrandMark).Assembly
          .GetManifestResourceStream("Navpaky.Assets.mark-128.png")!;
      BitmapFrame image = BitmapFrame.Create(
          mark, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
      image.Freeze();
      return image;
    }
  }
}
