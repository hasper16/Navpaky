namespace Navpaky.App.Onboarding {
  // The folders that tell where Navpaky runs from: Velopack's install root when Navpaky is
  // installed, the temporary folders and the user's Downloads folder when known.
  internal sealed record LocationFolders(
      string? InstallRoot, IReadOnlyList<string> Temporary, string? Downloads);
}
