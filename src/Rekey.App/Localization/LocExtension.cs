using System.Windows.Data;
using System.Windows.Markup;

namespace Rekey.App.Localization {
  // {l:Loc Key}: a one-way binding to the string Key that follows the interface language.
  [MarkupExtensionReturnType(typeof(object))]
  internal sealed class LocExtension : MarkupExtension {
    public LocExtension(string key) {
      Key = key;
    }

    [ConstructorArgument("key")]
    public string Key { get; set; }

    // The same binding for views built in code.
    public static Binding Binding(string key) {
      return new Binding($"[{key}]") { Source = Localizer.Instance, Mode = BindingMode.OneWay };
    }

    public override object ProvideValue(IServiceProvider serviceProvider) {
      return Binding(Key).ProvideValue(serviceProvider);
    }
  }
}
