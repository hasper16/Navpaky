using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rekey.App.Tray {
  // The icon Windows shows for a program, for the Excluded Apps list and picker.
  internal static class AppIcons {
    public static ImageSource? Of(string path) {
      if (!File.Exists(path)) {
        return null;
      }

      try {
        using Icon? icon = Icon.ExtractAssociatedIcon(path);
        if (icon is null) {
          return null;
        }

        BitmapSource image = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        image.Freeze();
        return image;
      } catch (Exception e) when (e is Win32Exception or IOException or ArgumentException) {
        return null;
      }
    }
  }
}
