using System.Text.Json.Serialization;

namespace Navpaky.Core {
  /// <summary>
  /// Specifies what an install does with Navpaky data kept from an earlier install.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter<KeptData>))]
  public enum KeptData {
    /// <summary>
    /// No data was kept: a first install.
    /// </summary>
    [JsonStringEnumMemberName("new")]
    New,

    /// <summary>
    /// The user chose "Use previous settings": the kept data stays.
    /// </summary>
    [JsonStringEnumMemberName("keep")]
    Keep,

    /// <summary>
    /// The user chose "Start fresh": the kept data goes once the install completes.
    /// </summary>
    [JsonStringEnumMemberName("fresh")]
    Fresh,
  }
}
