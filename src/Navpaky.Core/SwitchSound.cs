using System.Text.Json.Serialization;

namespace Navpaky.Core {
  /// <summary>
  /// Specifies the sound Navpaky plays after a Correction when the switch sound is on.
  /// </summary>
  [JsonConverter(typeof(JsonStringEnumConverter<SwitchSound>))]
  public enum SwitchSound {
    /// <summary>
    /// A short key click.
    /// </summary>
    [JsonStringEnumMemberName("keyClick")]
    KeyClick,

    /// <summary>
    /// A soft tick.
    /// </summary>
    [JsonStringEnumMemberName("softTick")]
    SoftTick,

    /// <summary>
    /// A short chime.
    /// </summary>
    [JsonStringEnumMemberName("chime")]
    Chime,

    /// <summary>
    /// A plucked bandura string.
    /// </summary>
    [JsonStringEnumMemberName("bandura")]
    Bandura,

    /// <summary>
    /// Two dry knocks of wooden spoons.
    /// </summary>
    [JsonStringEnumMemberName("woodenSpoons")]
    WoodenSpoons,

    /// <summary>
    /// A short water drop.
    /// </summary>
    [JsonStringEnumMemberName("drop")]
    Drop,

    /// <summary>
    /// A light tap on a glass.
    /// </summary>
    [JsonStringEnumMemberName("glass")]
    Glass,

    /// <summary>
    /// A hammer stroke on cymbal strings.
    /// </summary>
    [JsonStringEnumMemberName("cymbals")]
    Cymbals,

    /// <summary>
    /// A short sopilka note.
    /// </summary>
    [JsonStringEnumMemberName("sopilka")]
    Sopilka,

    /// <summary>
    /// A short springy jaw harp twang.
    /// </summary>
    [JsonStringEnumMemberName("jawHarp")]
    JawHarp,

    /// <summary>
    /// A soft frame drum beat with jingles.
    /// </summary>
    [JsonStringEnumMemberName("frameDrum")]
    FrameDrum,

    /// <summary>
    /// Two notes that rise when the new Language is not English and fall when it is English.
    /// </summary>
    [JsonStringEnumMemberName("navpaky")]
    Navpaky,
  }
}
