using System.Text.Json.Serialization;

namespace Navpaky.Core {
  /// <summary>
  /// Specifies the language of Navpaky's interface.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter<UiLanguage>))]
  public enum UiLanguage {
    /// <summary>
    /// Ukrainian on Ukrainian or Russian Windows, otherwise English.
    /// </summary>
    [JsonStringEnumMemberName("system")]
    System,

    /// <summary>
    /// English.
    /// </summary>
    [JsonStringEnumMemberName("en")]
    English,

    /// <summary>
    /// Ukrainian.
    /// </summary>
    [JsonStringEnumMemberName("uk")]
    Ukrainian,
  }
}
