using System.Diagnostics;
using System.Text.Json;
using Xunit;

namespace Navpaky.Core.Tests.Telemetry {
  public sealed class PathMapTests {
    [Fact]
    public void StackTraceShowsRepositoryRelativePaths() {
      JsonException thrown = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
          "{ \"excludedApps\": 5 }", SettingsJsonContext.Default.Settings));

      string[] files = [.. new StackTrace(thrown, fNeedFileInfo: true).GetFrames()
          .Select(frame => frame.GetFileName())
          .OfType<string>()];

      string core = Assert.Single(
          files, file => file.EndsWith("ExcludedAppList.cs", StringComparison.Ordinal));
      Assert.Equal(
          "/_/src/Navpaky.Core/ExcludedAppList.cs", core.Replace(Path.DirectorySeparatorChar, '/'));
    }
  }
}
