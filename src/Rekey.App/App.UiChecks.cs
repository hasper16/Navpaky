#if DEBUG
using System.IO;
using System.Windows.Threading;
using Rekey.Core;

namespace Rekey.App {
  // For tools/Rekey.DesktopChecks (set tray-and-settings): with --check-ui the app writes what
  // its tray, indicator and sound did to check-ui.txt and takes commands from
  // check-ui-command.txt, standing in for clicks on the tray menu. Debug builds only.
  internal sealed partial class App {
    private const string _checkUi = "--check-ui";
    private const string _uiStateFile = "check-ui.txt";
    private const string _uiCommandFile = "check-ui-command.txt";

    private bool _checkingUi;
    private int _corrections;
    private int _settingsOpened;
    private DispatcherTimer? _commandTimer;

    private void StartUiCheck() {
      _checkingUi = true;
      File.Delete(Path.Join(_dataFolder, _uiCommandFile));
      _commandTimer = new DispatcherTimer(
          TimeSpan.FromMilliseconds(200), DispatcherPriority.Normal, (_, _) => TakeCommand(),
          Dispatcher);
      _commandTimer.Start();
      WriteUiState();
    }

    private void TakeCommand() {
      string path = Path.Join(_dataFolder, _uiCommandFile);
      if (!File.Exists(path)) {
        return;
      }

      string command = File.ReadAllText(path).Trim();
      File.Delete(path);
      if (command == "open-settings") {
        OpenSettings();
        WriteUiState();
      } else if (command == "quit") {
        Shutdown();
      } else {
        // Unknown commands are dropped.
      }
    }

    private void NoteSwitchForCheck(SwitchFeedback feedback) {
      if (feedback.Cause == SwitchCause.Correction) {
        _corrections++;
      }
    }

    private void NoteSettingsOpenedForCheck() {
      _settingsOpened++;
    }

    private void WriteUiState() {
      if (!_checkingUi || _tray is null) {
        return;
      }

      string state = $"language={_tray.LanguageCode} corrections={_corrections} "
          + $"sounds={_sounds?.Played ?? 0} settings-opened={_settingsOpened} "
          + $"indicator={_indicator?.IsShown ?? false}";
      Directory.CreateDirectory(_dataFolder);
      File.WriteAllText(Path.Join(_dataFolder, _uiStateFile), state);
    }
  }
}
#endif
