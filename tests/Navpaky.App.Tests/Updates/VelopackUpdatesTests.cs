using System.IO;
using Navpaky.App.Updates;
using Navpaky.Core;
using Navpaky.Core.Tests;
using Navpaky.Win32.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class VelopackUpdatesTests : IDisposable {
    private readonly FakeFeed _feed = new("1.2.0");
    private readonly List<Incident> _incidents = [];
    private readonly List<string> _verified = [];
    private SignatureVerdict _verdict = SignatureVerdict.Valid;

    public void Dispose() {
      _feed.Dispose();
    }

    [Fact]
    public async Task NewerReleaseIsDownloadedAndWaitsForRestart() {
      _feed.Publish("1.3.0");

      UpdateStatus status = await Updates().CheckAndDownloadAsync();

      Assert.Equal(UpdateStatus.ReadyToRestart, status);
      Assert.True(File.Exists(Path.Join(_feed.PackagesDir, "navpaky-app-1.3.0-full.nupkg")));
      Assert.Empty(_incidents);
    }

    [Fact]
    public async Task UpdaterThatCannotStartKeepsNavpakyRunning() {
      _feed.Publish("1.3.0");
      int exits = 0;
      var manager = new NavpakyUpdateManager(_feed, Verify, _feed.Locator);
      var updates = new VelopackUpdates(manager, () => exits++, _incidents.Add, new ManualClock());
      await updates.CheckAndDownloadAsync();

      updates.RestartToUpdate();

      Assert.Equal(0, exits);
    }

    [Fact]
    public async Task SignatureIsCheckedOnTheDownloadedPackage() {
      _feed.Publish("1.3.0");

      await Updates().CheckAndDownloadAsync();

      string checkedFile = Assert.Single(_verified);
      Assert.StartsWith(_feed.PackagesDir, checkedFile, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public async Task TamperedPackageIsDiscardedAndRecorded() {
      string other = Path.Join(_feed.PackagesDir, "..", "other.nupkg");
      await File.WriteAllBytesAsync(
          other, [9, 9, 9], TestContext.Current.CancellationToken);
      _feed.Publish("1.3.0", served: other);

      UpdateStatus status = await Updates().CheckAndDownloadAsync();

      Assert.Equal(UpdateStatus.Failed, status);
      Assert.Empty(Directory.GetFiles(_feed.PackagesDir, "*.nupkg*"));
      Incident incident = Assert.Single(_incidents);
      Assert.Equal(IncidentType.UpdateChecksumFailed, incident.Type);
      Assert.Empty(_verified);
    }

    [Theory]
    [InlineData(nameof(SignatureVerdict.NotSigned))]
    [InlineData(nameof(SignatureVerdict.WrongPublisher))]
    [InlineData(nameof(SignatureVerdict.Revoked))]
    public async Task PackageWithoutOurSignatureIsDiscardedAndRecorded(string verdict) {
      _verdict = Enum.Parse<SignatureVerdict>(verdict);
      _feed.Publish("1.3.0");

      UpdateStatus status = await Updates().CheckAndDownloadAsync();

      Assert.Equal(UpdateStatus.Failed, status);
      Assert.Empty(Directory.GetFiles(_feed.PackagesDir, "*.nupkg*"));
      Incident incident = Assert.Single(_incidents);
      Assert.Equal(IncidentType.UpdateSignatureFailed, incident.Type);
      Assert.Equal([_verdict], incident.Details);
    }

    private VelopackUpdates Updates() {
      var manager = new NavpakyUpdateManager(_feed, Verify, _feed.Locator);
      return new VelopackUpdates(manager, () => { }, _incidents.Add, new ManualClock());
    }

    private SignatureVerdict Verify(string package) {
      _verified.Add(package);
      return _verdict;
    }
  }
}
