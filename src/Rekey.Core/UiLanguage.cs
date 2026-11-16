using System.Text.Json.Serialization;

namespace Rekey.Core {
  /// <summary>
  /// Specifies the language of Rekey's interface.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter<UiLanguage>))]
  public enum UiLanguage {
    /// <summary>
    /// The Windows display language when Rekey has it, otherwise English.
    /// </summary>
    [JsonStringEnumMemberName("system")]
    System,

    /// <summary>
    /// English.
    /// </summary>
    [JsonStringEnumMemberName("en")]
    English,

    /// <summary>
    /// Russian.
    /// </summary>
    [JsonStringEnumMemberName("ru")]
    Russian,

    /// <summary>
    /// Ukrainian.
    /// </summary>
    [JsonStringEnumMemberName("uk")]
    Ukrainian,
  }
}
