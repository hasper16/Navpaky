using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Navpaky.App.Feedback;
using Navpaky.Core;
using Xunit;

namespace Navpaky.App.Tests.Feedback {
  [Collection(nameof(Sta))]
  public sealed class IndicatorMarkTests {
    [Fact]
    public void PillShowsTheCodeWithAnAccentDot() {
      (string? text, bool dot, Brush? background) = Look(CursorIndicatorStyle.Pill, dark: false);

      Assert.Equal("UK", text);
      Assert.True(dot);
      Assert.NotNull(background);
    }

    [Fact]
    public void TagIsFilledWithTheAccent() {
      (_, bool dot, Brush? background) = Look(CursorIndicatorStyle.Tag, dark: true);

      Assert.False(dot);
      Assert.Equal(Color.FromRgb(0x4C, 0x86, 0xF0), ((SolidColorBrush)background!).Color);
    }

    [Fact]
    public void UnderlineHasNoBackground() {
      (string? text, bool dot, Brush? background) =
          Look(CursorIndicatorStyle.Underline, dark: false);

      Assert.Equal("UK", text);
      Assert.False(dot);
      Assert.Null(background);
    }

    private static (string? Text, bool Dot, Brush? Background) Look(
        CursorIndicatorStyle style, bool dark) {
      (string?, bool, Brush?) result = default;
      Sta.Run(() => {
        var mark = (Border)IndicatorMark.Build(style, "UK", dark);
        result = (
            Find<TextBlock>(mark)?.Text,
            Find<Ellipse>(mark) is not null,
            mark.Background);
      });
      return result;
    }

    private static T? Find<T>(DependencyObject root)
        where T : DependencyObject {
      if (root is T found) {
        return found;
      }

      foreach (object child in LogicalTreeHelper.GetChildren(root)) {
        if (child is DependencyObject node && Find<T>(node) is { } hit) {
          return hit;
        }
      }

      return null;
    }
  }
}
