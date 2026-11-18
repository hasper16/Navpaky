using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rekey.App.Localization {
  // The interface strings in the current language for XAML bindings ({l:Loc Key}); a language
  // change refreshes every binding at once (design D2).
  internal sealed class Localizer : INotifyPropertyChanged {
    private sealed class LanguageConverter : IValueConverter {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return XmlLanguage.GetLanguage(((CultureInfo)value).IetfLanguageTag);
      }

      public object ConvertBack(
          object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
      }
    }

    public Localizer(CultureInfo culture) {
      Culture = culture;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // The app's one instance; App sets its language from the settings.
    public static Localizer Instance { get; } = new(CultureInfo.CurrentUICulture);

    public CultureInfo Culture { get; private set; }

    public string this[string key] =>
        Strings.ResourceManager.GetString(key, Culture) ?? throw new KeyNotFoundException(key);

    // For text a view sets in code: show runs whenever the view loads and while it is loaded
    // the language changes. Unloading lets go, so a closed view is not kept alive.
    public static void Follow(FrameworkElement view, Action show) {
      PropertyChangedEventHandler changed = (_, _) => show();
      view.Loaded += (_, _) => {
        Instance.PropertyChanged -= changed;
        Instance.PropertyChanged += changed;
        show();
      };
      view.Unloaded += (_, _) => Instance.PropertyChanged -= changed;
    }

    // Screen readers choose their voice by an element's language, which WPF does not take from
    // the culture: the root of each window and menu follows the interface language instead.
    public static void BindLanguage(FrameworkElement root) {
      root.SetBinding(FrameworkElement.LanguageProperty, new Binding(nameof(Culture)) {
        Source = Instance,
        Converter = new LanguageConverter(),
        ConverterCulture = CultureInfo.InvariantCulture,
      });
    }

    // Every window takes the interface language when it loads. Once per process, before any window
    // opens.
    public static void BindEveryWindow() {
      EventManager.RegisterClassHandler(
          typeof(Window),
          FrameworkElement.LoadedEvent,
          new RoutedEventHandler((window, _) => {
            if (!BindingOperations.IsDataBound((Window)window, FrameworkElement.LanguageProperty)) {
              BindLanguage((Window)window);
            }
          }));
    }

    public void Use(CultureInfo culture) {
      if (Equals(culture, Culture)) {
        return;
      }

      Culture = culture;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
    }
  }
}
