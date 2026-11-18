using Navpaky.Core.Diagnostics;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;
using Xunit;

namespace Navpaky.Core.Tests.Diagnostics {
  // Spec statistics-and-diagnostics: the export holds versions, Layouts, settings, counters and
  // Incidents, no typed text unless the user adds the word-analysis log, and masks personal
  // paths and names (design D5).
  public sealed class DiagnosticsReportTests {
    private const string _analysisLine =
        "2026-11-16 09:00:00.000\t04090409:ghbdtn\t04190419:привет\tCorrect";

    // The same line as the export shows it.
    private const string _exportedLine =
        "2026-11-16 09:00:00.000  04090409:ghbdtn  04190419:привет  Correct";

    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);
    private static readonly PersonalNames _anna = new(@"C:\Users\anna", "anna", "ANNA-PC");

    [Fact]
    public void ExportHoldsVersionsLayoutsSettingsCountersAndIncidents() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: false);

      Assert.Contains("Navpaky 1.2.3", report, StringComparison.Ordinal);
      Assert.Contains("Windows 10.0.26100", report, StringComparison.Ordinal);
      Assert.Contains("04190419 (RU)", report, StringComparison.Ordinal);
      Assert.Contains("sensitivity: balanced", report, StringComparison.Ordinal);
      Assert.Contains("Auto Corrections: 4", report, StringComparison.Ordinal);
      Assert.Contains("HookReinstalled", report, StringComparison.Ordinal);
      Assert.Contains("Language.Russian", report, StringComparison.Ordinal);
    }

    [Fact]
    public void DefaultExportLeavesTheWordAnalysisLogOut() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: false);

      Assert.DoesNotContain("ghbdtn", report, StringComparison.Ordinal);
      Assert.DoesNotContain("привет", report, StringComparison.Ordinal);
    }

    [Fact]
    public void IncludedWordAnalysisLogIsExported() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: true);

      Assert.Contains(_exportedLine, report, StringComparison.Ordinal);
    }

    // Fix 2.9 of add-final-acceptance: tab-separated lines ran together in the preview.
    [Fact]
    public void WordAnalysisLinesShowInAlignedColumns() {
      DiagnosticsFacts facts = Facts() with {
        AnalysisLog = [
            "2026-11-16 09:00:00.000\t04090409:ghbdtn\t04190419:привет\tCorrect",
            "2026-11-16 09:00:01.000\t04090409:ok\t04190419:щл\tKeep",
        ],
      };

      string report = DiagnosticsReport.Build(facts, _anna, includeAnalysisLog: true);

      string[] lines = [.. report.Split(Environment.NewLine)
          .Where(line => line.StartsWith("2026-11-16 09:00:0", StringComparison.Ordinal))];
      Assert.Equal(2, lines.Length);
      Assert.DoesNotContain('\t', lines[0]);
      Assert.Equal(lines[0].IndexOf("Correct", StringComparison.Ordinal),
          lines[1].IndexOf("Keep", StringComparison.Ordinal));
      Assert.Equal(lines[0].IndexOf("04190419", StringComparison.Ordinal),
          lines[1].IndexOf("04190419", StringComparison.Ordinal));
    }

    [Fact]
    public void ProfilePathUserNameAndMachineNameAreMasked() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: true);

      Assert.Contains(@"%USERPROFILE%\AppData\Local\Navpaky", report, StringComparison.Ordinal);
      Assert.Contains(@"D:\Work\%USERNAME%\tool.exe", report, StringComparison.Ordinal);
      Assert.Contains(@"\\%COMPUTERNAME%\share", report, StringComparison.Ordinal);
      Assert.DoesNotContain("anna", report, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UserNameInsideAnotherWordIsKept() {
      var shortName = new PersonalNames(@"C:\Users\an", "an", "PC");

      string report = DiagnosticsReport.Build(Facts(), shortName, includeAnalysisLog: false);

      Assert.Contains("Language.Russian", report, StringComparison.Ordinal);
      Assert.Contains("sensitivity: balanced", report, StringComparison.Ordinal);
    }

    private static DiagnosticsFacts Facts() {
      var settings = new Settings(ExcludedApps: new ExcludedAppList([
          @"C:\Users\anna\AppData\Local\Navpaky\helper.exe",
          @"D:\Work\anna\tool.exe",
          @"\\ANNA-PC\share\app.exe",
      ]));
      var stats = new StatsDocument([
          DayStats.Empty(DateOnly.FromDateTime(_monday.Date))
              .Add(StatCounter.AutoCorrections).Add(StatCounter.AutoCorrections)
              .Add(StatCounter.AutoCorrections).Add(StatCounter.AutoCorrections),
      ]);
      return new DiagnosticsFacts(
          _monday,
          "1.2.3",
          "10.0.26100",
          [new LayoutId(0x04090409), new LayoutId(0x04190419)],
          settings,
          stats,
          [
            new IncidentEntry(_monday, IncidentType.HookReinstalled, []),
            new IncidentEntry(_monday, IncidentType.LanguageProfileSkipped, ["Language.Russian"]),
          ],
          [_analysisLine]);
    }
  }
}
