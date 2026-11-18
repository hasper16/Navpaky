using Navpaky.App.Updates;
using Velopack;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class VersionGateTests {
    [Theory]
    [InlineData("1.2.3", "1.2.4")]
    [InlineData("1.2.3", "2.0.0")]
    [InlineData("1.2.3-beta.1", "1.2.3")]
    public void NewerVersionIsAllowed(string running, string candidate) {
      Assert.True(
          VersionGate.Allows(SemanticVersion.Parse(running), SemanticVersion.Parse(candidate)));
    }

    [Theory]
    [InlineData("1.2.3", "1.2.3")]
    [InlineData("1.2.3", "1.2.2")]
    [InlineData("1.2.3", "1.2.3-beta.1")]
    [InlineData("2.0.0", "1.9.9")]
    public void LowerOrEqualVersionIsRejected(string running, string candidate) {
      Assert.False(
          VersionGate.Allows(SemanticVersion.Parse(running), SemanticVersion.Parse(candidate)));
    }

    [Fact]
    public void NothingIsAllowedWithoutARunningVersion() {
      Assert.False(VersionGate.Allows(null, SemanticVersion.Parse("1.0.0")));
    }
  }
}
