using System.IO;
using Rekey.App.Onboarding;
using Xunit;

namespace Rekey.App.Tests.Onboarding {
  public sealed class InstallSourceTests {
    private const string _folder = @"C:\Users\Invented\Downloads\Rekey";

    [Fact]
    public void BundledSetupIsRun() {
      string setup = Path.Join(_folder, InstallSource.SetupName);

      Assert.Equal(setup, InstallSource.For(_folder, path => path == setup));
    }

    [Fact]
    public void WithoutASetupTheReleasesPageOpens() {
      Assert.Equal(InstallSource.ReleasesPage, InstallSource.For(_folder, _ => false));
    }
  }
}
