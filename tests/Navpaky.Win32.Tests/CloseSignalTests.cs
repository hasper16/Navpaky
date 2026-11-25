using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Xunit;

namespace Navpaky.Win32.Tests {
  // Each test uses its own name in the session namespace, never the real global one.
  public sealed class CloseSignalTests {
    private readonly string _name = $@"Local\Navpaky.Tests.{Guid.NewGuid():N}";

    [Fact]
    public void NotRaisedWithoutTheEvent() {
      Assert.False(CloseSignal.IsRaised(_name));
    }

    [Fact]
    public void RaisedByAnAdministrator() {
      Assert.SkipUnless(IsElevated(), "Raising needs an elevated test run.");

      using IDisposable signal = CloseSignal.Raise(_name);

      Assert.True(CloseSignal.IsRaised(_name));
    }

    [Fact]
    public void GoneOnceTheRaiserCloses() {
      Assert.SkipUnless(IsElevated(), "Raising needs an elevated test run.");

      CloseSignal.Raise(_name).Dispose();

      Assert.False(CloseSignal.IsRaised(_name));
    }

    // A handle someone keeps open must not keep the request alive after the hook ends it.
    [Fact]
    public void EndsWhenTheRaiserIsDoneEvenIfAnotherHandleStaysOpen() {
      Assert.SkipUnless(IsElevated(), "Raising needs an elevated test run.");
      IDisposable raised = CloseSignal.Raise(_name);
      EventWaitHandle? kept = null;
      try {
        Assert.True(EventWaitHandle.TryOpenExisting(_name, out kept));

        raised.Dispose();

        Assert.False(CloseSignal.IsRaised(_name));
      } finally {
        kept?.Dispose();
        raised.Dispose();
      }
    }

    [Fact]
    public void EndingTwiceIsHarmless() {
      Assert.SkipUnless(IsElevated(), "Raising needs an elevated test run.");
      IDisposable raised = CloseSignal.Raise(_name);

      raised.Dispose();
      raised.Dispose();

      Assert.False(CloseSignal.IsRaised(_name));
    }

    // A squatter can deny everyone the rights the check needs; that must not crash Navpaky.
    [Fact]
    public void IgnoresAnEventItCannotRead() {
      var security = new EventWaitHandleSecurity();
      security.AddAccessRule(new EventWaitHandleAccessRule(
          new SecurityIdentifier("S-1-3-4"),
          EventWaitHandleRights.Synchronize,
          AccessControlType.Allow));
      using EventWaitHandle squatter = EventWaitHandleAcl.Create(
          true, EventResetMode.ManualReset, _name, out _, security);

      Assert.False(CloseSignal.IsRaised(_name));
    }

    // A standard user can create the name first; owned by that user, it must close nothing.
    [Fact]
    public void IgnoresAnEventOwnedByAUser() {
      using var identity = WindowsIdentity.GetCurrent();
      var security = new EventWaitHandleSecurity();
      security.SetOwner(identity.User!);
      security.AddAccessRule(new EventWaitHandleAccessRule(
          new SecurityIdentifier(WellKnownSidType.WorldSid, null),
          EventWaitHandleRights.FullControl,
          AccessControlType.Allow));
      using EventWaitHandle squatter = EventWaitHandleAcl.Create(
          true, EventResetMode.ManualReset, _name, out _, security);

      Assert.False(CloseSignal.IsRaised(_name));
    }

    private static bool IsElevated() {
      using var identity = WindowsIdentity.GetCurrent();
      return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
  }
}
