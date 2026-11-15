using System.IO;
using System.Security.Cryptography;
using Velopack;
using Velopack.Locators;
using Velopack.Logging;
using Velopack.Sources;

namespace Rekey.App.Tests.Updates {
  // A release feed and an installed Rekey in a temporary folder, for UpdateManager tests.
  internal sealed class FakeFeed : IUpdateSource, IDisposable {
    private readonly string _root = Path.Join(
        Path.GetTempPath(), "rekey-tests", Guid.NewGuid().ToString("N"));

    private readonly Dictionary<string, string> _files = [];
    private readonly List<VelopackAsset> _assets = [];

    public FakeFeed(string runningVersion) {
      Directory.CreateDirectory(Path.Join(_root, "packages"));
      Directory.CreateDirectory(Path.Join(_root, "current"));
      Locator = new TestVelopackLocator(
          "key-switcher", runningVersion, Path.Join(_root, "packages"),
          Path.Join(_root, "current"), _root, Path.Join(_root, "Update.exe"), "win",
          NullVelopackLogger.Instance, null, Path.Join(_root, "current", "Rekey.App.exe"));
    }

    public IVelopackLocator Locator { get; }

    public string PackagesDir => Path.Join(_root, "packages");

    // Lists a full package; `served` is what the download delivers, the real file when null.
    public void Publish(string version, string? package = null, string? served = null) {
      package ??= Path.Join(_root, $"source-{version}.nupkg");
      if (!File.Exists(package)) {
        File.WriteAllBytes(package, [1, 2, 3]);
      }

      byte[] bytes = File.ReadAllBytes(package);
      string name = $"key-switcher-{version}-full.nupkg";
      _assets.Add(new VelopackAsset {
        PackageId = "key-switcher",
        Version = SemanticVersion.Parse(version),
        Type = VelopackAssetType.Full,
        FileName = name,
        SHA256 = Convert.ToHexString(SHA256.HashData(bytes)),
        Size = bytes.Length,
      });
      _files[name] = served ?? package;
    }

    public Task<VelopackAssetFeed> GetReleaseFeed(
        IVelopackLogger logger, string? appId, string channel, Guid? stagingId = null,
        VelopackAsset? latestLocalRelease = null) {
      return Task.FromResult(new VelopackAssetFeed { Assets = [.. _assets] });
    }

    public Task DownloadReleaseEntry(
        IVelopackLogger logger, VelopackAsset releaseEntry, string localFile,
        Action<int> progress, CancellationToken cancelToken = default) {
      File.Copy(_files[releaseEntry.FileName], localFile, overwrite: true);
      return Task.CompletedTask;
    }

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }
  }
}
