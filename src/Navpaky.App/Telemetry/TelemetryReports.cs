using System.Globalization;
using Navpaky.Core;
using Navpaky.Core.Languages;
using Navpaky.Core.Statistics;
using Navpaky.Core.Telemetry.Schemas;

namespace Navpaky.App.Telemetry {
  // Builds the JSON of the Usage Digest and the setup signal from the schema records only
  // (design D2), each with a fresh request id.
  internal static class TelemetryReports {
    public static string Digest(
        DateOnly day, Settings settings, StatsDocument stats, IReadOnlyList<LayoutId> layouts) {
      DayStats counters = stats.Days.FirstOrDefault(entry => entry.Day == day)
          ?? DayStats.Empty(day);
      return TelemetryJson.Write(new UsageDigest(
          Guid.NewGuid(),
          day,
          Environment(layouts),
          SettingStates.Of(settings),
          DigestCounters.Of(counters)));
    }

    public static string Setup(
        Settings settings,
        IReadOnlyList<LayoutId> layouts,
        Func<LayoutId, Language?> languageOf,
        WelcomeProgress progress) {
      // Several Layouts can share one Language, such as English (US) and English (UK).
      int covered = layouts.Select(languageOf).OfType<Language>().Distinct().Count();
      return TelemetryJson.Write(new SetupSignal(
          Guid.NewGuid(),
          Environment(layouts),
          layouts.Count,
          covered,
          SettingStates.Of(settings),
          progress));
    }

    private static AppEnvironment Environment(IReadOnlyList<LayoutId> layouts) {
      return new AppEnvironment(
          TelemetryHost.Version,
          System.Environment.OSVersion.Version.ToString(),
          CultureInfo.CurrentUICulture.Name,
          [.. layouts.Select(layout => layout.LanguageCode.ToLowerInvariant()).Distinct()]);
    }
  }
}
