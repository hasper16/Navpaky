using System.Reflection;
using Navpaky.App.Localization;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;

namespace Navpaky.App.Views {
  // About: the keycap mark (design D1) and the version.
  internal sealed partial class AboutPage {
    public AboutPage(bool dark) {
      InitializeComponent();
      Mark.Source = Keycap.Render("EN", TrayState.Active, dark, 64);
      string version = Assembly.GetExecutingAssembly()
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
          ?? "0";
      Localizer.Follow(
          this, () => Version.Text = UiText.Format(Strings.AboutVersion, version.Split('+')[0]));
    }
  }
}
