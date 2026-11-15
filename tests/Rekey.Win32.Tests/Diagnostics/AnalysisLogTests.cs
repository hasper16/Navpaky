using System.Security.AccessControl;
using System.Security.Principal;
using Rekey.Core;
using Rekey.Core.Detection;
using Rekey.Core.Languages;
using Rekey.Win32.Diagnostics;
using Xunit;

namespace Rekey.Win32.Tests.Diagnostics {
  // Spec statistics-and-diagnostics: the word-analysis log is opt-in, keeps 7 days, is deleted
  // when turned off, and only the current user (and SYSTEM) may read it (design D4).
  public sealed class AnalysisLogTests : IDisposable {
    private static readonly LayoutId _english = new(0x04090409);
    private static readonly LayoutId _russian = new(0x04190419);
    private static readonly LogAnalysis _entry = new(
        "ghbdtn", _english, [new TargetRendering(_russian, "привет")], DecisionKind.Correct,
        ProtectedWord: false);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("rekey-analysis-");
    private readonly ManualTime _time = new();
    private readonly string _folder;

    public AnalysisLogTests() {
      _folder = Path.Join(_root.FullName, "analysis");
    }

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void OffWritesNoFile() {
      var log = new AnalysisLog(_folder, _time);

      log.Write(_entry);

      Assert.False(Directory.Exists(_folder) && Directory.EnumerateFiles(_folder).Any());
    }

    [Fact]
    public void OnWritesTheWordItsRenderingsAndTheDecision() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };

      log.Write(_entry);
      log.Write(_entry with { Decision = DecisionKind.Keep, ProtectedWord = true });

      IReadOnlyList<string> lines = AnalysisLog.ReadAll(_folder);
      Assert.Equal(2, lines.Count);
      Assert.Contains("ghbdtn", lines[0], StringComparison.Ordinal);
      Assert.Contains("04190419:привет", lines[0], StringComparison.Ordinal);
      Assert.EndsWith("Correct", lines[0], StringComparison.Ordinal);
      Assert.EndsWith("Keep (Protected Word)", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void EachDayHasItsOwnFile() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };

      log.Write(_entry);
      _time.Advance(TimeSpan.FromDays(1));
      log.Write(_entry);

      Assert.Equal(2, Directory.GetFiles(_folder, "analysis-*.log").Length);
      Assert.Equal(2, AnalysisLog.ReadAll(_folder).Count);
    }

    [Fact]
    public void EntriesOlderThan7DaysAreDeletedWhenTheDayChanges() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };
      log.Write(_entry);

      _time.Advance(TimeSpan.FromDays(6));
      log.Write(_entry);
      Assert.Equal(2, AnalysisLog.ReadAll(_folder).Count);

      _time.Advance(TimeSpan.FromDays(1));
      log.Write(_entry);
      Assert.Equal(2, AnalysisLog.ReadAll(_folder).Count);
    }

    [Fact]
    public void EntriesOlderThan7DaysAreDeletedOnStart() {
      new AnalysisLog(_folder, _time) { Enabled = true }.Write(_entry);
      _time.Advance(TimeSpan.FromDays(7));

      _ = new AnalysisLog(_folder, _time) { Enabled = true };

      Assert.Empty(AnalysisLog.ReadAll(_folder));
    }

    [Fact]
    public void TurningOffDeletesTheLog() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };
      log.Write(_entry);

      log.Enabled = false;

      Assert.Empty(Directory.GetFiles(_folder));
    }

    [Fact]
    public void TurningOffDeletesEveryFileItCanAndRetriesTheRest() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };
      log.Write(_entry);
      _time.Advance(TimeSpan.FromDays(1));
      log.Write(_entry);
      string[] files = [.. Directory.GetFiles(_folder).Order(StringComparer.Ordinal)];

      // Another app holds the older day open without delete sharing, as an editor may.
      using (new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read)) {
        log.Enabled = false;

        Assert.Equal([files[0]], Directory.GetFiles(_folder));
        Assert.True(log.HasUndeletedFiles);
      }

      log.RetryDeletes();

      Assert.Empty(Directory.GetFiles(_folder));
      Assert.False(log.HasUndeletedFiles);
    }

    [Fact]
    public void StartingWithTheLogOffDeletesWhatIsLeft() {
      new AnalysisLog(_folder, _time) { Enabled = true }.Write(_entry);

      _ = new AnalysisLog(_folder, _time) { Enabled = false };

      Assert.Empty(Directory.GetFiles(_folder));
    }

    [Fact]
    public void OnlyTheUserAndSystemMayOpenTheLog() {
      var log = new AnalysisLog(_folder, _time) { Enabled = true };
      log.Write(_entry);

      FileSecurity security = new FileInfo(Directory.GetFiles(_folder).Single())
          .GetAccessControl();
      string[] identities = [.. security
          .GetAccessRules(includeExplicit: true, includeInherited: true, typeof(SecurityIdentifier))
          .Cast<FileSystemAccessRule>()
          .Select(rule => rule.IdentityReference.Value)
          .Distinct()];

      // Windows orders the rules itself.
      string[] expected = [
          WindowsIdentity.GetCurrent().User!.Value,
          new SecurityIdentifier(WellKnownSidType.LocalSystemSid, domainSid: null).Value,
      ];
      Assert.True(security.AreAccessRulesProtected);
      Assert.Equal(
          expected.Order(StringComparer.Ordinal), identities.Order(StringComparer.Ordinal));
    }
  }
}
