using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Navpaky.Core {
  /// <summary>
  /// Holds the Excluded Apps: the applications in which Auto Correction is off.
  /// </summary>
  /// <remarks>
  /// Each entry is an image path or, for a packaged Windows app, its application user model
  /// id. Entries compare without case, in the form <see cref="AppId.Normalise" /> gives. The
  /// default value is the empty list.
  /// </remarks>
  [JsonConverter(typeof(Converter))]
  public readonly struct ExcludedAppList : IEquatable<ExcludedAppList> {
    // The settings file holds the list as an array of strings.
    internal sealed class Converter : JsonConverter<ExcludedAppList> {
      public override ExcludedAppList Read(
          ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
          throw new JsonException("The Excluded Apps are an array of strings.");
        }

        var entries = new List<string>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
          entries.Add(reader.TokenType == JsonTokenType.String
              ? reader.GetString()!
              : throw new JsonException("An Excluded App is a string."));
        }

        return new ExcludedAppList(entries);
      }

      public override void Write(
          Utf8JsonWriter writer, ExcludedAppList value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        foreach (string entry in value.Entries) {
          writer.WriteStringValue(entry);
        }

        writer.WriteEndArray();
      }
    }

    private static readonly ImmutableSortedSet<string> _none =
        ImmutableSortedSet.Create<string>(StringComparer.OrdinalIgnoreCase);

    // Null in the default value.
    private readonly ImmutableSortedSet<string>? _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcludedAppList" /> struct.
    /// </summary>
    /// <param name="entries">
    /// The image paths and application user model ids of the Excluded Apps; blank entries are
    /// skipped.
    /// </param>
    public ExcludedAppList(IEnumerable<string> entries) {
      _entries = _none.Union(entries.Select(AppId.Normalise).Where(entry => entry.Length > 0));
    }

    /// <summary>
    /// Gets the entries in their compared form, sorted.
    /// </summary>
    public IReadOnlyList<string> Entries => _entries ?? _none;

    /// <summary>
    /// Returns a value indicating whether two lists hold the same Excluded Apps.
    /// </summary>
    /// <param name="left">The first list.</param>
    /// <param name="right">The second list.</param>
    /// <returns>
    /// <see langword="true" /> if they hold the same entries; otherwise,
    /// <see langword="false" />.
    /// </returns>
    public static bool operator ==(ExcludedAppList left, ExcludedAppList right) {
      return left.Equals(right);
    }

    /// <summary>
    /// Returns a value indicating whether two lists hold different Excluded Apps.
    /// </summary>
    /// <param name="left">The first list.</param>
    /// <param name="right">The second list.</param>
    /// <returns>
    /// <see langword="true" /> if their entries differ; otherwise, <see langword="false" />.
    /// </returns>
    public static bool operator !=(ExcludedAppList left, ExcludedAppList right) {
      return !left.Equals(right);
    }

    /// <summary>
    /// Returns a value indicating whether an application is an Excluded App.
    /// </summary>
    /// <remarks>
    /// A path entry also matches a packaged app at another path with the same file name, so a
    /// launcher stub such as Windows 11's <c>notepad.exe</c> covers the Store app it starts. A
    /// packaged app at the entry's own path is decided by its <see cref="AppId.Key" /> alone,
    /// so excluding a frame host does not exclude the apps it hosts.
    /// </remarks>
    /// <param name="app">The application that owns the focused window.</param>
    /// <returns>
    /// <see langword="true" /> if the list holds the application's <see cref="AppId.Key" />
    /// or, for a packaged app, a path with its file name; otherwise, <see langword="false" />.
    /// </returns>
    public bool Contains(AppId app) {
      ImmutableSortedSet<string> entries = _entries ?? _none;
      if (entries.Contains(app.Key)) {
        return true;
      }

      if (app.Aumid is null) {
        return false;
      }

      string path = AppId.Normalise(app.ExePath);
      string name = Path.GetFileName(path);
      return name.Length > 0 && entries.Any(entry => entry.Contains('\\', StringComparison.Ordinal)
          && !string.Equals(entry, path, StringComparison.OrdinalIgnoreCase)
          && string.Equals(Path.GetFileName(entry), name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public bool Equals(ExcludedAppList other) {
      return (_entries ?? _none).SetEquals(other._entries ?? _none);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) {
      return obj is ExcludedAppList other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
      var hash = default(HashCode);
      foreach (string entry in _entries ?? _none) {
        hash.Add(entry, StringComparer.OrdinalIgnoreCase);
      }

      return hash.ToHashCode();
    }
  }
}
