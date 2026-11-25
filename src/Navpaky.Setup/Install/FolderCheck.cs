namespace Navpaky.Setup.Install {
  // The folder the wizard would install into, whether it may, and whether the user can change it.
  // Extras are the top-level entries of a folder holding a Navpaky install that are not part of
  // it: the setup replaces the whole folder, so they would be deleted (design D1, D2 of
  // warn-reinstall-extra-files).
  internal sealed record FolderCheck(
      string Folder, FolderVerdict Verdict, bool Locked, IReadOnlyList<string> Extras) {
    public FolderCheck(string folder, FolderVerdict verdict, bool locked)
        : this(folder, verdict, locked, []) {
    }
  }
}
