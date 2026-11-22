using System.Text.Json.Serialization;

namespace Navpaky.Core {
  /// <summary>
  /// Specifies how the cursor indicator shows the new Language next to the caret.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter<CursorIndicatorStyle>))]
  public enum CursorIndicatorStyle {
    /// <summary>
    /// A translucent pill with a dot in the theme accent.
    /// </summary>
    [JsonStringEnumMemberName("pill")]
    Pill,

    /// <summary>
    /// A solid accent tag with a tail toward the caret.
    /// </summary>
    [JsonStringEnumMemberName("tag")]
    Tag,

    /// <summary>
    /// Accent letters with an underline and no background.
    /// </summary>
    [JsonStringEnumMemberName("underline")]
    Underline,
  }
}
