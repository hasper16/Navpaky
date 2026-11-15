using Velopack;

namespace Rekey.App {
  internal static class Program {
    // Velopack runs first: during install, update and uninstall it calls Rekey with its own
    // arguments and exits before any window, hook or single-instance claim exists.
    [STAThread]
    public static void Main() {
      VelopackApp.Build().Run();
      var app = new App();
      app.InitializeComponent();
      app.Run();
    }
  }
}
