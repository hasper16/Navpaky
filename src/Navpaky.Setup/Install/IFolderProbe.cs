namespace Navpaky.Setup.Install {
  /// <summary>
  /// What the folder rules need to know about the file system; tests use an in-memory one.
  /// </summary>
  internal interface IFolderProbe {
    /// <summary>Returns whether the folder exists and holds any file or folder.</summary>
    /// <param name="folder">The folder to look into.</param>
    /// <returns><see langword="true" /> when it has entries.</returns>
    bool HasEntries(string folder);

    /// <summary>Returns the names of the folder's top-level files and folders.</summary>
    /// <param name="folder">The folder to list.</param>
    /// <returns>The names, or <see langword="null" /> when the folder can't be listed.</returns>
    IReadOnlyList<string>? Entries(string folder);

    /// <summary>Returns whether a file exists.</summary>
    /// <param name="path">The file's full path.</param>
    /// <returns><see langword="true" /> when it exists.</returns>
    bool FileExists(string path);

    /// <summary>
    /// Returns whether the user can create a file in the folder, or in its nearest existing
    /// parent when the folder doesn't exist yet.
    /// </summary>
    /// <param name="folder">The folder to probe.</param>
    /// <returns><see langword="true" /> when a file can be created there.</returns>
    bool CanWrite(string folder);

    /// <summary>Returns the free space on the folder's drive.</summary>
    /// <param name="folder">The folder; it need not exist.</param>
    /// <returns>The free bytes available to the user.</returns>
    long FreeBytes(string folder);
  }
}
