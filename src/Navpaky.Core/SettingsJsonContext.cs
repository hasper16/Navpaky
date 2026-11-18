using System.Text.Json.Serialization;

namespace Navpaky.Core {
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
      WriteIndented = true)]
  [JsonSerializable(typeof(Settings))]
  internal sealed partial class SettingsJsonContext : JsonSerializerContext {
  }
}
