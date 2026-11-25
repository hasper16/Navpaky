using System.Windows.Data;
using System.Windows.Markup;

namespace Navpaky.Ui {
  /// <summary>
  /// <c>{u:Loc Key}</c>: a one-way binding to the interface string <c>Key</c> that follows the
  /// interface language.
  /// </summary>
  [MarkupExtensionReturnType(typeof(object))]
  public sealed class LocExtension : MarkupExtension {
    /// <summary>Initializes a new instance of the <see cref="LocExtension"/> class.</summary>
    /// <param name="key">The key of the interface string.</param>
    public LocExtension(string key) {
      Key = key;
    }

    /// <summary>Gets or sets the key of the interface string.</summary>
    [ConstructorArgument("key")]
    public string Key { get; set; }

    /// <summary>Returns the same binding for views built in code.</summary>
    /// <param name="key">The key of the interface string.</param>
    /// <returns>A one-way binding to the string in the current interface language.</returns>
    public static Binding Binding(string key) {
      return new Binding($"[{key}]") { Source = Localizer.Instance, Mode = BindingMode.OneWay };
    }

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) {
      return Binding(Key).ProvideValue(serviceProvider);
    }
  }
}
