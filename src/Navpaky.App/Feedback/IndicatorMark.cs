using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Navpaky.App.Views;
using Navpaky.Core;
using Navpaky.Ui;

namespace Navpaky.App.Feedback {
  // The mark the cursor indicator shows, in the style the user chose (design D7): a translucent
  // pill with an accent dot, a solid accent tag, or underlined accent letters. Settings show the
  // same marks as previews.
  internal static class IndicatorMark {
    private static readonly FontFamily _font = new("Segoe UI Variable Display, Segoe UI");

    public static FrameworkElement Build(CursorIndicatorStyle style, string code, bool dark) {
      Color accent = BrandTheme.Accent(dark);
      var text = new TextBlock {
        Text = code,
        FontFamily = _font,
        FontWeight = FontWeights.SemiBold,
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center,
      };
      return style switch {
        CursorIndicatorStyle.Tag => Tag(text, accent),
        CursorIndicatorStyle.Underline => Underline(text, accent),
        _ => Pill(text, accent, dark),
      };
    }

    private static Border Pill(TextBlock text, Color accent, bool dark) {
      text.Foreground = Frozen(
          dark ? Color.FromRgb(0xE8, 0xEC, 0xF6) : Color.FromRgb(0x11, 0x17, 0x26));
      var dot = new Ellipse {
        Width = 7,
        Height = 7,
        Fill = Frozen(accent),
        Margin = new Thickness(0, 0, 5, 0),
        VerticalAlignment = VerticalAlignment.Center,
      };
      var row = new StackPanel { Orientation = Orientation.Horizontal };
      row.Children.Add(dot);
      row.Children.Add(text);
      return new Border {
        Child = row,
        Padding = new Thickness(6, 3, 8, 3),
        CornerRadius = new CornerRadius(999),
        Background = Frozen(dark
            ? Color.FromArgb(0xD9, 0x13, 0x1B, 0x2F)
            : Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
        BorderBrush = Frozen(dark
            ? Color.FromArgb(0x30, 0x96, 0xAA, 0xE6)
            : Color.FromArgb(0x1F, 0x12, 0x28, 0x3A)),
        BorderThickness = new Thickness(1),
        Margin = new Thickness(6),
        Effect = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.18 },
      };
    }

    private static Border Tag(TextBlock text, Color accent) {
      text.Foreground = Brushes.White;
      return new Border {
        Child = text,
        Padding = new Thickness(7, 3, 7, 3),
        CornerRadius = new CornerRadius(5),
        Background = Frozen(accent),
        Margin = new Thickness(6),
      };
    }

    private static Border Underline(TextBlock text, Color accent) {
      text.Foreground = Frozen(accent);
      text.FontSize = 12;
      return new Border {
        Child = text,
        Padding = new Thickness(0, 0, 0, 2),
        BorderBrush = Frozen(accent),
        BorderThickness = new Thickness(0, 0, 0, 2),
        Margin = new Thickness(6),
      };
    }

    private static SolidColorBrush Frozen(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }
  }
}
