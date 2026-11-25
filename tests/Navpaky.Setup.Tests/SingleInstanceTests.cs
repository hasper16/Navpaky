using Xunit;

namespace Navpaky.Setup.Tests {
  // Task 6.1 of add-install-wizard: a second start exits and brings the first window forward.
  public sealed class SingleInstanceTests {
    private readonly string _name = $"NavpakySetupTest-{Guid.NewGuid():N}";

    [Fact]
    public void ASecondStartIsRefusedAndWakesTheFirst() {
      using var woke = new ManualResetEventSlim();
      using SingleInstance? first = SingleInstance.TryClaim(_name, woke.Set);

      SingleInstance? second = SingleInstance.TryClaim(_name, () => { });

      Assert.NotNull(first);
      Assert.Null(second);
      Assert.True(woke.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void TheNameIsFreeAgainOnceTheFirstHasClosed() {
      SingleInstance.TryClaim(_name, () => { })?.Dispose();

      using SingleInstance? again = SingleInstance.TryClaim(_name, () => { });

      Assert.NotNull(again);
    }
  }
}
