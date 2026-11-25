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
    /// Creates the event, owned by Administrators, which only an elevated process can do. It
    /// stays raised until the returned handle is disposed.
    /// </summary>
    /// <param name="name">The event's name; tests pass their own.</param>
    /// <returns>The handle that keeps the request alive.</returns>
    public static EventWaitHandle Raise(string name) {
      var security = new EventWaitHandleSecurity();
      security.SetOwner(Sid(WellKnownSidType.BuiltinAdministratorsSid));
      security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
      security.AddAccessRule(
          Rule(WellKnownSidType.LocalSystemSid, EventWaitHandleRights.FullControl));
      security.AddAccessRule(
          Rule(WellKnownSidType.BuiltinAdministratorsSid, EventWaitHandleRights.FullControl));
      security.AddAccessRule(Rule(WellKnownSidType.AuthenticatedUserSid, _readRights));
      return EventWaitHandleAcl.Create(
          true, EventResetMode.ManualReset, name, out _, security);
    }

    /// <summary>
    /// Returns whether an administrator asked every Navpaky to close. An event of that name owned
    /// by anyone else is ignored, so a standard user cannot close other users' Navpaky.
    /// </summary>
    /// <param name="name">The event's name; tests pass their own.</param>
    /// <returns><see langword="true" /> when the request is raised.</returns>
    public static bool IsRaised(string name) {
      if (!EventWaitHandleAcl.TryOpenExisting(name, _readRights, out EventWaitHandle? signal)) {
        return false;
      }

      using (signal) {
        IdentityReference? owner = signal.GetAccessControl().GetOwner(typeof(SecurityIdentifier));
        return owner is SecurityIdentifier sid
            && (sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid)
                || sid.IsWellKnown(WellKnownSidType.LocalSystemSid));
      }
    }

    private static SecurityIdentifier Sid(WellKnownSidType type) {
      return new SecurityIdentifier(type, null);
    }

    private static EventWaitHandleAccessRule Rule(
        WellKnownSidType type, EventWaitHandleRights rights) {
      return new EventWaitHandleAccessRule(Sid(type), rights, AccessControlType.Allow);
    }
  }
}
