using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Markup;

namespace Navpaky.Setup.Tests {
  // WPF objects need an STA thread; the wizard's window tests share one UI thread, as the app's
  // tests do (Navpaky.App.Tests/Sta).
  internal static class Sta {
    private static readonly Lazy<Dispatcher> _ui = new(StartUiThread);

    // Runs the test on the UI thread; a failure there is rethrown with its original stack.
    public static void Run(Action test) {
      ExceptionDispatchInfo? failure = null;
      _ui.Value.Invoke(() => {
        try {
          EnsureApplication();
          test();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          failure = ExceptionDispatchInfo.Capture(e);
        }
      });
      failure?.Throw();
    }

    // Runs what the dispatcher queued, such as bindings and layout, up to background priority.
    public static void Flush() {
      var frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(
          DispatcherPriority.Background, () => frame.Continue = false);
      Dispatcher.PushFrame(frame);
    }

    // Lets time pass on the UI thread, so animations such as a switch's knob finish.
    public static void Wait(TimeSpan time) {
      var frame = new DispatcherFrame();
      var timer = new DispatcherTimer { Interval = time };
      timer.Tick += (_, _) => {
        timer.Stop();
        frame.Continue = false;
      };
      timer.Start();
      Dispatcher.PushFrame(frame);
    }

    // The window takes the brand styles from the application, as the wizard's App.xaml gives them.
    private static void EnsureApplication() {
      if (Application.Current is null) {
        var host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        host.Resources.MergedDictionaries.Add(
            new ThemesDictionary { Theme = ApplicationTheme.Light });
        host.Resources.MergedDictionaries.Add(new ControlsDictionary());
        host.Resources.MergedDictionaries.Add(new ResourceDictionary {
          Source = new Uri("pack://application:,,,/Navpaky.Ui;component/Brand.xaml"),
        });
      }
    }

    private static Dispatcher StartUiThread() {
      using var ready = new ManualResetEventSlim();
      Dispatcher? dispatcher = null;
      var thread = new Thread(() => {
        dispatcher = Dispatcher.CurrentDispatcher;
        ready.Set();
        Dispatcher.Run();
      }) { IsBackground = true, Name = "WPF tests" };
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      ready.Wait();
      return dispatcher!;
    }
  }
}
