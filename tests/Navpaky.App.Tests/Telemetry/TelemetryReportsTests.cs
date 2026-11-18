using System.Text.Json;
using Navpaky.App.Telemetry;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;
using Navpaky.Core.Telemetry.Schemas;
using Xunit;

namespace Navpaky.App.Tests.Telemetry {
  // Spec telemetry: "Daily Usage Digest" and "One-time setup signal" content.
  public sealed class TelemetryReportsTests {
    private static readonly DateOnly _day = new(2026, 11, 15);
    private static readonly LayoutId[] _layouts = [new(0x04090409), new(0x04190419)];

    [Fact]
    public void DigestCarriesTheCountersOfThatDayOnly() {
      var stats = new StatsDocument([
          DayStats.Empty(_day.AddDays(-1)).Add(StatCounter.Undos),
          DayStats.Empty(_day).Add(StatCounter.WordsAnalysed).Add(StatCounter.WordsAnalysed)]);

      using JsonDocument json = JsonDocument.Parse(
          TelemetryReports.Digest(_day, new Settings(), stats, _layouts));

      JsonElement counters = json.RootElement.GetProperty("counters");
      Assert.Equal(2, counters.GetProperty("analysed").GetInt64());
      Assert.Equal(0, counters.GetProperty("undos").GetInt64());
      Assert.Equal("2026-11-15", json.RootElement.GetProperty("day").GetString());
      Assert.Equal(
          ["en", "ru"],
          json.RootElement.GetProperty("environment").GetProperty("layoutLanguages")
              .EnumerateArray().Select(code => code.GetString()));
    }

    [Fact]
    public void EachDigestHasItsOwnRequestId() {
      string first = TelemetryReports.Digest(_day, new Settings(), StatsDocument.Empty, []);
      string second = TelemetryReports.Digest(_day, new Settings(), StatsDocument.Empty, []);

      Assert.NotEqual(RequestId(first), RequestId(second));
    }

    [Fact]
    public void SetupSignalCountsLayoutsAndCoveredLanguages() {
      // English (UK) shares the English profile with English (US); Turkish has none.
      LayoutId[] layouts = [.. _layouts, new(0x08090809), new(0x041F041F)];

      using JsonDocument json = JsonDocument.Parse(TelemetryReports.Setup(
          new Settings(), layouts, ProfileOf, new(true, false)));

      Assert.Equal(4, json.RootElement.GetProperty("layoutCount").GetInt32());
      Assert.Equal(2, json.RootElement.GetProperty("coveredLanguageCount").GetInt32());
      Assert.True(json.RootElement.GetProperty("progress").GetProperty("triedCorrection")
          .GetBoolean());
    }

    private static Language? ProfileOf(LayoutId layout) {
      return layout.LanguageId switch {
        0x0409 or 0x0809 => Language.English,
        0x0419 => Language.Russian,
        _ => null,
      };
    }

    private static string RequestId(string json) {
      using JsonDocument document = JsonDocument.Parse(json);
      return document.RootElement.GetProperty("requestId").GetString()!;
    }
  }
}
