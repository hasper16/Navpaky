using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Navpaky.Core.Statistics;

namespace Navpaky.Core.Diagnostics {
  /// <summary>
  /// Builds the plain text of a diagnostics export; the preview shows exactly this text.
  /// </summary>
  public static class DiagnosticsReport {
    private const int _monthDays = 30;

    /// <summary>
    /// Returns the export text with personal paths and names masked.
    /// </summary>
    /// <param name="facts">What the export may contain.</param>
    /// <param name="names">The personal names to mask.</param>
    /// <param name="includeAnalysisLog">
    /// <see langword="true" /> to add the word-analysis log, which holds typed text; otherwise,
    /// <see langword="false" />.
    /// </param>
    /// <returns>The export text.</returns>
    public static string Build(
        DiagnosticsFacts facts, PersonalNames names, bool includeAnalysisLog) {
      var text = new StringBuilder();
      string layouts = string.Join(", ", facts.Layouts.Select(
          layout => Invariant($"{layout.Value:X8} ({layout.LanguageCode})")));
      text.AppendLine("Navpaky diagnostics")
          .AppendLine(Invariant($"Exported: {facts.Time:O}"))
          .AppendLine("Navpaky " + facts.AppVersion)
          .AppendLine("Windows " + facts.WindowsVersion)
          .AppendLine("Layouts: " + layouts);
      AppendSettings(text, facts.Settings);
      DateOnly today = DateOnly.FromDateTime(facts.Time.DateTime);
      AppendTotals(text, "Statistics, today", StatsTotals.Of(facts.Statistics, today, 1));
      AppendTotals(
          text,
          "Statistics, last 30 days",
          StatsTotals.Of(facts.Statistics, today, _monthDays));
      text.AppendLine().AppendLine(Invariant($"[Incidents: {facts.Incidents.Count}]"));
      foreach (IncidentEntry incident in facts.Incidents) {
        text.AppendLine(Invariant(
            $"{incident.Time:O} {incident.Type} {string.Join(',', incident.Details)}").TrimEnd());
      }

      if (includeAnalysisLog) {
        text.AppendLine().AppendLine("[Word-analysis log]");
        foreach (string line in Columns(facts.AnalysisLog)) {
          text.AppendLine(line);
        }
      }

      return Mask(text.ToString(), names);
    }

    // The log separates its fields with tabs, which a text box shows unevenly; padding each
    // field to its column's width keeps the columns aligned in a fixed-width font.
    private static IEnumerable<string> Columns(IReadOnlyList<string> lines) {
      string[][] rows = [.. lines.Select(line => line.Split('\t'))];
      int columns = rows.Length == 0 ? 0 : rows.Max(row => row.Length);
      int[] widths = [.. Enumerable.Range(0, columns)
          .Select(column => rows.Max(row => column < row.Length ? row[column].Length : 0))];
      return rows.Select(row => string.Join(
          "  ", row.Select((field, column) => field.PadRight(widths[column]))).TrimEnd());
    }

    // Design D5: the profile path and the machine name first, since they often contain the user
    // name.
    private static string Mask(string text, PersonalNames names) {
      string masked = string.IsNullOrEmpty(names.UserProfile)
          ? text
          : text.Replace(names.UserProfile, "%USERPROFILE%", StringComparison.OrdinalIgnoreCase);
      masked = MaskWord(masked, names.MachineName, "%COMPUTERNAME%");
      return MaskWord(masked, names.UserName, "%USERNAME%");
    }

    // Whole words only, so a short name does not cut into other words.
    private static string MaskWord(string text, string name, string placeholder) {
      if (string.IsNullOrWhiteSpace(name)) {
        return text;
      }

      string pattern = $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(name)}(?![\p{{L}}\p{{N}}])";
      return Regex.Replace(
          text,
          pattern,
          placeholder.Replace("$", "$$", StringComparison.Ordinal),
          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
          TimeSpan.FromSeconds(1));
    }

    // Each setting as "name: value", list items one by one, so paths read as typed and can be
    // masked.
    private static void AppendSettings(StringBuilder text, Settings settings) {
      text.AppendLine().AppendLine("[Settings]");
      string json = JsonSerializer.Serialize(settings, SettingsJsonContext.Default.Settings);
      using JsonDocument document = JsonDocument.Parse(json);
      foreach (JsonProperty property in document.RootElement.EnumerateObject()) {
        text.Append(property.Name).Append(": ").AppendLine(property.Value.ValueKind switch {
          JsonValueKind.Array => string.Join("; ", property.Value.EnumerateArray().Select(ValueOf)),
          _ => ValueOf(property.Value),
        });
      }
    }

    private static string ValueOf(JsonElement value) {
      return value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();
    }

    private static void AppendTotals(StringBuilder text, string title, StatsTotals totals) {
      DurationStats durations = totals.Durations;
      string[] lines = [
          Invariant($"Words analysed: {totals.Count(StatCounter.WordsAnalysed)}"),
          Invariant($"Auto Corrections: {totals.Count(StatCounter.AutoCorrections)}"),
          Invariant($"Manual Corrections: {totals.Count(StatCounter.ManualCorrections)}"),
          Invariant($"Undos: {totals.Undos}"),
          "Undo share: " + Percent(totals.UndoShare),
          Invariant($"Decisions: correct {totals.Count(StatCounter.CorrectDecisions)}, ")
              + Invariant($"defer {totals.Count(StatCounter.DeferDecisions)}, ")
              + Invariant($"keep {totals.Count(StatCounter.KeepDecisions)}, ")
              + Invariant($"Protected Word {totals.Count(StatCounter.ProtectedWordDecisions)}"),
          $"Replacement duration (ms): min {Ms(durations.Min)}, median {Ms(durations.Median)}, "
              + $"max {Ms(durations.Max)}, average {Ms(durations.Average)}",
      ];
      text.AppendLine().Append('[').Append(title).AppendLine("]");
      foreach (string line in lines) {
        text.AppendLine(line);
      }
    }

    private static string Percent(double? share) {
      return share is { } value ? value.ToString("P0", CultureInfo.InvariantCulture) : "-";
    }

    private static string Ms(TimeSpan? duration) {
      return duration is { } value
          ? value.TotalMilliseconds.ToString("0.#", CultureInfo.InvariantCulture)
          : "-";
    }

    private static string Invariant(FormattableString text) {
      return FormattableString.Invariant(text);
    }
  }
}
