using System.Reflection;
using System.Text.Json;
using Navpaky.Core.Statistics;
using Navpaky.Core.Telemetry.Schemas;
using Xunit;

namespace Navpaky.Core.Tests.Telemetry {
  public sealed class SchemaPrivacyTests {
    // Names that could carry typed text or a lasting identifier (design, Risks).
    private static readonly string[] _forbidden = [
        "text", "word", "title", "path", "user", "name", "device", "machine", "install",
        "host", "session", "clipboard", "window"];

    private static readonly Type[] _requests = [typeof(UsageDigest), typeof(SetupSignal)];

    public static TheoryData<Type> Schemas() {
      return new TheoryData<Type>(typeof(UsageDigest).Assembly.GetTypes()
          .Where(type => type.Namespace == typeof(UsageDigest).Namespace && type.IsPublic));
    }

    [Theory]
    [MemberData(nameof(Schemas))]
    public void SchemaHasNoTextOrIdentifierProperty(Type schema) {
      Assert.Empty(Offences(schema));
    }

    [Fact]
    public void CheckCatchesTextProperty() {
      Assert.Equal(["SecretWord", "WindowTitle"], Offences(typeof(LeakyReport)));
    }

    [Fact]
    public void OnlyIdentifierIsThePerRequestOne() {
      Assert.All(
          _requests.Select(request => AllProperties(request)
              .Where(property => property.PropertyType == typeof(Guid)
                  || property.Name.EndsWith("Id", StringComparison.Ordinal))
              .Select(property => property.Name)),
          ids => Assert.Equal(["RequestId"], ids));
    }

    [Fact]
    public void DigestJsonCarriesCountersOnly() {
      var digest = new UsageDigest(
          Guid.NewGuid(),
          new DateOnly(2026, 11, 14),
          new AppEnvironment("1.0.0", "10.0.26100", "en-US", ["en", "ru"]),
          SettingStates.Of(new Settings()),
          DigestCounters.Of(DayStats.Empty(new DateOnly(2026, 11, 14))
              .Add(StatCounter.WordsAnalysed).Add(StatCounter.AutoCorrections)));

      using JsonDocument json = JsonDocument.Parse(TelemetryJson.Write(digest));

      JsonElement counters = json.RootElement.GetProperty("counters");
      Assert.Equal(1, counters.GetProperty("analysed").GetInt64());
      Assert.Equal(1, counters.GetProperty("autoCorrections").GetInt64());
      Assert.Equal(1, json.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    private static string[] Offences(Type schema) {
      return [.. AllProperties(schema)
          .Select(property => property.Name)
          .Where(name => _forbidden.Any(
              bad => name.Contains(bad, StringComparison.OrdinalIgnoreCase)))];
    }

    private static IEnumerable<PropertyInfo> AllProperties(Type type) {
      foreach (PropertyInfo property in type.GetProperties()) {
        if (property.Name == "EqualityContract") {
          continue;
        }

        yield return property;
        Type nested = property.PropertyType;
        if (nested.Namespace == type.Namespace && nested != type) {
          foreach (PropertyInfo inner in AllProperties(nested)) {
            yield return inner;
          }
        }
      }
    }

    private sealed record LeakyReport(string SecretWord, int Count, string WindowTitle);
  }
}
