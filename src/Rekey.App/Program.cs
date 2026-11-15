using System.Windows;
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

    // Velopack ends the process 30 s after the hook starts; an unanswered question keeps the data.
    private static void Uninstall() {
      string data = DataFolder.Resolve(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), null);
      new Uninstaller(data, () => new SignInLaunch().Set(false, string.Empty), AskDeleteData)
          .Run();
    }

    private static bool AskDeleteData() {
      MessageBoxResult answer = MessageBox.Show(
          Strings.UninstallDeleteData,
          Strings.UninstallTitle,
          MessageBoxButton.YesNo,
          MessageBoxImage.Question,
          MessageBoxResult.No);
      return answer == MessageBoxResult.Yes;
    }
  }
}
