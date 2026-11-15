using Rekey.App.Updates;
using Rekey.Win32.Updates;
using Velopack;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class RekeyUpdateManagerTests : IDisposable {
    private readonly FakeFeed _feed = new("1.2.0");

    public void Dispose() {
      _feed.Dispose();
    }

    [Fact]
    public async Task NewerReleaseIsOffered() {
      _feed.Publish("1.3.0");

      UpdateInfo? update = await Manager().CheckForUpdatesAsync();

      Assert.Equal(SemanticVersion.Parse("1.3.0"), update?.TargetFullRelease.Version);
    }

    [Theory]
    [InlineData("1.1.0")]
    [InlineData("1.2.0")]
    public async Task OlderOrSameReleaseIsNotOffered(string latest) {
      _feed.Publish(latest);

      Assert.Null(await Manager().CheckForUpdatesAsync());
    }

    private RekeyUpdateManager Manager() {
      return new RekeyUpdateManager(_feed, _ => SignatureVerdict.Valid, _feed.Locator);
    }
  }
}
