using System.Text.Json;
using Rekey.Core.Diagnostics;
using Rekey.Core.Languages;
using Xunit;

namespace Rekey.Core.Tests.Diagnostics {
  // Spec statistics-and-diagnostics: a local log of the last 500 Incidents; design D3: JSON
  // lines with time, type and details, never a window title.
  public sealed class IncidentLogTests : IDisposable {
    private static readonly DateTimeOffset _monday = new(2026, 11, 16, 9, 0, 0, TimeSpan.Zero);

    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("rekey-incidents-");
    private readonly string _folder;

    public IncidentLogTests() {
      _folder = Path.Join(_root.FullName, "Rekey");
    }

    public void Dispose() {
      _root.Delete(recursive: true);
    }

    [Fact]
    public void HookReinstalledIsAddedWithItsTime() {
      var log = new IncidentLog(_folder);

      log.Append(new Incident(_monday, IncidentType.HookReinstalled, []));

      IncidentEntry entry = Assert.Single(new IncidentLog(_folder).Read());
      Assert.Equal(_monday, entry.Time);
      Assert.Equal(IncidentType.HookReinstalled, entry.Type);
      Assert.Empty(entry.Details);
    }

    [Fact]
    public void DetailsAreKeptAsEnumNames() {
      var log = new IncidentLog(_folder);

      log.Append(new Incident(_monday, IncidentType.LanguageProfileSkipped, [Language.Russian]));

      Assert.Equal(["Language.Russian"], Assert.Single(log.Read()).Details);
    }

    [Fact]
    public void LogIsJsonLinesWithOnlyTimeTypeAndDetails() {
      var log = new IncidentLog(_folder);

      log.Append(new Incident(_monday, IncidentType.SwitchNotLanded, []));
      log.Append(new Incident(_monday, IncidentType.HoldTimeout, []));

      Assert.Equal(Path.Join(_folder, "incidents.jsonl"), log.FilePath);
      string[] lines = File.ReadAllLines(log.FilePath);
      Assert.Equal(2, lines.Length);
      using JsonDocument line = JsonDocument.Parse(lines[0]);
      Assert.Equal(
          ["time", "type", "details"],
          line.RootElement.EnumerateObject().Select(property => property.Name));
      Assert.Equal("SwitchNotLanded", line.RootElement.GetProperty("type").GetString());
    }

    [Fact]
    public void The501stIncidentRemovesTheOldest() {
      var log = new IncidentLog(_folder);
      for (int i = 0; i < 501; i++) {
        log.Append(new Incident(_monday.AddMinutes(i), IncidentType.HookReinstalled, []));
      }

      IReadOnlyList<IncidentEntry> entries = new IncidentLog(_folder).Read();

      Assert.Equal(500, entries.Count);
      Assert.Equal(_monday.AddMinutes(1), entries[0].Time);
      Assert.Equal(_monday.AddMinutes(500), entries[^1].Time);
    }

    [Fact]
    public void LimitHoldsAcrossRestarts() {
      for (int i = 0; i < 300; i++) {
        new IncidentLog(_folder).Append(
            new Incident(_monday.AddMinutes(i), IncidentType.SelfPaused, []));
      }

      var log = new IncidentLog(_folder);
      for (int i = 300; i < 600; i++) {
        log.Append(new Incident(_monday.AddMinutes(i), IncidentType.SelfPaused, []));
      }

      IReadOnlyList<IncidentEntry> entries = log.Read();
      Assert.Equal(500, entries.Count);
      Assert.Equal(_monday.AddMinutes(100), entries[0].Time);
    }

    [Fact]
    public void UnreadableLinesAreSkipped() {
      Directory.CreateDirectory(_folder);
      var log = new IncidentLog(_folder);
      File.WriteAllLines(log.FilePath, ["{broken", "{\"type\":\"NoSuchType\"}"]);

      log.Append(new Incident(_monday, IncidentType.InputFrozen, []));

      Assert.Equal(IncidentType.InputFrozen, Assert.Single(log.Read()).Type);
    }

    [Fact]
    public void MissingLogReadsEmpty() {
      Assert.Empty(new IncidentLog(_folder).Read());
    }

    [Fact]
    public void EveryIncidentTypeIsPersisted() {
      var log = new IncidentLog(_folder);
      IncidentType[] types = Enum.GetValues<IncidentType>();

      foreach (IncidentType type in types) {
        log.Append(new Incident(_monday, type, []));
      }

      Assert.Equal(types, new IncidentLog(_folder).Read().Select(entry => entry.Type));
    }
  }
}
