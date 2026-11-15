using System.Text.Json.Serialization;

namespace Rekey.Core.Diagnostics {
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
      UseStringEnumConverter = true)]
  [JsonSerializable(typeof(IncidentLog.IncidentLine))]
  internal sealed partial class IncidentJsonContext : JsonSerializerContext {
  }
}
