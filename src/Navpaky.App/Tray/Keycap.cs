using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Navpaky.App.ViewModels;

namespace Navpaky.App.Tray {
  // The tray keycap: the icon set's 3D key (rim, skirt, top face, front lip) with the Language
  // code on it (design D1 of redesign-visual-identity). Shapes are drawn on a 32-unit grid
  // scaled to the asked size; the legend is drawn at the real size so it stays crisp.
  internal static class Keycap {
    private const double _grid = 32;
    private static readonly Color _failedDot = Rgb(0xE11D48);
    private static readonly Color _darkRing = Rgb(0x22181A);
    private static readonly Typeface _legendFace = new(
        new FontFamily("Segoe UI Variable Display, Segoe UI"),
        FontStyles.Normal,
        FontWeights.SemiBold,
        FontStretches.Normal);

    public static BitmapSource Render(string code, TrayState state, bool dark, int size) {
      var visual = new DrawingVisual();
      using (DrawingContext context = visual.RenderOpen()) {
        Draw(context, code, state, dark, size, new Point(0, 0));
      }

      var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
      bitmap.Render(visual);
      bitmap.Freeze();
      return bitmap;
    }

    // Every state in both themes, one column per size, on a background like the taskbar's.
    public static void WriteSheet(string path, IReadOnlyList<int> sizes) {
      const int gap = 8;
      TrayState[] states = Enum.GetValues<TrayState>();
      int cell = sizes.Max() + gap;
      int width = (cell * sizes.Count) + gap;
      int height = (cell * states.Length * 2) + gap;
      var visual = new DrawingVisual();
      using (DrawingContext context = visual.RenderOpen()) {
        int row = 0;
        foreach (bool dark in new[] { false, true }) {
          foreach (TrayState state in states) {
            double top = gap + (row * (double)cell);
            var band = new Rect(0, top - (gap / 2.0), width, cell);
            context.DrawRectangle(
                new SolidColorBrush(dark ? Color.FromRgb(0x20, 0x20, 0x20) : Colors.White),
                null,
                band);
            for (int column = 0; column < sizes.Count; column++) {
              var at = new Point(gap + (column * (double)cell), top);
              Draw(context, "EN", state, dark, sizes[column], at);
            }

            row++;
          }
        }
      }

      var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
      bitmap.Render(visual);
      var encoder = new PngBitmapEncoder();
      encoder.Frames.Add(BitmapFrame.Create(bitmap));
      using FileStream file = File.Create(path);
      encoder.Save(file);
    }

    private static void Draw(
        DrawingContext context, string code, TrayState state, bool dark, int size, Point at) {
      Look look = Look.For(state, dark);
      double scale = size / _grid;
      context.PushTransform(new TranslateTransform(at.X, at.Y));
      context.PushTransform(new ScaleTransform(scale, scale));

      var body = new Rect(0.4, 0.4, _grid - 0.8, _grid - 0.8);
      context.DrawRoundedRectangle(
          Vertical(look.EdgeTop, look.EdgeBottom),
          new Pen(Brush(look.Rim, look.RimOpacity), 0.8),
          body,
          6.72,
          6.72);

      // The bottom facet and the corner creases make the key read as seen from slightly above.
      var facet = new StreamGeometry();
      using (StreamGeometryContext path = facet.Open()) {
        path.BeginFigure(new Point(2, _grid - 0.5), true, true);
        path.LineTo(new Point(3, _grid - 5), false, false);
        path.LineTo(new Point(_grid - 3, _grid - 5), false, false);
        path.LineTo(new Point(_grid - 2, _grid - 0.5), false, false);
      }

      context.DrawGeometry(Brush(look.Facet, 0.65), null, facet);
      var crease = new Pen(Brush(look.Chamfer, 0.75), 1.28);
      context.DrawLine(crease, new Point(1.4, _grid - 1.4), new Point(3.2, _grid - 5.5));
      context.DrawLine(
          crease, new Point(_grid - 1.4, _grid - 1.4), new Point(_grid - 3.2, _grid - 5.5));

      var face = new Rect(2, 2, _grid - 4, _grid - 7);
      context.DrawRoundedRectangle(Vertical(look.FaceTop, look.FaceBottom), null, face, 4.84, 4.84);
      var catchLight = new Pen(Brush(look.CatchLight, 0.8), 0.9) {
        StartLineCap = PenLineCap.Round,
        EndLineCap = PenLineCap.Round,
      };
      context.DrawLine(catchLight, new Point(5.9, 2.5), new Point(_grid - 5.9, 2.5));
      context.DrawRoundedRectangle(
          Brush(look.Lip), null, new Rect((_grid / 2) - 6, _grid - 3.5, 12, 2), 1, 1);

      if (state == TrayState.SelfPaused) {
        double dot = Math.Max(4.5, 2.3 * _grid / size);
        context.DrawEllipse(
            Brush(_failedDot),
            new Pen(Brush(dark ? _darkRing : Colors.White), 1.2 * _grid / size),
            new Point(_grid - dot, dot),
            dot,
            dot);
      }

      context.Pop();

      // The legend in real pixels, centred on the face.
      var legendArea = new Rect(face.X * scale, face.Y * scale, face.Width * scale,
          face.Height * scale);
      var text = new FormattedText(
          code,
          CultureInfo.InvariantCulture,
          FlowDirection.LeftToRight,
          _legendFace,
          LegendSize(code, legendArea.Width),
          Brush(look.Legend, state == TrayState.Off ? 0.45 : 1),
          1.0);
      context.DrawText(
          text,
          new Point(
              legendArea.X + ((legendArea.Width - text.Width) / 2),
              legendArea.Y + ((legendArea.Height - text.Height) / 2)));
      context.Pop();
    }

    // Two letters fill most of the face; a longer code shrinks to fit.
    private static double LegendSize(string code, double faceWidth) {
      return faceWidth * (code.Length <= 2 ? 0.52 : 1.05 / code.Length);
    }

    private static LinearGradientBrush Vertical(Color top, Color bottom) {
      var brush = new LinearGradientBrush(top, bottom, 90);
      brush.Freeze();
      return brush;
    }

    private static SolidColorBrush Brush(Color color, double opacity = 1) {
      var brush = new SolidColorBrush(color) { Opacity = opacity };
      brush.Freeze();
      return brush;
    }

    private static Color Rgb(int rgb) {
      return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    // One state on one taskbar theme; colours from the icon set, active on a light taskbar
    // recoloured red and black (owner decision).
    private sealed record Look(
        Color EdgeTop,
        Color EdgeBottom,
        Color Rim,
        double RimOpacity,
        Color Facet,
        Color Chamfer,
        Color FaceTop,
        Color FaceBottom,
        Color CatchLight,
        Color Lip,
        Color Legend) {
      public static Look For(TrayState state, bool dark) {
        return (state, dark) switch {
          (TrayState.Active, false) => Make(0x3A3A40, 0x17171C, 0x17171C, 0.9, 0x0B0B0E, 0x000000,
              0xFFFFFF, 0xF1F1F3, 0xFFFFFF, 0xC8102E, 0x17171C),
          (TrayState.Active, true) => Make(0x2D578F, 0x0D2042, 0xFACC15, 0.85, 0x091630, 0x061024,
              0x3B68A4, 0x1F3F6D, 0x7BA5D9, 0xFDE68A, 0xFFFBEB),
          (TrayState.Paused, false) => Make(0xFBBF24, 0xB45309, 0x92400E, 0.9, 0x92400E, 0x78350F,
              0xFFFDF7, 0xFEF3C7, 0xFFFFFF, 0xFDE68A, 0x1F2328),
          (TrayState.Paused, true) => Make(0xF59E0B, 0x92400E, 0xFDE68A, 0.9, 0x78350F, 0x451A03,
              0x38322C, 0x26221E, 0xFBBF24, 0xFDE047, 0xFEF3C7),
          (TrayState.Off, false) => Make(0xD5DBE3, 0xB8C1CE, 0x8C96A6, 0.8, 0x9FAAB9, 0x7D8797,
              0xF4F6F9, 0xE9EDF2, 0xFFFFFF, 0x737D8C, 0x1F2328),
          (TrayState.Off, true) => Make(0x434852, 0x2D3139, 0x585E6B, 0.75, 0x22252B, 0x16181D,
              0x2A2E35, 0x22252B, 0x3D434E, 0x68707E, 0xF2F4F7),
          (_, false) => Make(0xEF4444, 0xB91C1C, 0x881337, 0.9, 0x991B1B, 0x7F1D1D,
              0xFFFBFB, 0xFEE2E2, 0xFFFFFF, 0xFECACA, 0x1F2328),
          _ => Make(0xEF4444, 0x991B1B, 0xFCA5A5, 0.9, 0x7F1D1D, 0x450A0A,
              0x3A282B, 0x281C1E, 0xF87171, 0xFCA5A5, 0xFEE2E2),
        };
      }

      private static Look Make(
          int edgeTop, int edgeBottom, int rim, double rimOpacity, int facet, int chamfer,
          int faceTop, int faceBottom, int catchLight, int lip, int legend) {
        return new Look(Rgb(edgeTop), Rgb(edgeBottom), Rgb(rim), rimOpacity, Rgb(facet),
            Rgb(chamfer), Rgb(faceTop), Rgb(faceBottom), Rgb(catchLight), Rgb(lip),
            Rgb(legend));
      }
    }
  }
}
