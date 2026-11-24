using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Navpaky.App.Views;
using Navpaky.Ui;
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

    // The prototype puts white on the accent in both themes; WPF UI's dark theme uses black.
    [Theory]
    [InlineData("ToggleSwitchKnobFillOn")]
    [InlineData("AccentButtonForeground")]
    public void TextOnTheAccentIsWhiteInTheDarkTheme(string key) {
      string? applied = null;
      Sta.Run(() => {
        Sta.EnsureApplication();
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        BrandTheme.Apply(ApplicationTheme.Dark);
        applied = Hex(((SolidColorBrush)UiApplication.Current.Resources[key]).Color);
      });

      Assert.Equal("#FFFFFFFF", applied);
    }

    // WCAG 1.4.3: small hint and caption text, and white text on the accent fill, reach 4.5:1.
    [Theory]
    [InlineData(ApplicationTheme.Light)]
    [InlineData(ApplicationTheme.Dark)]
    public void TextReachesTheContrastMinimum(ApplicationTheme theme) {
      var pairs = new List<(Color Text, Color Back)>();
      Sta.Run(() => {
        Sta.EnsureApplication();
        ApplicationThemeManager.Apply(theme);
        BrandTheme.Apply(theme);
        Color tertiary = Brush("BrandTextTertiary");
        pairs.Add((tertiary, Brush("BrandWindowBackground")));
        pairs.Add((tertiary, Brush("BrandSurface")));
        pairs.Add((Brush("AccentButtonForeground"), Brush("AccentFillColorDefaultBrush")));
        pairs.Add((
            Brush("AccentButtonForegroundPointerOver"),
            Brush("AccentFillColorSecondaryBrush")));
        pairs.Add((Brush("AccentButtonForegroundPressed"), Brush("AccentFillColorTertiaryBrush")));
        pairs.Add((
            Brush("TextOnAccentFillColorPrimaryBrush"), Brush("AccentFillColorDefaultBrush")));
      });

      Assert.All(pairs, pair => Assert.True(
          Contrast(pair.Text, pair.Back) >= 4.5, $"{pair.Text} on {pair.Back}"));
    }

    // In high contrast the surfaces come from the Windows scheme, so text stays readable.
    [Fact]
    public void HighContrastUsesTheSystemColours() {
      Color window = default;
      Color tertiary = default;
      Sta.Run(() => {
        Sta.EnsureApplication();
        BrandTheme.Apply(ApplicationTheme.HighContrast);
        window = Brush("BrandWindowBackground");
        tertiary = Brush("BrandTextTertiary");
      });

      Assert.Equal(SystemColors.WindowColor, window);
      Assert.Equal(SystemColors.WindowTextColor, tertiary);
    }

    // The white text and knobs of the other themes would hide the high-contrast scheme's own.
    [Fact]
    public void HighContrastDropsTheWhiteOnTheAccent() {
      bool kept = true;
      Sta.Run(() => {
        Sta.EnsureApplication();
        BrandTheme.Apply(ApplicationTheme.Dark);
        BrandTheme.Apply(ApplicationTheme.HighContrast);
        kept = UiApplication.Current.Resources.Keys.Cast<object>()
            .Contains("AccentButtonForeground");
        BrandTheme.Apply(ApplicationTheme.Light);
      });

      Assert.False(kept);
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

    private static Color Brush(string key) {
      return ((SolidColorBrush)UiApplication.Current.Resources[key]).Color;
    }

    // Blends a translucent text colour over its background first, as the screen does.
    private static double Contrast(Color text, Color back) {
      double alpha = text.A / 255.0;
      double light = Luminance(
          Mix(text.R, back.R, alpha), Mix(text.G, back.G, alpha), Mix(text.B, back.B, alpha));
      double dark = Luminance(back.R, back.G, back.B);
      (double high, double low) = light > dark ? (light, dark) : (dark, light);
      return (high + 0.05) / (low + 0.05);
    }

    private static double Mix(byte front, byte behind, double alpha) {
      return (front * alpha) + (behind * (1 - alpha));
    }

    private static double Luminance(double r, double g, double b) {
      return (0.2126 * Channel(r)) + (0.7152 * Channel(g)) + (0.0722 * Channel(b));
    }

    private static double Channel(double value) {
      double c = value / 255;
      return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }

    private static string Hex(object value) {
      return ((Color)value).ToString(CultureInfo.InvariantCulture);
    }
  }
}
