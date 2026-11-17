using Rekey.Core;
using Rekey.Core.Detection;
using Rekey.Core.Languages;
using Rekey.Win32.Diagnostics;
using Xunit;

namespace Rekey.Win32.Tests.Diagnostics {
  // Fix 2.9 of add-final-acceptance (owner decision): the worker hands analysis lines to a
  // writer thread, so a slow disk never holds typing back; the order of writes and of turning
  // the log on and off stays as the worker sent it.
  public sealed class AnalysisLogWriterTests : IDisposable {
    private static readonly LogAnalysis _entry = new(
        "ghbdtn",
        new LayoutId(0x04090409),
        [new TargetRendering(new LayoutId(0x04190419), "привет")],
        DecisionKind.Correct,
        ProtectedWord: false);

    private static readonly TimeSpan _wait = TimeSpan.FromSeconds(10);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("rekey-writer-");
    private readonly string _folder;

    public AnalysisLogWriterTests() {
      _folder = Path.Join(_root.FullName, "analysis");
    }

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void WritesReachTheLogInOrder() {
      using var writer = new AnalysisLogWriter(new AnalysisLog(_folder, new ManualTime()));

      writer.UseEnabled(true);
      writer.Write(_entry);
      writer.Write(_entry with { Decision = DecisionKind.Keep });

      Assert.True(writer.Drain(_wait));
      IReadOnlyList<string> lines = AnalysisLog.ReadAll(_folder);
      Assert.Equal(2, lines.Count);
      Assert.EndsWith("Correct", lines[0], StringComparison.Ordinal);
      Assert.EndsWith("Keep", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void TurningOffAfterWritesDeletesThem() {
      using var writer = new AnalysisLogWriter(new AnalysisLog(_folder, new ManualTime()));

      writer.UseEnabled(true);
      writer.Write(_entry);
      writer.UseEnabled(false);

      Assert.True(writer.Drain(_wait));
      Assert.Empty(AnalysisLog.ReadAll(_folder));
    }
  }
}
