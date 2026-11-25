using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class CloseForUpdateTests {
    private bool _raised;
    private int _closes;

    [Fact]
    public void StaysOpenWhileNothingIsRaised() {
      using CloseForUpdate watch = Watch();

      watch.Check();

      Assert.Equal(0, _closes);
    }

    [Fact]
    public void ClosesOnceWhenRaised() {
      using CloseForUpdate watch = Watch();
      _raised = true;

      watch.Check();
      watch.Check();

      Assert.Equal(1, _closes);
    }

    [Fact]
    public void ChecksOnItsTimer() {
      using var closed = new ManualResetEventSlim();
      using var watch = new CloseForUpdate(() => true, closed.Set, TimeSpan.FromMilliseconds(10));

      watch.Start();

      Assert.True(closed.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }

    private CloseForUpdate Watch() {
      return new CloseForUpdate(() => _raised, () => _closes++, TimeSpan.FromHours(1));
    }
  }
}
