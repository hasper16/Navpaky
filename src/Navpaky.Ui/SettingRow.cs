using System.Windows;
using System.Windows.Controls;

namespace Navpaky.Ui {
  /// <summary>
  /// One row of a settings card, as in the approved prototype: the label and an optional hint on
  /// the left, the control on the right, a divider below. Its look is the <c>SettingRow</c> style
  /// in <c>Brand.xaml</c>.
  /// </summary>
  public sealed class SettingRow : ContentControl {
    /// <summary>Identifies the <see cref="Label"/> dependency property.</summary>
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(SettingRow), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Hint"/> dependency property.</summary>
    public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
        nameof(Hint), typeof(string), typeof(SettingRow), new PropertyMetadata(null));

    /// <summary>Gets or sets the row's label.</summary>
    public string Label {
      get => (string)GetValue(LabelProperty);
      set => SetValue(LabelProperty, value);
    }

    /// <summary>Gets or sets the hint under the label; <see langword="null"/> hides it.</summary>
    public string? Hint {
      get => (string?)GetValue(HintProperty);
      set => SetValue(HintProperty, value);
    }
  }
}
