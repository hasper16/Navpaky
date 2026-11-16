using System.Globalization;
using System.Windows.Data;

namespace Rekey.App.Localization {
  // Turns a string key (first value) into its text; the second value, the Localizer's culture,
  // only makes the binding run again when the language changes.
  internal sealed class KeyTextConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
      return values[0] is string key ? Localizer.Instance[key] : string.Empty;
    }

    public object[] ConvertBack(
        object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }
  }
}
