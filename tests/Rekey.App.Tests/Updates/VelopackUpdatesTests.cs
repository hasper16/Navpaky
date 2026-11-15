using System.IO;
using Rekey.App.Updates;
using Xunit;

namespace Rekey.App.Tests.Updates {
  public sealed class VelopackUpdatesTests : IDisposable {
    private readonly FakeFeed _feed = new("1.2.0");

    public void Dispose() {
      _feed.Dispose();
    }

    [Fact]
    public async Task NewerReleaseIsDownloadedAndWaitsForRestart() {
      _feed.Publish("1.3.0");

      UpdateStatus status = await Updates().CheckAndDownloadAsync();

      Assert.Equal(UpdateStatus.ReadyToRestart, status);
      Assert.True(File.Exists(Path.Join(_feed.PackagesDir, "key-switcher-1.3.0-full.nupkg")));
    }

    [Fact]
    public async Task NoNewerReleaseIsUpToDate() {
      _feed.Publish("1.2.0");

      Assert.Equal(UpdateStatus.UpToDate, await Updates().CheckAndDownloadAsync());
    }

    [Fact]
    public async Task FailedDownloadFails() {
      _feed.Publish("1.3.0", served: Path.Join(_feed.PackagesDir, "missing.nupkg"));

      Assert.Equal(UpdateStatus.Failed, await Updates().CheckAndDownloadAsync());
    }

    private VelopackUpdates Updates() {
      return new VelopackUpdates(new RekeyUpdateManager(_feed, _feed.Locator), () => { });
    }
  }
}
