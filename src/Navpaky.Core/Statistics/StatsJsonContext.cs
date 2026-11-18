using System.Text.Json.Serialization;

namespace Navpaky.Core.Statistics {
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
      WriteIndented = true)]
  [JsonSerializable(typeof(StatsJson.StatsFile))]
  internal sealed partial class StatsJsonContext : JsonSerializerContext {
  }
}
