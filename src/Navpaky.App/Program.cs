using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Navpaky.App.Localization;
using Navpaky.App.Updates;
using Navpaky.Core;
using Navpaky.Ui;
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
        return BrandFonts.Check() ? 0 : 1;
      }

      bool machine = MachineInstall.IsMachineRoot(InstallRoot());
      if (AllUsersUninstall.Applies(args, machine)) {
        return StartAllUsersUninstall();
      }

      // All users: a pending update is applied only from the tray, never at start or sign-in,
      // where it would ask every user for an administrator's approval.
      VelopackApp.Build()
          .SetAutoApplyOnStartup(!machine)
          .OnAfterInstallFastCallback(_ => ApplyInstall(machine))
          .OnBeforeUpdateFastCallback(_ => InMachineMode(machine, hooks => hooks.BeforeUpdate()))
          .OnAfterUpdateFastCallback(version => InMachineMode(
              machine, hooks => hooks.AfterUpdate(version.ToString())))
          .OnBeforeUninstallFastCallback(_ => Uninstall(machine))
          .Run();
      var app = new App();
      app.InitializeComponent();
      return app.Run();
    }

    // Velopack runs Navpaky from <root>\current; the root holds the Navpaky.exe launcher.
    // AppContext.BaseDirectory would be the %TEMP% folder native libraries are extracted to.
    internal static string InstallRoot() {
      return InstallRoot(Environment.ProcessPath!);
    }

    internal static string InstallRoot(string processPath) {
      return Path.GetDirectoryName(Path.GetDirectoryName(processPath))!;
    }

    // Applies the install wizard's choices (design D4 of add-install-wizard). A step that fails
    // leaves no acknowledgement, so the wizard reports the install as failed; without the wizard
    // only the install marker is written, and its failure must not fail the install.
    private static void ApplyInstall(bool machine) {
      try {
        new InstallHook(
            DataFolders.ForCurrentUser(null),
            InstallRoot(),
            (on, exe) => new SignInLaunch().Set(on, exe),
            new UserDesktopShortcut(),
            machine ? () => MachineInstall.ForThisPc().MarkActiveSetupDone() : null).Run();
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException
          or System.Runtime.InteropServices.COMException) {
        Trace.WriteLine($"Navpaky could not apply its installation: {e.GetType().Name}");
      }
    }

    // Runs an elevated hook step of an all-users install; a per-user install has none. A step
    // that fails leaves the rest of the update or uninstall to go on, as it did before.
    private static void InMachineMode(bool machine, Action<MachineHooks> step) {
      if (!machine) {
        return;
      }

      string root = InstallRoot();
      MachineInstall install = MachineInstall.ForThisPc();
      try {
        step(new MachineHooks(
            version => install.RefreshEntry(root, version),
            () => install.Unregister(root),
            () => CloseSignal.Raise(CloseSignal.Name),
            new OtherSessions(root)));
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException
          or System.Security.SecurityException or WaitHandleCannotBeOpenedException
          or System.Runtime.InteropServices.COMException) {
        Trace.WriteLine($"Navpaky could not finish an all-users step: {e.GetType().Name}");
      }
    }

    private static int StartAllUsersUninstall() {
      try {
        Process.Start(AllUsersUninstall.Next(
            Environment.IsPrivilegedProcess, Environment.ProcessPath!, InstallRoot()))?.Dispose();
        return 0;
      } catch (Win32Exception e) {
        // 1223: the administrator's approval was declined; nothing changed.
        Trace.WriteLine($"Navpaky uninstall not started: {e.NativeErrorCode}");
        return e.NativeErrorCode;
      }
    }

    private static void Uninstall(bool machine) {
      InMachineMode(machine, hooks => hooks.BeforeUninstall());

      // The uninstall questions follow the Windows display language, mapped as the "system"
      // choice is; settings are not read, so uninstalling never writes to the data folder.
      Strings.Culture = UiCulture.For(UiLanguage.System, CultureInfo.CurrentUICulture);
      new Uninstaller(
          DataFolders.ForCurrentUser(null),
          () => new SignInLaunch().Set(false, string.Empty),
          () => new UserDesktopShortcut().Remove(InstallRoot()),
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
