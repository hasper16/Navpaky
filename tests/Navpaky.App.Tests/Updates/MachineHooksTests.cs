using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class MachineHooksTests {
    private readonly List<string> _steps = [];
    private bool _othersExit;

    [Fact]
    public void AfterUpdateRefreshesTheEntryWithTheNewVersion() {
      Hooks().AfterUpdate("0.2.0");

      Assert.Equal(["entry 0.2.0"], _steps);
    }

    [Fact]
    public void BeforeUpdateAsksOthersToCloseAndWaits() {
      _othersExit = true;

      Hooks().BeforeUpdate();

      Assert.Equal(["raise", "wait", "lower"], _steps);
    }

    [Fact]
    public void StopsTheOnesThatDidNotClose() {
      Hooks().BeforeUpdate();

      Assert.Equal(["raise", "wait", "stop", "lower"], _steps);
    }

    [Fact]
    public void BeforeUninstallClosesOthersThenUnregisters() {
      _othersExit = true;

      Hooks().BeforeUninstall();

      Assert.Equal(["raise", "wait", "lower", "unregister"], _steps);
    }

    private MachineHooks Hooks() {
      return new MachineHooks(
          version => _steps.Add($"entry {version}"),
          () => _steps.Add("unregister"),
          () => {
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
        Assert.Equal(MachineHooks.CloseWait, timeout);
        steps.Add("wait");
        return exit();
      }

      public void Stop() {
        steps.Add("stop");
      }
    }
  }
}
