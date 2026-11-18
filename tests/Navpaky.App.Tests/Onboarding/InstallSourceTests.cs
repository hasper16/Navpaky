using System.IO;
using Navpaky.App.Onboarding;
using Xunit;

namespace Navpaky.App.Tests.Onboarding {
  public sealed class InstallSourceTests {
    private const string _folder = @"C:\Users\Invented\Downloads\Navpaky";

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
