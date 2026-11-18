using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Navpaky.App.Localization;
using Navpaky.App.Updates;
using Navpaky.Core;
using Navpaky.Win32;
using Velopack;

namespace Navpaky.App {
  internal static class Program {
    private static readonly TimeSpan _answerWait = TimeSpan.FromSeconds(50);

    // Velopack runs first: during install, update and uninstall it calls Navpaky with its own
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
    // answer for 50 s, a margin below the 60 s Velopack 1.2.161 gives it (fix 2.12 of
    // add-final-acceptance). Past that the question stays open, and data goes only on Yes.
    // When the question cannot start, the data stays, as when the user declines.
    private static void AskDeleteData(string data) {
      // The hook runs as <install root>\current\Navpaky.App.exe.
      string installRoot = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(
          AppContext.BaseDirectory))!;
      try {
        using Process? question = Process.Start(DataQuestion.StartInfo(data, installRoot));
        question?.WaitForExit(_answerWait);
      } catch (Exception e) when (e is Win32Exception or FileNotFoundException) {
        Trace.WriteLine($"Navpaky could not ask about its data: {e.GetType().Name}");
      }
    }
  }
}
