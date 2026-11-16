using System.Text.Json.Serialization;

namespace Rekey.Core {
  /// <summary>
  /// Specifies the language of Rekey's interface.
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
