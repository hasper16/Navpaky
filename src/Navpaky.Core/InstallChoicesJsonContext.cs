using System.Text.Json.Serialization;

namespace Navpaky.Core {
  [JsonSourceGenerationOptions(
      PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
      WriteIndented = true)]
  [JsonSerializable(typeof(InstallChoices.Wire))]
  internal sealed partial class InstallChoicesJsonContext : JsonSerializerContext {
  }
}
