using System.Reflection;
using Rekey.App.Localization;
using Rekey.App.Tray;
using Rekey.App.ViewModels;

namespace Rekey.App.Views {
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
