#if DEBUG
using System.IO;
using System.Windows.Threading;
using Rekey.Core;

namespace Rekey.App {
  // For tools/Rekey.DesktopChecks (sets tray-and-settings and onboarding): with --check-ui the
  // app writes what its tray, indicator, sound and Undo tip did to check-ui.txt and takes
  // commands from check-ui-command.txt, standing in for clicks on the tray menu. Debug builds
  // only.
  internal sealed partial class App {
    private const string _checkUi = "--check-ui";
    private const string _uiStateFile = "check-ui.txt";
    private const string _uiCommandFile = "check-ui-command.txt";

    private bool _checkingUi;
    private int _corrections;
    private int _settingsOpened;
    private int _tips;
    private DispatcherTimer? _commandTimer;

    // Publishes a whole check file in one step, so the tool never reads half of it. While the
    // tool holds the file open the move fails; the next change writes it again.
    private static void WriteCheckFile(string name, string text) {
      string path = Path.Join(_dataFolder, name);
      string draft = path + ".tmp";
      try {
        Directory.CreateDirectory(_dataFolder);
        File.WriteAllText(draft, text);
        File.Move(draft, path, overwrite: true);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        // Left for the next write.
      }
    }

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

      string command;
      try {
        command = File.ReadAllText(path).Trim();
        File.Delete(path);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        // The tool is still writing it; the next poll takes it.
        return;
      }

      if (command == "open-settings") {
        OpenSettings();
        WriteUiState();
      } else if (command == "open-welcome") {
        OpenWelcome();
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

    private void NoteTipForCheck() {
      _tips++;
      WriteUiState();
    }

    private void WriteUiState() {
      if (!_checkingUi || _tray is null) {
        return;
      }

      string state = $"language={_tray.LanguageCode} corrections={_corrections} "
          + $"sounds={_sounds?.Played ?? 0} settings-opened={_settingsOpened} "
          + $"indicator={_indicator?.IsShown ?? false} tips={_tips}";
      WriteCheckFile(_uiStateFile, state);
    }
  }
}
#endif
