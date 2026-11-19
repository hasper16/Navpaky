using System.Reflection;
using Navpaky.App.Localization;
using Navpaky.App.Tray;
using Navpaky.App.ViewModels;

namespace Navpaky.App.Views {
  // About: the Navpaky mark and the version.
  internal sealed partial class AboutPage {
    public AboutPage() {
      InitializeComponent();
      Mark.Source = BrandMark.Large();
      string version = Assembly.GetExecutingAssembly()
          .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
          ?? "0";
      Localizer.Follow(
          this, () => Version.Text = UiText.Format(Strings.AboutVersion, version.Split('+')[0]));
    }
  }
}
