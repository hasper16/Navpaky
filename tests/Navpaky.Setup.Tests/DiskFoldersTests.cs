using System.IO;
using Navpaky.Setup.Install;
using Xunit;

namespace Navpaky.Setup.Tests {
  public sealed class DiskFoldersTests : IDisposable {
    private readonly string _root = Directory.CreateTempSubdirectory("navpaky-disk").FullName;
    private readonly DiskFolders _disk = new();

    public void Dispose() {
      Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void AMissingFolderIsWritableThroughItsParentAndLeavesNoProbe() {
      Assert.True(_disk.CanWrite(Path.Join(_root, "Tools", "Navpaky")));
      Assert.Empty(Directory.EnumerateFileSystemEntries(_root));
    }

    [Fact]
    public void AFolderWithAFileHasEntries() {
      File.WriteAllText(Path.Join(_root, "keep-me.txt"), string.Empty);

      Assert.True(_disk.HasEntries(_root));
      Assert.False(_disk.HasEntries(Path.Join(_root, "missing")));
    }

    [Fact]
    public void FreeSpaceIsReadFromTheDrive() {
      Assert.True(_disk.FreeBytes(Path.Join(_root, "missing")) > 0);
      Assert.Equal(0, _disk.FreeBytes(@"\\nowhere\share\Navpaky"));
    }
  }
}
