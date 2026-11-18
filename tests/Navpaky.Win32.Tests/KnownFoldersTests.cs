using Xunit;

namespace Navpaky.Win32.Tests {
  public sealed class KnownFoldersTests {
    [Fact]
    public void DownloadsIsAFullPath() {
      string? downloads = KnownFolders.Downloads();

      Assert.NotNull(downloads);
      Assert.True(Path.IsPathFullyQualified(downloads));
    }
  }
}
