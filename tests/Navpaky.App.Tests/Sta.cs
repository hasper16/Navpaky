using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using Xunit;

namespace Navpaky.App.Tests {
  // WPF objects need an STA thread. Tests that make them run one at a time, all on one UI thread
  // of their own: WPF objects made on several STA threads at once deadlocked the test run, and
  // the Application one test creates belongs to the thread that made it.
  [CollectionDefinition(nameof(Sta), DisableParallelization = true)]
  public sealed class Sta {
    private static readonly Lazy<Dispatcher> _ui = new(StartUiThread);

    private Sta() {
    }

    // Runs the test on the UI thread; a failure there is rethrown with its original stack.
    public static void Run(Action test) {
      ExceptionDispatchInfo? failure = null;
      _ui.Value.Invoke(() => {
        try {
          test();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          failure = ExceptionDispatchInfo.Capture(e);
        }
      });
      failure?.Throw();
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
