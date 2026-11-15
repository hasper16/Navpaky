namespace Rekey.App.Onboarding {
  // The folders that tell where Rekey runs from: Velopack's install root when Rekey is
  // installed, the temporary folders and the user's Downloads folder when known.
  internal sealed record LocationFolders(
      string? InstallRoot, IReadOnlyList<string> Temporary, string? Downloads);
}
