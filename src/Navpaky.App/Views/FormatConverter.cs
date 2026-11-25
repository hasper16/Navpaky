using System.Globalization;
using System.Windows.Data;
using Navpaky.App.Localization;
using Navpaky.Ui;

namespace Navpaky.App.Views {
  // Fills the placeholder of the UI string named by the parameter with the first bound value,
  // as for "Remove Telegram"; the second value, the Localizer's culture, only makes the binding
  // run again when the language changes.
  internal sealed class FormatConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
      return UiText.Format(Localizer.Instance[(string)parameter], values[0]);
    }

    public object[] ConvertBack(
        object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }
  }
}
