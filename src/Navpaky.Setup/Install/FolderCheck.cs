namespace Navpaky.Setup.Install {
  // The folder the wizard would install into, whether it may, and whether the user can change it.
  internal sealed record FolderCheck(string Folder, FolderVerdict Verdict, bool Locked);
}
