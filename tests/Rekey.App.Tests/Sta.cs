using System.Runtime.ExceptionServices;
using Xunit;

namespace Rekey.App.Tests {
  // WPF objects need an STA thread. Tests that make them run one at a time: WPF objects made
  // on several STA threads at once deadlocked the test run.
  [CollectionDefinition(nameof(Sta), DisableParallelization = true)]
  public sealed class Sta {
    private Sta() {
    }

    // Runs the test on a new STA thread; a failure there is rethrown with its original stack.
    public static void Run(Action test) {
      ExceptionDispatchInfo? failure = null;
      var thread = new Thread(() => {
        try {
          test();
        } catch (Exception e) when (e is not OutOfMemoryException) {
          failure = ExceptionDispatchInfo.Capture(e);
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
      failure?.Throw();
    }
  }
}
