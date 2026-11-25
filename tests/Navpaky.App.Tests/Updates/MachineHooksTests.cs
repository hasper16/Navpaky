using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class MachineHooksTests {
    private readonly List<string> _steps = [];
    private bool _othersExit;
    private Exception? _raiseFails;

    [Fact]
    public void AfterUpdateRefreshesTheEntryWithTheNewVersion() {
      Hooks().AfterUpdate("0.2.0");

      Assert.Equal(["entry 0.2.0"], _steps);
    }

    [Fact]
    public void BeforeUpdateAsksOthersToCloseAndWaits() {
      _othersExit = true;

      Hooks().BeforeUpdate();

      Assert.Equal(["raise", "wait 10", "lower"], _steps);
    }

    // Kill returns before the process is gone; the update must not start on a locked folder.
    [Fact]
    public void StopsTheOnesThatDidNotCloseAndWaitsForThemToGo() {
      Hooks().BeforeUpdate();

      Assert.Equal(["raise", "wait 10", "stop", "wait 3", "lower"], _steps);
    }

    [Fact]
    public void BeforeUninstallClosesOthersThenUnregisters() {
      _othersExit = true;

      Hooks().BeforeUninstall();

      Assert.Equal(["raise", "wait 10", "lower", "unregister"], _steps);
    }

    // Someone else holds the event name: no graceful close, but the rest still happens.
    [Fact]
    public void ASignalThatCannotBeRaisedStillStopsOthersAndUnregisters() {
      _raiseFails = new UnauthorizedAccessException();

      Hooks().BeforeUninstall();

      Assert.Equal(["stop", "wait 3", "unregister"], _steps);
    }

    private MachineHooks Hooks() {
      return new MachineHooks(
          version => _steps.Add($"entry {version}"),
          () => _steps.Add("unregister"),
          () => {
            if (_raiseFails is not null) {
              throw _raiseFails;
            }

            _steps.Add("raise");
            return new Lowered(_steps);
          },
          new FakeOthers(_steps, () => _othersExit));
    }

    private sealed class Lowered(List<string> steps) : IDisposable {
      public void Dispose() {
        steps.Add("lower");
      }
    }

    private sealed class FakeOthers(List<string> steps, Func<bool> exit) : IOtherSessions {
      public bool WaitForExit(TimeSpan timeout) {
        steps.Add($"wait {timeout.TotalSeconds}");
        return exit();
      }

      public void Stop() {
        steps.Add("stop");
      }
    }
  }
}
