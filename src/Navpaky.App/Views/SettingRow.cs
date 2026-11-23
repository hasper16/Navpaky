using System.Windows;
using System.Windows.Controls;

namespace Navpaky.App.Views {
  // One row of a settings card, as in the approved prototype: the label (and an optional hint)
  // on the left, the control on the right, a divider below. The look is the SettingRow style in
  // Resources.xaml.
  internal sealed class SettingRow : ContentControl {
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(SettingRow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty HintProperty = DependencyProperty.Register(
        nameof(Hint), typeof(string), typeof(SettingRow), new PropertyMetadata(null));

    public string Label {
      get => (string)GetValue(LabelProperty);
      set => SetValue(LabelProperty, value);
    }

    public string? Hint {
      get => (string?)GetValue(HintProperty);
      set => SetValue(HintProperty, value);
    }
  }
}
