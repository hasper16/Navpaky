using Microsoft.Win32;
using Xunit;

namespace Navpaky.Win32.Tests {
  // Runs against a throwaway key under HKCU\Software, never the real Run key.
  public sealed class SignInLaunchTests : IDisposable {
    private const string _exe = @"C:\Users\someone\AppData\Local\Navpaky\Navpaky.exe";

    private readonly string _root = $@"Software\Navpaky.Tests\{Guid.NewGuid():N}";
    private readonly SignInLaunch _launch;

    public SignInLaunchTests() {
      _launch = new SignInLaunch(Registry.CurrentUser, _root, "Navpaky");
    }

    public void Dispose() {
      Registry.CurrentUser.DeleteSubKeyTree(_root, throwOnMissingSubKey: false);
    }

    [Fact]
    public void OffWithoutARunEntry() {
      Assert.False(_launch.IsOn());
    }

    [Fact]
    public void TurningOnWritesTheQuotedPath() {
      _launch.Set(true, _exe);

      Assert.True(_launch.IsOn());
      using RegistryKey run = Registry.CurrentUser.OpenSubKey(_launch.RunKey)!;
      Assert.Equal($"\"{_exe}\"", run.GetValue("Navpaky"));
    }

    [Fact]
    public void TurningOffRemovesTheEntry() {
      _launch.Set(true, _exe);

      _launch.Set(false, _exe);

      Assert.False(_launch.IsOn());
    }

    [Fact]
    public void DisabledInTaskManagerCountsAsOff() {
      _launch.Set(true, _exe);
      WriteApproved([0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

      Assert.False(_launch.IsOn());
    }

    [Fact]
    public void EnabledInTaskManagerCountsAsOn() {
      _launch.Set(true, _exe);
      WriteApproved([0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

      Assert.True(_launch.IsOn());
    }

    [Fact]
    public void TurningOnAgainClearsTheTaskManagerDisable() {
      _launch.Set(true, _exe);
      WriteApproved([0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

      _launch.Set(true, _exe);

      Assert.True(_launch.IsOn());
    }

    private void WriteApproved(byte[] value) {
      using RegistryKey key = Registry.CurrentUser.CreateSubKey(_launch.ApprovedKey);
      key.SetValue("Navpaky", value, RegistryValueKind.Binary);
    }
  }
}
