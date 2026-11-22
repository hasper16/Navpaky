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
    // Runs only the font check on this build and exits with 0 when both brand fonts load (CI).
    private const string _checkFonts = "--check-fonts";

    private static readonly TimeSpan _answerWait = TimeSpan.FromSeconds(50);

    // Velopack runs first: during install, update and uninstall it calls Navpaky with its own
    // arguments and exits before any window, hook or single-instance claim exists.
    [STAThread]
    public static int Main(string[] args) {
      if (args.Contains(_checkFonts)) {
        _ = new System.Windows.Application();
        return Views.BrandFonts.Check() ? 0 : 1;
      }

      VelopackApp.Build()
          .OnAfterInstallFastCallback(_ => MarkInstall())
          .OnBeforeUninstallFastCallback(_ => Uninstall())
          .Run();
      var app = new App();
      app.InitializeComponent();
      return app.Run();
    }

    // Without the marker only a reinstall made while an old data question is open goes unseen,
    // so a failed write must not fail the install.
    private static void MarkInstall() {
      try {
        InstallMarker.Write(DataFolders.ForCurrentUser(null).Local);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        Trace.WriteLine($"Navpaky could not mark its installation: {e.GetType().Name}");
      }
    }

    private static void Uninstall() {
      // The uninstall questions follow the Windows display language, mapped as the "system"
      // choice is; settings are not read, so uninstalling never writes to the data folder.
      Strings.Culture = UiCulture.For(UiLanguage.System, CultureInfo.CurrentUICulture);
      new Uninstaller(
          DataFolders.ForCurrentUser(null),
          () => new SignInLaunch().Set(false, string.Empty),
          AskDeleteData).Run();
    }

    // Velopack shows "removal complete" once this hook returns, so the hook waits for the
    // answer for 50 s, a margin below the 60 s Velopack 1.2.161 gives it (fix 2.12 of
    // add-final-acceptance). Past that the question stays open, and data goes only on Yes.
    // The question ends as soon as it is answered, so the uninstall goes on at once.
    // When the question cannot start, the data stays, as when the user declines.
    private static void AskDeleteData(DataFolders data) {
      try {
        using Process? question = Process.Start(DataQuestion.StartInfo(data));
        question?.WaitForExit(_answerWait);
      } catch (Exception e) when (e is Win32Exception or FileNotFoundException) {
        Trace.WriteLine($"Navpaky could not ask about its data: {e.GetType().Name}");
      }
    }
  }
}
