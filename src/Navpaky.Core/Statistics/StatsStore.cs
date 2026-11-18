namespace Navpaky.Core.Statistics {
  /// <summary>
  /// Loads, saves and deletes the daily counters in <c>stats.json</c>.
  /// </summary>
  /// <remarks>Not thread-safe: call it from one thread.</remarks>
  public sealed class StatsStore {
    private StatsDocument? _saved;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsStore" /> class.
    /// </summary>
    /// <param name="folder">
    /// The folder that holds the counters file; it is created when missing.
    /// </param>
    public StatsStore(string folder) {
      FilePath = Path.Join(folder, "stats.json");
    }

    /// <summary>
    /// Gets the full path of the counters file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Returns the stored counters.
    /// </summary>
    /// <remarks>
    /// When the file cannot be parsed, renames it with a <c>.bak</c> suffix and starts anew.
    /// </remarks>
    /// <returns>The stored counters, or none when the file is missing or unreadable.</returns>
    /// <exception cref="IOException">The file exists but cannot be read.</exception>
    public StatsDocument Load() {
      byte[] json;
      try {
        json = File.ReadAllBytes(FilePath);
      } catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException) {
        return StatsDocument.Empty;
      }

      if (StatsJson.Parse(json) is { } document) {
        return document;
      }

      JsonFile.KeepUnreadable(FilePath);
      return StatsDocument.Empty;
    }

    /// <summary>
    /// Writes the counters to the file, replacing it in one step; the snapshot saved last is
    /// not written again.
    /// </summary>
    /// <param name="document">The counters to write.</param>
    public void Save(StatsDocument document) {
      // Snapshots are immutable, so the same reference holds the same counters.
      if (ReferenceEquals(document, _saved)) {
        return;
      }

      byte[] json = StatsJson.Write(document);
      JsonFile.WriteAtomically(FilePath, stream => stream.Write(json));
      _saved = document;
    }

    /// <summary>
    /// Deletes the counters file and any unreadable copies set aside, if there are any.
    /// </summary>
    public void Delete() {
      _saved = null;
      File.Delete(FilePath);
      string folder = Path.GetDirectoryName(FilePath)!;
      if (!Directory.Exists(folder)) {
        return;
      }

      foreach (string kept in Directory.EnumerateFiles(folder, "stats.json*.bak")) {
        File.Delete(kept);
      }
    }
  }
}
