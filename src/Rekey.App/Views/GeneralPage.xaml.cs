using System.Windows.Controls;
using Rekey.App.ViewModels;
using Rekey.Core;

namespace Rekey.App.Views {
  // General: start at sign-in, the cursor indicator and the switch sound with a preview.
  internal sealed partial class GeneralPage {
    public GeneralPage(SettingsViewModel model, Action<SwitchSound> preview) {
      InitializeComponent();
      DataContext = model;
      (SwitchSound Sound, string Name)[] sounds = [
          (SwitchSound.KeyClick, Strings.SoundKeyClick),
          (SwitchSound.SoftTick, Strings.SoundSoftTick),
          (SwitchSound.Chime, Strings.SoundChime)];
      foreach ((SwitchSound sound, string name) in sounds) {
        Sounds.Items.Add(new ComboBoxItem { Content = name, Tag = sound });
      }

      Preview.Click += (_, _) => preview(model.SwitchSound);
    }
  }
}
