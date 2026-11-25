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

      using EventWaitHandle signal = CloseSignal.Raise(_name);

      Assert.True(CloseSignal.IsRaised(_name));
    }

    [Fact]
    public void GoneOnceTheRaiserCloses() {
      Assert.SkipUnless(IsElevated(), "Raising needs an elevated test run.");

      CloseSignal.Raise(_name).Dispose();

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
