using System.IO;
using Navpaky.App.Updates;
using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class InstallMarkerTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-marker").FullName;

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void EveryInstallationWritesANewId() {
      string local = Path.Join(_root, "Navpaky");
      string marker = Path.Join(local, InstallMarker.FileName);

      InstallMarker.Write(local);
      string first = File.ReadAllText(marker);
      InstallMarker.Write(local);

      Assert.NotEmpty(first);
      Assert.NotEqual(first, File.ReadAllText(marker));
    }
  }
}
