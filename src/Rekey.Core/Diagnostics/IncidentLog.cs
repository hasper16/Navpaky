using System.Text;
using System.Text.Json;

namespace Rekey.Core.Diagnostics {
  /// <summary>
  /// Keeps the last 500 Incidents in <c>incidents.jsonl</c>, one JSON object per line.
  /// </summary>
  /// <remarks>
  /// Not thread-safe: one thread appends, as <see cref="QueuedIncidentLog" /> does. A line holds
  /// only the time, the type and enum-coded details (design D3), so no window title or typed text
  /// can reach it.
  /// </remarks>
  public sealed class IncidentLog {
    // Spec statistics-and-diagnostics: the last 500 entries (design decision).
    private const int _maxEntries = 500;

    private int? _lines;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncidentLog" /> class.
    /// </summary>
    /// <param name="folder">The folder that holds the log; it is created when missing.</param>
    public IncidentLog(string folder) {
      FilePath = Path.Join(folder, "incidents.jsonl");
    }

    /// <summary>
    /// Gets the full path of the log file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Adds an Incident and removes the oldest ones beyond the last 500.
    /// </summary>
    /// <param name="incident">The Incident to add.</param>
    /// <exception cref="IOException">The log cannot be written.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the log is denied.</exception>
    public void Append(Incident incident) {
      int lines = _lines ?? ReadLines().Length;
      var line = new IncidentLine(
          incident.Time,
          incident.Type,
          [.. incident.Details.Select(detail => $"{detail.GetType().Name}.{detail}")]);
      string json = JsonSerializer.Serialize(line, IncidentJsonContext.Default.IncidentLine);
      Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
      File.AppendAllText(FilePath, json + "\n", Encoding.UTF8);
      lines++;
      if (lines > _maxEntries) {
        string[] kept = ReadLines()[^_maxEntries..];
        JsonFile.WriteAtomically(FilePath, stream => {
          using var writer = new StreamWriter(stream, leaveOpen: true);
          writer.NewLine = "\n";
          foreach (string text in kept) {
            writer.WriteLine(text);
          }
        });
        lines = _maxEntries;
      }

      _lines = lines;
    }

    /// <summary>
    /// Returns the logged Incidents, oldest first; lines Rekey cannot read are left out.
    /// </summary>
    /// <returns>The Incidents.</returns>
    /// <exception cref="IOException">The log exists but cannot be read.</exception>
    public IReadOnlyList<IncidentEntry> Read() {
      return [.. ReadLines().Select(Parse).OfType<IncidentEntry>()];
    }

    private static IncidentEntry? Parse(string text) {
      IncidentLine? line;
      try {
        line = JsonSerializer.Deserialize(text, IncidentJsonContext.Default.IncidentLine);
      } catch (JsonException) {
        return null;
      }

      if (line is not { Time: { } time, Type: { } type, Details: { } details }
          || !Enum.IsDefined(type)) {
        return null;
      }

      return new IncidentEntry(time, type, [.. details.OfType<string>()]);
    }

    private string[] ReadLines() {
      try {
        // Shared, so an export can read while the writer appends.
        using var stream = new FileStream(
            FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        List<string> lines = [];
        while (reader.ReadLine() is { } text) {
          if (text.Length > 0) {
            lines.Add(text);
          }
        }

        return [.. lines];
      } catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) {
        return [];
      }
    }

    internal sealed record IncidentLine(
        DateTimeOffset? Time, IncidentType? Type, IReadOnlyList<string?>? Details);
  }
}
