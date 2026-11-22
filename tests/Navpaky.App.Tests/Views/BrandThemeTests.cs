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
        Sta.EnsureApplication();
        BrandTheme.Apply(theme);
        appliedAccent = ((Color)UiApplication.Current.Resources["SystemAccentColor"])
            .ToString(CultureInfo.InvariantCulture);
        appliedSecond = ((SolidColorBrush)UiApplication.Current.Resources["BrandSecondaryBrush"])
            .Color.ToString(CultureInfo.InvariantCulture);
      });

      Assert.Equal(accent, appliedAccent);
      Assert.Equal(second, appliedSecond);
    }

    [Theory]
    [InlineData(ApplicationTheme.Light, "#FFB00E28")]
    [InlineData(ApplicationTheme.Dark, "#FF6B9CF7")]
    public void PrimaryAccentKeepsTheBrandShade(ApplicationTheme theme, string primary) {
      string? applied = null;
      Sta.Run(() => {
        Sta.EnsureApplication();
        BrandTheme.Apply(theme);
        applied = Hex(UiApplication.Current.Resources["SystemAccentColorPrimary"]);
      });

      Assert.Equal(primary, applied);
    }

    [Fact]
    public void RefreshFollowsAThemeChangedWhileRunning() {
      ApplicationTheme after = ApplicationTheme.Unknown;
      string? primary = null;
      Sta.Run(() => {
        Sta.EnsureApplication();
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        ThemeFollower.Refresh(ApplicationTheme.Light);
        after = ApplicationThemeManager.GetAppTheme();
        primary = Hex(UiApplication.Current.Resources["SystemAccentColorPrimary"]);
      });

      Assert.Equal(ApplicationTheme.Light, after);
      Assert.Equal("#FFB00E28", primary);
    }

    [Theory]
    [InlineData(false, 0, ApplicationTheme.Dark)]
    [InlineData(false, 1, ApplicationTheme.Light)]
    [InlineData(false, null, ApplicationTheme.Light)]
    [InlineData(true, 0, ApplicationTheme.HighContrast)]
    public void ThemeFollowsTheAppsModeSetting(
        bool highContrast, int? appsUseLightTheme, ApplicationTheme expected) {
      Assert.Equal(expected, ThemeFollower.ThemeFor(highContrast, appsUseLightTheme));
    }

    private static string Hex(object value) {
      return ((Color)value).ToString(CultureInfo.InvariantCulture);
    }
  }
}
