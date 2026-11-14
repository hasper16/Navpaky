using System.Globalization;
using System.Windows.Data;

namespace Rekey.App.Views {
  // Fills a UI string's placeholder with the bound value, as for "Remove Telegram".
  internal sealed class FormatConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return UiText.Format((string)parameter, value);
    }

    public object ConvertBack(
        object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }
  }
}
