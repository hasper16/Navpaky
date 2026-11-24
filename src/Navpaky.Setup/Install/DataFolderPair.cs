namespace Navpaky.Setup.Install {
  // Navpaky's two data folders for the current user: settings and word lists (roaming), and
  // machine-local state.
  internal sealed record DataFolderPair(string Roaming, string Local);
}
