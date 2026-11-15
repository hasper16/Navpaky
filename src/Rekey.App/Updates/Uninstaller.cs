using System.Diagnostics;
using System.IO;

namespace Rekey.App.Updates {
  // Runs while Velopack uninstalls Rekey (design D5 of add-auto-update-and-installer): the
  // sign-in entry goes, and the user decides whether their data goes too. Velopack removes the
  // install root and the Start menu entry itself.
  internal sealed class Uninstaller {
    private const int _deleteAttempts = 5;
    private static readonly TimeSpan _retryDelay = TimeSpan.FromMilliseconds(200);

    private readonly string _dataFolder;
    private readonly Action _turnOffSignIn;
    private readonly Func<bool> _askDelete;

    public Uninstaller(string dataFolder, Action turnOffSignIn, Func<bool> askDelete) {
      _dataFolder = dataFolder;
      _turnOffSignIn = turnOffSignIn;
      _askDelete = askDelete;
    }

    public void Run() {
      _turnOffSignIn();
      if (Directory.Exists(_dataFolder) && _askDelete()) {
        Delete();
      }
    }

    // A Rekey that is still closing may hold a log open for a moment.
    private void Delete() {
      for (int attempt = 1; attempt <= _deleteAttempts; attempt++) {
        try {
          Directory.Delete(_dataFolder, recursive: true);
          return;
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
          // The type only: messages name paths inside the user profile.
          Trace.WriteLine($"Rekey could not delete its data yet: {e.GetType().Name}");
          Thread.Sleep(_retryDelay);
        }
      }
    }
  }
}
