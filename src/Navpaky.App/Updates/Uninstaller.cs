using System.IO;

namespace Navpaky.App.Updates {
  // Runs in Velopack's uninstall hook (design D5 and R11 of add-auto-update-and-installer),
  // which must show no UI and end quickly: the sign-in entry goes, and a separate process asks
  // whether the data goes too. Velopack removes the install root and the Start menu entry itself.
  internal sealed class Uninstaller {
    private readonly DataFolders _data;
    private readonly Action _turnOffSignIn;
    private readonly Action<DataFolders> _startDataQuestion;

    public Uninstaller(
        DataFolders data, Action turnOffSignIn, Action<DataFolders> startDataQuestion) {
      _data = data;
      _turnOffSignIn = turnOffSignIn;
      _startDataQuestion = startDataQuestion;
    }

    public void Run() {
      _turnOffSignIn();
      if (Directory.Exists(_data.Settings) || Directory.Exists(_data.Local)) {
        _startDataQuestion(_data);
      }
    }
  }
}
