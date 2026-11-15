using System.Text.Json.Serialization;

namespace Rekey.Core.Statistics {
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
      WriteIndented = true)]
  [JsonSerializable(typeof(StatsJson.StatsFile))]
  internal sealed partial class StatsJsonContext : JsonSerializerContext {
  }
}
