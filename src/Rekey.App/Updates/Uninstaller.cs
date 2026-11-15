using System.IO;

namespace Rekey.App.Updates {
  // Runs in Velopack's uninstall hook (design D5 and R11 of add-auto-update-and-installer),
  // which must show no UI and end quickly: the sign-in entry goes, and a separate process asks
  // whether the data goes too. Velopack removes the install root and the Start menu entry itself.
  internal sealed class Uninstaller {
    private readonly string _dataFolder;
    private readonly Action _turnOffSignIn;
    private readonly Action<string> _startDataQuestion;

    public Uninstaller(string dataFolder, Action turnOffSignIn, Action<string> startDataQuestion) {
      _dataFolder = dataFolder;
      _turnOffSignIn = turnOffSignIn;
      _startDataQuestion = startDataQuestion;
    }

    public void Run() {
      _turnOffSignIn();
      if (Directory.Exists(_dataFolder)) {
        _startDataQuestion(_dataFolder);
      }
    }
  }
}
