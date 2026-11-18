using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Navpaky.App.Views {
  // Binds a radio button to one value of an enum property: checked when the property has the
  // value, and checking it sets the value.
  internal sealed class EqualsConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return Equals(value, parameter);
    }

    public object ConvertBack(
        object value, Type targetType, object parameter, CultureInfo culture) {
      return value is true ? parameter : DependencyProperty.UnsetValue;
    }
  }
}
