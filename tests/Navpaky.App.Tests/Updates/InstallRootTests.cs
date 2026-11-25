using Xunit;

namespace Navpaky.App.Tests.Updates {
  public sealed class InstallRootTests {
    // A single-file build that extracts native libraries runs from <root>\current, while its
    // base directory is the extraction folder in %TEMP% (found in the add-install-wizard VM run).
    [Fact]
    public void TheRootIsTheFolderAboveTheExecutablesFolder() {
      Assert.Equal(
          @"C:\Users\Invented\AppData\Local\Programs\Navpaky",
          Program.InstallRoot(
              @"C:\Users\Invented\AppData\Local\Programs\Navpaky\current\Navpaky.App.exe"));
    }
  }
}
