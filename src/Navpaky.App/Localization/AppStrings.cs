using System.Runtime.CompilerServices;
using Navpaky.Ui;

namespace Navpaky.App.Localization {
  // The shared Localizer serves the app's strings from the moment this assembly loads, so views
  // and tests never see it without them (add-install-wizard, design D2).
  internal static class AppStrings {
    [ModuleInitializer]
    internal static void Register() {
      Localizer.Instance.AddStrings(Strings.ResourceManager);
    }
  }
}
