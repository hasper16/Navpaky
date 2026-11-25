using System.IO;
using Navpaky.Setup.Install;
using Navpaky.Win32;

namespace Navpaky.Setup {
  internal static class Program {
    // The elevated all-users step runs before any WPF object exists: no window, and none of the
    // UI libraries load in the elevated process (design D2 of add-machine-wide-install).
    [STAThread]
    public static int Main(string[] args) {
      if (MachineSetup.Parse(args) is { } request) {
        return RunMachineSetup(request.Attempt, request.SignIn);
      }

      var app = new App();
      app.InitializeComponent();
      return app.Run();
    }

    private static int RunMachineSetup(Guid attempt, bool signIn) {
      var setup = new MachineSetup(
          Environment.IsPrivilegedProcess,
          SetupPayload.ExtractTo,
          new ProcessSetupLauncher(),
          (root, version, on) => MachineInstall.ForThisPc().Register(root, version, on),
          MachineInstall.MachineRoot,
          Path.GetTempPath());
      return setup.RunAsync(attempt, signIn).GetAwaiter().GetResult();
    }
  }
}
