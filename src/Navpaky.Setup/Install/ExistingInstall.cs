namespace Navpaky.Setup.Install {
  // An install of Navpaky the wizard must reuse, and whether it is the all-users one.
  internal sealed record ExistingInstall(string Folder, bool ForAllUsers);
}
