using System.Globalization;
using System.Windows.Data;
using Rekey.App.Localization;

namespace Rekey.App.Views {
  // Fills the placeholder of the UI string named by the parameter with the bound value, as for
  // "Remove Telegram".
  internal sealed class FormatConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return UiText.Format(Localizer.Instance[(string)parameter], value);
    }

    public object ConvertBack(
        object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }
  }
}
