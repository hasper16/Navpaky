using System.Text.Json.Serialization;

namespace Rekey.Core.Telemetry.Schemas {
  // One line per request, as sent and as kept in sent.jsonl.
  [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
  [JsonSerializable(typeof(UsageDigest))]
  [JsonSerializable(typeof(SetupSignal))]
  [JsonSerializable(typeof(SignUp))]
  internal sealed partial class TelemetryJsonContext : JsonSerializerContext {
  }
}
