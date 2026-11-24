using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Navpaky.Ui {
  /// <summary>
  /// The interface strings in the current language for XAML bindings (<c>{u:Loc Key}</c>). A
  /// language change refreshes every binding at once (design D2 of add-ui-localization). Each
  /// executable adds its own strings with <see cref="AddStrings"/>.
  /// </summary>
  public sealed class Localizer : INotifyPropertyChanged {
    private sealed class LanguageConverter : IValueConverter {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return XmlLanguage.GetLanguage(((CultureInfo)value).IetfLanguageTag);
      }

      public object ConvertBack(
          object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
      }
    }

    private readonly List<ResourceManager> _strings = [];

    /// <summary>Initializes a new instance of the <see cref="Localizer"/> class.</summary>
    /// <param name="culture">The interface language to start with.</param>
    public Localizer(CultureInfo culture) {
      Culture = culture;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the process's one instance; each executable sets its language.</summary>
    public static Localizer Instance { get; } = new(CultureInfo.CurrentUICulture);

    /// <summary>Gets the current interface language.</summary>
    public CultureInfo Culture { get; private set; }

    /// <summary>
    /// Gets the interface string for <paramref name="key"/> in the current language.
    /// </summary>
    /// <param name="key">The key of the interface string.</param>
    /// <exception cref="KeyNotFoundException">No added strings have the key.</exception>
    public string this[string key] {
      get {
        foreach (ResourceManager strings in _strings) {
          if (strings.GetString(key, Culture) is { } text) {
            return text;
          }
        }

        throw new KeyNotFoundException(key);
      }
    }

    /// <summary>
    /// For text a view sets in code: <paramref name="show"/> runs whenever the view loads and,
    /// while it is loaded, whenever the language changes. Unloading lets go, so a closed view is
    /// not kept alive.
    /// </summary>
    /// <param name="view">The view that shows the text.</param>
    /// <param name="show">Sets the text in the current language.</param>
    public static void Follow(FrameworkElement view, Action show) {
      PropertyChangedEventHandler changed = (_, _) => show();
      view.Loaded += (_, _) => {
        Instance.PropertyChanged -= changed;
        Instance.PropertyChanged += changed;
        show();
      };
      view.Unloaded += (_, _) => Instance.PropertyChanged -= changed;
    }

    /// <summary>
    /// Makes <paramref name="root"/> follow the interface language. Screen readers choose their
    /// voice by an element's language, which WPF does not take from the culture.
    /// </summary>
    /// <param name="root">The root of a window or menu.</param>
    public static void BindLanguage(FrameworkElement root) {
      root.SetBinding(FrameworkElement.LanguageProperty, new Binding(nameof(Culture)) {
        Source = Instance,
        Converter = new LanguageConverter(),
        ConverterCulture = CultureInfo.InvariantCulture,
      });
    }

    /// <summary>
    /// Makes every window take the interface language when it loads. Call it once per process,
    /// before any window opens.
    /// </summary>
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

    /// <summary>Adds an executable's strings; adding the same strings again does nothing.</summary>
    /// <param name="strings">The executable's resource manager.</param>
    public void AddStrings(ResourceManager strings) {
      if (!_strings.Contains(strings)) {
        _strings.Add(strings);
      }
    }

    /// <summary>Switches the interface language and refreshes every binding.</summary>
    /// <param name="culture">The new interface language.</param>
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
