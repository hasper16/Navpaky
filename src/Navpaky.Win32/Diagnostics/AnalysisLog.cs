using System.Globalization;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Navpaky.Core;

namespace Navpaky.Win32.Diagnostics {
  /// <summary>
  /// Writes the word-analysis log: one file per local day, kept 7 days, readable only by the
  /// current user and SYSTEM.
  /// </summary>
  /// <remarks>
  /// It holds typed Words, so it exists only while the user keeps it on (design D4). Not
  /// thread-safe: the worker thread owns it; any thread may call <see cref="ReadAll" />.
  /// </remarks>
  public sealed class AnalysisLog {
    // Spec statistics-and-diagnostics: at most the last 7 days (design decision).
    private const int _keptDays = 7;
    private const string _prefix = "analysis-";
    private const string _extension = ".log";
    private const string _dateFormat = "yyyy-MM-dd";

    private readonly string _folder;

    // Files that could not be deleted when the log was turned off, as when another app holds one
    // open; they hold typed Words, so deleting them is tried again.
    private readonly List<string> _undeleted = [];
    private readonly TimeProvider _time;
    private bool _enabled;
    private DateOnly _prunedOn;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalysisLog" /> class, turned off.
    /// </summary>
    /// <param name="folder">The folder that holds the log files.</param>
    /// <param name="time">The clock that dates the entries.</param>
    public AnalysisLog(string folder, TimeProvider time) {
      _folder = folder;
      _time = time;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the log is kept.
    /// </summary>
    /// <value>
    /// <see langword="true" /> to write entries and delete those older than 7 days;
    /// <see langword="false" /> to write nothing and delete the log. The default is
    /// <see langword="false" />.
    /// </value>
    /// <exception cref="IOException">
    /// An expired log file cannot be deleted while turning the log on. Files that cannot be
    /// deleted while turning it off are kept for <see cref="RetryDeletes" /> instead.
    /// </exception>
    public bool Enabled {
      get {
        return _enabled;
      }

      set {
        _enabled = value;
        if (value) {
          Prune(Today());
        } else {
          DeleteAll([.. LogFiles(_folder)]);
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether a log file could not be deleted when the log was turned
    /// off and waits for <see cref="RetryDeletes" />.
    /// </summary>
    public bool HasUndeletedFiles => _undeleted.Count > 0;

    /// <summary>
    /// Returns the lines of every log file, oldest first, each after its date.
    /// </summary>
    /// <param name="folder">The folder that holds the log files.</param>
    /// <returns>The lines; none when the log is off.</returns>
    /// <exception cref="IOException">A log file cannot be read.</exception>
    public static IReadOnlyList<string> ReadAll(string folder) {
      List<string> lines = [];
      foreach (string file in LogFiles(folder).Order(StringComparer.Ordinal)) {
        string day = Path.GetFileNameWithoutExtension(file)[_prefix.Length..];
        using var stream = new FileStream(
            file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (reader.ReadLine() is { } line) {
          lines.Add($"{day} {line}");
        }
      }

      return lines;
    }

    /// <summary>
    /// Adds an analysed Word to today's file while the log is on.
    /// </summary>
    /// <param name="entry">The analysed Word.</param>
    /// <exception cref="IOException">The log cannot be written.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to the log is denied.</exception>
    public void Write(LogAnalysis entry) {
      if (!_enabled) {
        return;
      }

      DateTimeOffset now = _time.GetLocalNow();
      DateOnly today = DateOnly.FromDateTime(now.DateTime);
      if (today != _prunedOn) {
        Prune(today);
      }

      Directory.CreateDirectory(_folder);
      string path = Path.Join(_folder, FileName(today));

      // A file still waiting to be deleted holds Words from before the log was turned off; a new
      // entry never joins them, so the delete that follows cannot take it along.
      RetryDeletes();
      if (_undeleted.Contains(path, StringComparer.OrdinalIgnoreCase)) {
        return;
      }

      using FileStream stream = File.Exists(path)
          ? new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read)
          : CreateForUserOnly(path);
      byte[] line = Encoding.UTF8.GetBytes(Format(now, entry) + "\n");
      stream.Write(line);
    }

    /// <summary>
    /// Tries again to delete the log files that could not be deleted when the log was turned off.
    /// </summary>
    public void RetryDeletes() {
      if (_undeleted.Count > 0) {
        DeleteAll([.. _undeleted]);
      }
    }

    private static string Format(DateTimeOffset now, LogAnalysis entry) {
      IEnumerable<string> renderings = entry.Renderings.Select(
          rendering => $"{rendering.Layout.Value:X8}:{rendering.Text}");
      string decision = entry.ProtectedWord
          ? $"{entry.Decision} (Protected Word)"
          : entry.Decision.ToString();
      string time = now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
      string typed = $"{entry.Layout.Value:X8}:{entry.Word}";
      return $"{time}\t{typed}\t{string.Join(' ', renderings)}\t{decision}";
    }

    // Design D4: inheritance off, so neither administrators nor other users get access.
    private static FileStream CreateForUserOnly(string path) {
      var security = new FileSecurity();
      security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
      SecurityIdentifier[] identities = [
          WindowsIdentity.GetCurrent().User!,
          new SecurityIdentifier(WellKnownSidType.LocalSystemSid, domainSid: null)];
      foreach (SecurityIdentifier identity in identities) {
        security.AddAccessRule(new FileSystemAccessRule(
            identity, FileSystemRights.FullControl, AccessControlType.Allow));
      }

      return new FileInfo(path).Create(
          FileMode.CreateNew,
          FileSystemRights.Write,
          FileShare.Read,
          bufferSize: 4096,
          FileOptions.None,
          security);
    }

    private static string FileName(DateOnly day) {
      return _prefix + day.ToString(_dateFormat, CultureInfo.InvariantCulture) + _extension;
    }

    private static IEnumerable<string> LogFiles(string folder) {
      return Directory.Exists(folder)
          ? Directory.EnumerateFiles(folder, _prefix + "*" + _extension)
          : [];
    }

    private static DateOnly? DayOf(string file) {
      string name = Path.GetFileNameWithoutExtension(file)[_prefix.Length..];
      bool parsed = DateOnly.TryParseExact(
          name, _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly day);
      return parsed ? day : null;
    }

    // Spec statistics-and-diagnostics: turning the log off deletes it. Every file is tried, and
    // one that is in use is remembered for the next try.
    private void DeleteAll(string[] files) {
      _undeleted.Clear();
      foreach (string file in files) {
        try {
          File.Delete(file);
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
          _undeleted.Add(file);
        }
      }
    }

    private DateOnly Today() {
      return DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
    }

    // Files are whole days, so a day leaves once it is 7 or more days old. The date is kept
    // only after every expired file is gone, so a failed delete is tried again.
    private void Prune(DateOnly today) {
      DateOnly oldest = today.AddDays(1 - _keptDays);
      string[] expired = [.. LogFiles(_folder)
          .Where(file => DayOf(file) is not { } day || day < oldest)];
      foreach (string file in expired) {
        File.Delete(file);
      }

      _prunedOn = today;
    }
  }
}
