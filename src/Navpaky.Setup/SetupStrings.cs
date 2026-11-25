using System.Resources;
using System.Runtime.CompilerServices;
using Navpaky.Ui;

namespace Navpaky.Setup {
  // The shared Localizer serves the wizard's strings from the moment this assembly loads, so
  // views and tests never see it without them (add-install-wizard, design D2).
  internal static class SetupStrings {
    public static ResourceManager Strings { get; } =
        new("Navpaky.Setup.Strings", typeof(SetupStrings).Assembly);

    [ModuleInitializer]
    internal static void Register() {
      Localizer.Instance.AddStrings(Strings);
    }
  }
}
