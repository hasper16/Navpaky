using Rekey.App.Localization;
using Rekey.App.ViewModels;
using Rekey.Core;

namespace Rekey.App.Views {
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
      };

      Preview.Click += (_, _) => preview(model.SwitchSound);
    }
  }
}
