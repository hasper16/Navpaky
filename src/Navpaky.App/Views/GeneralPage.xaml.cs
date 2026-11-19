using Navpaky.App.Localization;
using Navpaky.App.ViewModels;
using Navpaky.Core;

namespace Navpaky.App.Views {
  // General: the interface language, start at sign-in, the cursor indicator and the switch
  // sound with a preview.
  internal sealed partial class GeneralPage {
    public GeneralPage(SettingsViewModel model, Action<SwitchSound> preview) {
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
    }
  }
}
