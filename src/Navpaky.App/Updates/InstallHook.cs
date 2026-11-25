using System.IO;
using Navpaky.Core;

namespace Navpaky.App.Updates {
  // Velopack's install hook: applies the install wizard's choices, if it left any, inside the 30 s
  // Velopack gives the hook (add-install-wizard, design D4). The acknowledgement comes last, so the
  // wizard sees a step that failed or ran out of time as a failed install.
  internal sealed class InstallHook {
    private readonly DataFolders _data;
    private readonly string _installFolder;
    private readonly Action<bool, string> _setSignIn;
    private readonly IDesktopShortcut _shortcut;

    public InstallHook(
        DataFolders data,
        string installFolder,
        Action<bool, string> setSignIn,
        IDesktopShortcut shortcut) {
      _data = data;
      _installFolder = installFolder;
      _setSignIn = setSignIn;
      _shortcut = shortcut;
    }

    public void Run() {
      InstallChoices? choices = CurrentChoices();
      if (choices?.Data == KeptData.Fresh) {
        MoveAside(choices.Attempt);
      }

      InstallMarker.Write(_data.Local);
      if (choices is null) {
        // The plain Velopack setup: the welcome window asks the install questions.
        return;
      }

      _setSignIn(choices.LaunchAtSignIn, Path.Join(_installFolder, "Navpaky.exe"));
      if (choices.DesktopShortcut) {
        _shortcut.Create(_installFolder);
      } else {
        _shortcut.Remove(_installFolder);
      }

      Remember(choices);
      File.WriteAllText(Path.Join(_data.Local, choices.AcknowledgementName), string.Empty);
    }

    // The wizard starts the setup right after writing its choices and gives it 5 minutes. An
    // older file was left by a wizard that crashed or was killed: it must not steer this install.
    private InstallChoices? CurrentChoices() {
      string file = Path.Join(_data.Local, InstallChoices.FileName);
      if (File.Exists(file)
          && DateTime.UtcNow - File.GetLastWriteTimeUtc(file) > TimeSpan.FromMinutes(10)) {
        File.Delete(file);
        return null;
      }

      return InstallChoices.ReadFrom(_data.Local);
    }

    private void Remember(InstallChoices choices) {
      var store = new SettingsStore(_data.Settings);
      Settings settings = store.Load();
      store.Save(settings with {
        UiLanguage = choices.UiLanguage,
        InstallFolder = _installFolder,
        DesktopShortcut = choices.DesktopShortcut,
        InstallQuestionsAnswered = true,

        // "Use previous settings" goes straight to work; a first install or Start fresh shows
        // the welcome window.
        WelcomeCompleted = choices.Data == KeptData.Keep || settings.WelcomeCompleted,
      });
    }

    // Start fresh: the kept data moves aside, deleting nothing, so a failed install can put it
    // back. The wizard deletes it once the install completed. A part whose backup of this attempt
    // exists was moved by a run Velopack stopped; what is there now belongs to the new install.
    private void MoveAside(Guid attempt) {
      string suffix = $".fresh-{attempt:N}";
      if (Directory.Exists(_data.Settings) && !Directory.Exists(_data.Settings + suffix)) {
        Directory.Move(_data.Settings, _data.Settings + suffix);
      }

      if (!Directory.Exists(_data.Local) || Directory.Exists(_data.Local + suffix)) {
        return;
      }

      string backup = Directory.CreateDirectory(_data.Local + suffix).FullName;
      foreach (string entry in Directory.EnumerateFileSystemEntries(_data.Local)) {
        string name = Path.GetFileName(entry);
        if (name == InstallChoices.FileName) {
          continue;
        }

        if (Directory.Exists(entry)) {
          Directory.Move(entry, Path.Join(backup, name));
        } else {
          File.Move(entry, Path.Join(backup, name));
        }
      }
    }
  }
}
