namespace Rekey.Core.Telemetry {
  /// <summary>
  /// Keeps the last 10 telemetry requests, one JSON document per line, in <c>sent.jsonl</c> so
  /// the user can see what was sent (spec telemetry, design D2).
  /// </summary>
  /// <remarks>Safe to call from any thread.</remarks>
  public sealed class SentLog {
    private const int _kept = 10;

    private readonly Lock _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SentLog" /> class.
    /// </summary>
    /// <param name="folder">The folder that holds the log.</param>
    public SentLog(string folder) {
      FilePath = Path.Join(folder, "sent.jsonl");
    }

    /// <summary>
    /// Gets the full path of the log.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Adds a sent request and drops the oldest beyond the last 10.
    /// </summary>
    /// <param name="json">The request body as sent.</param>
    /// <exception cref="IOException">The log cannot be written.</exception>
    public void Append(string json) {
      // Line breaks inside JSON are whitespace, so one line keeps the same document.
      string line = string.Join(
          ' ', json.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
      lock (_lock) {
        string[] lines = [.. ReadFile().TakeLast(_kept - 1), line];
        JsonFile.WriteAtomically(FilePath, stream => {
          using var writer = new StreamWriter(stream, leaveOpen: true);
          foreach (string kept in lines) {
            writer.WriteLine(kept);
          }
        });
      }
    }

    /// <summary>
    /// Returns the kept requests, oldest first.
    /// </summary>
    /// <returns>The requests; none when the log is missing.</returns>
    /// <exception cref="IOException">The log exists but cannot be read.</exception>
    public IReadOnlyList<string> Read() {
      // An open read would make the replace in Append fail and lose that entry.
      lock (_lock) {
        return ReadFile();
      }
    }

    private string[] ReadFile() {
      try {
        return File.ReadAllLines(FilePath);
      } catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) {
        return [];
      }
    }
  }
}
