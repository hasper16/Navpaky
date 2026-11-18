using Xunit;

namespace Rekey.App.Tests {
  // Spec app-lifecycle, second launch with arguments (add-final-acceptance): the running Rekey
  // never receives them, so the user is told instead of nothing happening.
  public sealed class SecondLaunchTests {
    [Fact]
    public void PlainSecondLaunchSaysNothing() {
      Assert.Null(SecondLaunch.Message([]));
    }

    [Fact]
    public void SecondLaunchWithArgumentsSaysRekeyIsRunning() {
      Assert.Equal(Strings.AlreadyRunning, SecondLaunch.Message(["--update-feed", "Feed"]));
    }

    // A replacement Rekey started after a freeze waits for the frozen one and must never stop
    // the user with a message.
    [Fact]
    public void RestartAfterAFreezeSaysNothing() {
      Assert.Null(SecondLaunch.Message(["--restarted-after-freeze", "1234"]));
    }
  }
}
