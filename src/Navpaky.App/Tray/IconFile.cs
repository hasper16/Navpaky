using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Navpaky.App.Tray {
  // Wraps rendered keycaps in an .ico, which Windows accepts with PNG images inside.
  internal static class IconFile {
    private const int _headerSize = 6;
    private const int _entrySize = 16;

    public static Icon From(BitmapSource image) {
      using var ico = new MemoryStream(Bytes([image]));
      return new Icon(ico);
    }

    // One image per size, as an .exe and the installed-apps list pick the size they need.
    public static byte[] Bytes(IReadOnlyList<BitmapSource> images) {
      byte[][] pngs = [.. images.Select(Png)];
      using var ico = new MemoryStream();
      using (var writer = new BinaryWriter(ico, System.Text.Encoding.UTF8, leaveOpen: true)) {
        writer.Write((ushort)0);
        writer.Write((ushort)1);
        writer.Write((ushort)images.Count);
        uint offset = (uint)(_headerSize + (_entrySize * images.Count));
        for (int i = 0; i < images.Count; i++) {
          // A side of 256 is written as 0.
          writer.Write((byte)(images[i].PixelWidth % 256));
          writer.Write((byte)(images[i].PixelHeight % 256));
          writer.Write((byte)0);
          writer.Write((byte)0);
          writer.Write((ushort)1);
          writer.Write((ushort)32);
          writer.Write((uint)pngs[i].Length);
          writer.Write(offset);
          offset += (uint)pngs[i].Length;
        }

        foreach (byte[] png in pngs) {
          writer.Write(png);
        }
      }

      return ico.ToArray();
    }

    private static byte[] Png(BitmapSource image) {
      var encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(image));
      using var png = new MemoryStream();
      encoder.Save(png);
      return png.ToArray();
    }
  }
}
