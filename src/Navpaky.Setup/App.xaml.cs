using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using Navpaky.Setup.Install;
using Navpaky.Setup.Wizard;
using Navpaky.Ui;
using Wpf.Ui.Appearance;

namespace Navpaky.Setup {
  /// <summary>
  /// The install wizard's application object.
  /// </summary>
  [SuppressMessage(
      "Design",
      "CA1001:Types that own disposable fields should be disposable",
      Justification = "WPF ends the Application through OnExit, which disposes the fields.")]
  public partial class App : Application {
    private SingleInstance? _instance;
    private ThemeFollower? _theme;
    private WizardWindow? _window;

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);
      _instance = SingleInstance.TryClaim(
          "NavpakySetup", () => Dispatcher.InvokeAsync(() => _window?.BringToFront()));
      if (_instance is null) {
        Shutdown();
        return;
      }

      ApplicationThemeManager.ApplySystemTheme();
      BrandTheme.Follow();
      BrandFonts.Apply();
      _theme = new ThemeFollower(Dispatcher, () => { });
      Localizer.BindEveryWindow();
      _window = new WizardWindow(Model());
      MainWindow = _window;
      _window.Show();
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e) {
      _theme?.Dispose();
      _instance?.Dispose();
      base.OnExit(e);
    }

    private static WizardViewModel Model() {
      string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      var data = new DataFolderPair(Path.Join(roaming, "Navpaky"), Path.Join(local, "Navpaky"));
      RestoreLeftovers(data);
      var probe = new DiskFolders();
      var folders = new WizardFolders(
          InstallLookup.Existing(new UninstallEntry(), probe),
          Path.Join(local, "Programs", "Navpaky"),
          data,
          SetupPayload.Length,
          probe);
      var runner = new SetupRunner(SetupPayload.ExtractTo, new ProcessSetupLauncher());
      return new WizardViewModel(
          KeptItems.Find(data),
          folders,
          (choices, log, progress) => Task.Run(() => runner.RunAsync(choices, data, log, progress)),
          StartNavpaky);
    }

    // A backup a crashed run left is put back before the wizard looks for kept data (design D4);
    // one that can't be moved now stays for the next run.
    private static void RestoreLeftovers(DataFolderPair data) {
      try {
        FreshBackups.RestoreLeftovers(data);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        Trace.WriteLine($"Start-fresh backup not restored: {e.GetType().Name}");
      }
    }

    private static void StartNavpaky(string launcher) {
      try {
        Process.Start(new ProcessStartInfo(launcher) { UseShellExecute = true })?.Dispose();
      } catch (Win32Exception e) {
        Trace.WriteLine($"Navpaky not started: {e.NativeErrorCode}");
      }
    }
  }
}
