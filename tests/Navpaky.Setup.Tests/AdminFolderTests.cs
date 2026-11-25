using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class AdminFolderTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-admin").FullName;

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void AFolderSomeoneElseMadeFirstIsRefused() {
      string path = Directory.CreateDirectory(Path.Join(_root, "work")).FullName;

      Assert.False(AdminFolder.TryCreate(path));
    }

    [Fact]
    public void ElevatedItIsOwnedByAdministratorsAndReadOnlyForTheUser() {
      using var me = WindowsIdentity.GetCurrent();
      Assert.SkipUnless(
          new WindowsPrincipal(me).IsInRole(WindowsBuiltInRole.Administrator),
          "Creating it needs an elevated test run.");
      string path = Path.Join(_root, "work");

      Assert.True(AdminFolder.TryCreate(path));

      DirectorySecurity security = new DirectoryInfo(path).GetAccessControl();
      Assert.True(security.AreAccessRulesProtected);
      FileSystemAccessRule mine = security
          .GetAccessRules(true, false, typeof(SecurityIdentifier))
          .OfType<FileSystemAccessRule>()
          .Single(rule => rule.IdentityReference.Equals(me.User));
      Assert.Equal(0, (int)(mine.FileSystemRights & FileSystemRights.WriteData));
    }
  }
}
