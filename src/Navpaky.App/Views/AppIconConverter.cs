using System.Globalization;
using System.Windows.Data;
using Navpaky.App.Tray;

namespace Navpaky.App.Views {
  // The icon of an Excluded App from its settings entry, a program path.
  internal sealed class AppIconConverter : IValueConverter {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value is string path ? AppIcons.Of(path) : null;
    }

    public object ConvertBack(
        object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotSupportedException();
    }
  }
}
