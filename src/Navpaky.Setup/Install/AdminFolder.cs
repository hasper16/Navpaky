using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Navpaky.Setup.Install {
  // A folder only administrators can write to and the current user can read (design D2 of
  // add-machine-wide-install). Only an elevated process can create it: the owner it sets is
  // Administrators.
  internal static class AdminFolder {
    public static bool TryCreate(string path) {
      var security = new DirectorySecurity();
      var administrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
      security.SetOwner(administrators);
      security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
      security.AddAccessRule(Rule(administrators, FileSystemRights.FullControl));
      security.AddAccessRule(Rule(
          new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
          FileSystemRights.FullControl));
      using (var me = WindowsIdentity.GetCurrent()) {
        security.AddAccessRule(Rule(me.User!, FileSystemRights.ReadAndExecute));
      }

      try {
        new DirectoryInfo(path).Create(security);

        // Creating keeps the security of a folder that is already there: check whose it is.
        IdentityReference? owner = new DirectoryInfo(path).GetAccessControl()
            .GetOwner(typeof(SecurityIdentifier));
        return administrators.Equals(owner);
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        System.Diagnostics.Trace.WriteLine($"Work folder not created: {e.GetType().Name}");
        return false;
      }
    }

    private static FileSystemAccessRule Rule(IdentityReference who, FileSystemRights rights) {
      return new FileSystemAccessRule(
          who,
          rights,
          InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
          PropagationFlags.None,
          AccessControlType.Allow);
    }
  }
}
