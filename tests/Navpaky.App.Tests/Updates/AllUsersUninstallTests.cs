using System.Diagnostics;
using System.IO;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class AllUsersUninstallTests {
    private const string _root = @"C:\Program Files\Navpaky";
    private static readonly string _self = Path.Join(_root, "current", "Navpaky.App.exe");

    [Fact]
    public void UnelevatedAsksToStartItselfElevated() {
      ProcessStartInfo next = AllUsersUninstall.Next(elevated: false, _self, _root);

      Assert.Equal(_self, next.FileName);
      Assert.Equal("runas", next.Verb);
      Assert.True(next.UseShellExecute);
      Assert.Equal([AllUsersUninstall.Argument], next.ArgumentList);
    }

    [Fact]
    public void ElevatedStartsVelopacksUninstall() {
      ProcessStartInfo next = AllUsersUninstall.Next(elevated: true, _self, _root);

      Assert.Equal(Path.Join(_root, "Update.exe"), next.FileName);
      Assert.Equal(["--uninstall"], next.ArgumentList);
      Assert.False(next.UseShellExecute);
      Assert.Equal(string.Empty, next.Verb);
    }
  }
}
