using Rekey.Core.Diagnostics;
using Rekey.Core.Languages;
using Rekey.Core.Statistics;
using Xunit;

namespace Rekey.Core.Tests.Diagnostics {
  // Spec statistics-and-diagnostics: the export holds versions, Layouts, settings, counters and
  // Incidents, no typed text unless the user adds the word-analysis log, and masks personal
  // paths and names (design D5).
  public sealed class DiagnosticsReportTests {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);
    private static readonly PersonalNames _anna = new(@"C:\Users\anna", "anna", "ANNA-PC");
    private static readonly string _analysisLine =
        "2026-11-16 09:00:00.000\t04090409:ghbdtn\t04190419:привет\tCorrect";

    [Fact]
    public void ExportHoldsVersionsLayoutsSettingsCountersAndIncidents() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: false);

      Assert.Contains("Rekey 1.2.3", report, StringComparison.Ordinal);
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

      Assert.Contains(_analysisLine, report, StringComparison.Ordinal);
    }

    [Fact]
    public void ProfilePathUserNameAndMachineNameAreMasked() {
      string report = DiagnosticsReport.Build(Facts(), _anna, includeAnalysisLog: true);

      Assert.Contains(@"%USERPROFILE%\AppData\Local\Rekey", report, StringComparison.Ordinal);
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
          @"C:\Users\anna\AppData\Local\Rekey\helper.exe",
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
