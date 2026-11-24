using System.Windows;

namespace Navpaky.Setup {
  /// <summary>
  /// The install wizard's application object.
  /// </summary>
  public partial class App : Application {
    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      // The window arrives with task 6.1 of add-install-wizard.
      Shutdown();
    }
  }
}
