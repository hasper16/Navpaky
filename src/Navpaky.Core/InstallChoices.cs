using System.Text.Json;

namespace Navpaky.Core {
  /// <summary>
  /// Holds the choices the install wizard hands to the app's install hook through a file in the
  /// user's local data folder (add-install-wizard, design D4). It carries no typed text.
  /// </summary>
  /// <param name="Attempt">
  /// Identifies this press of Install; the acknowledgement carries it.
  /// </param>
  /// <param name="UiLanguage">
  /// One of the enumeration values that specifies the interface language the user chose.
  /// </param>
  /// <param name="LaunchAtSignIn">
  /// <see langword="true" /> to start Navpaky when the user signs in; otherwise,
  /// <see langword="false" />.
  /// </param>
  /// <param name="DesktopShortcut">
  /// <see langword="true" /> to put a Navpaky shortcut on the user's desktop; otherwise,
  /// <see langword="false" />.
  /// </param>
  /// <param name="Data">
  /// One of the enumeration values that specifies what happens to kept data.
  /// </param>
  /// <param name="InstallFolder">The folder Navpaky is installed into.</param>
  public sealed record InstallChoices(
      Guid Attempt,
      UiLanguage UiLanguage,
      bool LaunchAtSignIn,
      bool DesktopShortcut,
      KeptData Data,
      string InstallFolder) {
    /// <summary>
    /// The name of the hand-off file in the user's local data folder.
    /// </summary>
    public const string FileName = "install-choices.json";

    private const int _version = 1;

    /// <summary>
    /// Gets the name of the file the install hook writes, last, once it applied every choice of
    /// this attempt.
    /// </summary>
    public string AcknowledgementName => $"install-done-{Attempt:N}";

    /// <summary>
    /// Reads the choices from their JSON text.
    /// </summary>
    /// <param name="json">The text of the hand-off file.</param>
    /// <returns>
    /// The choices, or <see langword="null" /> when the text is not a complete version-1 hand-off
    /// file.
    /// </returns>
    public static InstallChoices? Parse(string json) {
      Wire? wire;
      try {
        wire = JsonSerializer.Deserialize(json, InstallChoicesJsonContext.Default.Wire);
      } catch (JsonException) {
        return null;
      }

      return wire is {
        Version: _version,
        Attempt: { } attempt,
        UiLanguage: { } language,
        LaunchAtSignIn: { } signIn,
        DesktopShortcut: { } shortcut,
        Data: { } data,
        InstallFolder: { Length: > 0 } folder,
      } && attempt != Guid.Empty
          ? new InstallChoices(attempt, language, signIn, shortcut, data, folder)
          : null;
    }

    /// <summary>
    /// Reads the hand-off file from a folder.
    /// </summary>
    /// <param name="folder">The user's local data folder.</param>
    /// <returns>
    /// The choices, or <see langword="null" /> when the file is missing, unreadable or not a
    /// complete version-1 hand-off file.
    /// </returns>
    public static InstallChoices? ReadFrom(string folder) {
      try {
        return Parse(File.ReadAllText(Path.Join(folder, FileName)));
      } catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
        return null;
      }
    }

    /// <summary>
    /// Returns the choices as the JSON text of a version-1 hand-off file.
    /// </summary>
    /// <returns>The JSON text.</returns>
    public string ToJson() {
      return JsonSerializer.Serialize(
          new Wire(
              _version, Attempt, UiLanguage, LaunchAtSignIn, DesktopShortcut, Data, InstallFolder),
          InstallChoicesJsonContext.Default.Wire);
    }

    /// <summary>
    /// Writes the hand-off file into a folder, replacing any earlier one in one step.
    /// </summary>
    /// <param name="folder">The user's local data folder; it is created when missing.</param>
    public void WriteTo(string folder) {
      string json = ToJson();
      JsonFile.WriteAtomically(Path.Join(folder, FileName), stream => {
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.Write(json);
      });
    }

    // The file as written; every field is optional here so a missing one reads as no choices.
    internal sealed record Wire(
        int? Version,
        Guid? Attempt,
        UiLanguage? UiLanguage,
        bool? LaunchAtSignIn,
        bool? DesktopShortcut,
        KeptData? Data,
        string? InstallFolder);
  }
}
