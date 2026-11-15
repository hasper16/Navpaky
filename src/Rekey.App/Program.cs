using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Rekey.App.Updates;
using Rekey.Win32;
using Velopack;

namespace Rekey.App {
  internal static class Program {
    // Velopack runs first: during install, update and uninstall it calls Rekey with its own
    // arguments and exits before any window, hook or single-instance claim exists.
    [STAThread]
    public static void Main() {
      VelopackApp.Build().OnBeforeUninstallFastCallback(_ => Uninstall()).Run();
      var app = new App();
      app.InitializeComponent();
      app.Run();
    }

    private static void Uninstall() {
      string data = DataFolder.Resolve(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), null);
      new Uninstaller(data, () => new SignInLaunch().Set(false, string.Empty), AskDeleteData)
          .Run();
    }

    // When the question cannot start, the data stays, as when the user declines.
    private static void AskDeleteData(string data) {
      try {
        Process.Start(DataQuestion.StartInfo(data))?.Dispose();
      } catch (Exception e) when (e is Win32Exception or FileNotFoundException) {
        Trace.WriteLine($"Rekey could not ask about its data: {e.GetType().Name}");
      }
    }
  }
}
