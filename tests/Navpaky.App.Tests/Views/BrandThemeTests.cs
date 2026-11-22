using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Navpaky.App.Views;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Xunit;

namespace Navpaky.App.Tests.Views {
  [Collection(nameof(Sta))]
  public sealed class BrandThemeTests {
    [Theory]
    [InlineData(ApplicationTheme.Light, "#FFC8102E", "#FF17171C")]
    [InlineData(ApplicationTheme.Dark, "#FF4C86F0", "#FFFACC15")]
    public void ThemeGetsItsBrandColours(ApplicationTheme theme, string accent, string second) {
      string? appliedAccent = null;
      string? appliedSecond = null;
      Sta.Run(() => {
        if (Application.Current is null) {
          var host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
          host.Resources.MergedDictionaries.Add(new ResourceDictionary {
            Source = new Uri("pack://application:,,,/Navpaky.App;component/Resources.xaml"),
          });
        }

        BrandTheme.Apply(theme);
        appliedAccent = ((Color)UiApplication.Current.Resources["SystemAccentColor"])
            .ToString(CultureInfo.InvariantCulture);
        appliedSecond = ((SolidColorBrush)UiApplication.Current.Resources["BrandSecondaryBrush"])
            .Color.ToString(CultureInfo.InvariantCulture);
      });

      Assert.Equal(accent, appliedAccent);
      Assert.Equal(second, appliedSecond);
    }
  }
}
