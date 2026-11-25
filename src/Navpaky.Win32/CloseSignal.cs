using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Navpaky.Win32 {
  /// <summary>
  /// The machine-wide request that every Navpaky of an all-users install closes, raised by the
  /// elevated update and uninstall hooks (add-machine-wide-install, design D6).
  /// </summary>
  public static class CloseSignal {
    /// <summary>
    /// The name of the event every session can see.
    /// </summary>
    public const string Name = @"Global\NavpakyCloseForUpdate";

    private const EventWaitHandleRights _readRights =
        EventWaitHandleRights.Synchronize | EventWaitHandleRights.ReadPermissions;

    /// <summary>
    /// Creates the event, owned by Administrators, which only an elevated process can do, and
    /// sets it. The request ends when the returned object is disposed, even while other handles
    /// to the event stay open: only its set state counts, which only administrators can change.
    /// </summary>
    /// <param name="name">The event's name; tests pass their own.</param>
    /// <returns>The object that ends the request when disposed.</returns>
    public static IDisposable Raise(string name) {
      var security = new EventWaitHandleSecurity();
      security.SetOwner(Sid(WellKnownSidType.BuiltinAdministratorsSid));
      security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
      security.AddAccessRule(
          Rule(WellKnownSidType.LocalSystemSid, EventWaitHandleRights.FullControl));
      security.AddAccessRule(
          Rule(WellKnownSidType.BuiltinAdministratorsSid, EventWaitHandleRights.FullControl));
      security.AddAccessRule(Rule(WellKnownSidType.AuthenticatedUserSid, _readRights));
      EventWaitHandle signal = EventWaitHandleAcl.Create(
          false, EventResetMode.ManualReset, name, out _, security);
      signal.Set();
      return new Raised(signal);
    }

    /// <summary>
    /// Returns whether an administrator asked every Navpaky to close. An event of that name owned
    /// by anyone else is ignored, so a standard user cannot close other users' Navpaky.
    /// </summary>
    /// <param name="name">The event's name; tests pass their own.</param>
    /// <returns><see langword="true" /> when the request is raised.</returns>
    public static bool IsRaised(string name) {
      EventWaitHandle? signal;
      try {
        if (!EventWaitHandleAcl.TryOpenExisting(name, _readRights, out signal)) {
          return false;
        }
      } catch (UnauthorizedAccessException) {
        // Someone else's event that denies us the check: not an administrator's request.
        return false;
      }

      using (signal) {
        IdentityReference? owner = signal.GetAccessControl().GetOwner(typeof(SecurityIdentifier));
        bool trusted = owner is SecurityIdentifier sid
            && (sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)
                || sid.IsWellKnown(WellKnownSidType.LocalSystemSid));
        return trusted && signal.WaitOne(0);
      }
    }

    private static SecurityIdentifier Sid(WellKnownSidType type) {
      return new SecurityIdentifier(type, null);
    }

    private static EventWaitHandleAccessRule Rule(
        WellKnownSidType type, EventWaitHandleRights rights) {
      return new EventWaitHandleAccessRule(Sid(type), rights, AccessControlType.Allow);
    }

    // Ends the request before closing this handle: a handle another process keeps open must not
    // extend it.
    private sealed class Raised(EventWaitHandle signal) : IDisposable {
      private int _ended;

      public void Dispose() {
        if (Interlocked.Exchange(ref _ended, 1) == 0) {
          signal.Reset();
          signal.Dispose();
        }
      }
    }
  }
}
