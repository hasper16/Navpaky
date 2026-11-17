using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Rekey.App.Localization;
using Rekey.App.Updates;
using Rekey.Core;
using Rekey.Win32;
using Velopack;

namespace Rekey.App {
  internal static class Program {
    private static readonly TimeSpan _answerWait = TimeSpan.FromSeconds(25);

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
      // The uninstall questions follow the Windows display language, mapped as the "system"
      // choice is; settings are not read, so uninstalling never writes to the data folder.
      Strings.Culture = UiCulture.For(UiLanguage.System, CultureInfo.CurrentUICulture);
      string data = DataFolder.Resolve(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), null);
      new Uninstaller(data, () => new SignInLaunch().Set(false, string.Empty), AskDeleteData)
          .Run();
    }

    // Velopack shows "removal complete" once this hook returns, so the hook waits for the
    // answer, short of Velopack's 30-second limit (fix 2.12 of add-final-acceptance). When the
    // question cannot start, the data stays, as when the user declines.
    private static void AskDeleteData(string data) {
      // The hook runs as <install root>\current\Rekey.App.exe.
      string installRoot = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(
          AppContext.BaseDirectory))!;
      try {
        using Process? question = Process.Start(DataQuestion.StartInfo(data, installRoot));
        question?.WaitForExit(_answerWait);
      } catch (Exception e) when (e is Win32Exception or FileNotFoundException) {
        Trace.WriteLine($"Rekey could not ask about its data: {e.GetType().Name}");
      }
    }
  }
}
