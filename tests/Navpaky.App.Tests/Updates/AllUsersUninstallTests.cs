using System.Diagnostics;
using System.IO;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class AllUsersUninstallTests {
    private const string _root = @"C:\Program Files\Navpaky";
    private static readonly string _self = Path.Join(_root, "current", "Navpaky.App.exe");

    // A per-user copy must never ask for elevation to run an Update.exe its user can replace.
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void OnlyAnAllUsersInstallTakesTheArgument(bool machine, bool expected) {
      Assert.Equal(expected, AllUsersUninstall.Applies([AllUsersUninstall.Argument], machine));
    }

    [Fact]
    public void OtherArgumentsAreNotTheUninstall() {
      Assert.False(AllUsersUninstall.Applies([AllUsersUninstall.Argument, "x"], machine: true));
      Assert.False(AllUsersUninstall.Applies([], machine: true));
    }

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
