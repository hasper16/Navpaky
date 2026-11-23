using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Navpaky.App.Feedback;
using Navpaky.App.Localization;
using Navpaky.App.ViewModels;
using Navpaky.Core;

namespace Navpaky.App.Views {
  // General: the interface language, start at sign-in, the cursor indicator and the switch
  // sound with a preview.
  internal sealed partial class GeneralPage {
    public GeneralPage(SettingsViewModel model, Action<SwitchSound> preview, bool dark) {
      InitializeComponent();
      DataContext = model;
      Languages.ItemsSource = new LocalizedChoice[] {
        new(UiLanguage.System, "UiLanguageSystem"),
        new(UiLanguage.English, "UiLanguageEnglish"),
        new(UiLanguage.Ukrainian, "UiLanguageUkrainian"),
      };
      Sounds.ItemsSource = new LocalizedChoice[] {
        new(SwitchSound.KeyClick, "SoundKeyClick"),
        new(SwitchSound.SoftTick, "SoundSoftTick"),
        new(SwitchSound.Chime, "SoundChime"),
        new(SwitchSound.Bandura, "SoundBandura"),
        new(SwitchSound.WoodenSpoons, "SoundWoodenSpoons"),
        new(SwitchSound.Drop, "SoundDrop"),
        new(SwitchSound.Glass, "SoundGlass"),
        new(SwitchSound.Cymbals, "SoundCymbals"),
        new(SwitchSound.Sopilka, "SoundSopilka"),
        new(SwitchSound.JawHarp, "SoundJawHarp"),
        new(SwitchSound.FrameDrum, "SoundFrameDrum"),
        new(SwitchSound.Navpaky, "SoundNavpaky"),
      };

      Preview.Click += (_, _) => preview(model.SwitchSound);
      AddIndicatorStyles(model, dark);
    }

    private void AddIndicatorStyles(SettingsViewModel model, bool dark) {
      (CursorIndicatorStyle Style, string Key)[] styles = [
          (CursorIndicatorStyle.Pill, "IndicatorStylePill"),
          (CursorIndicatorStyle.Tag, "IndicatorStyleTag"),
          (CursorIndicatorStyle.Underline, "IndicatorStyleUnderline")];
      foreach ((CursorIndicatorStyle style, string key) in styles) {
        var name = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(IndicatorMark.Build(style, "UK", dark));
        content.Children.Add(name);
        var choice = new RadioButton {
          GroupName = "IndicatorStyle",
          Content = content,
          IsChecked = model.CursorIndicatorStyle == style,
          Style = (Style)FindResource("PillRadio"),
          Padding = new Thickness(4, 0, 12, 0),
        };
        Localizer.Follow(this, () => {
          name.Text = Strings.ResourceManager.GetString(key, Strings.Culture);
          AutomationProperties.SetName(choice, name.Text);
        });
        choice.Checked += (_, _) => model.CursorIndicatorStyle = style;
        IndicatorStyles.Children.Add(choice);
      }
    }
  }
}
