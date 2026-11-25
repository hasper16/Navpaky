using Navpaky.Setup.Install;

namespace Navpaky.Setup.Wizard {
  // What the Folder page needs (design D5): the install to reuse, the default for a new one, the
  // data folders the target must not overlap, the room the install takes, the file system, and
  // the all-users folder (design D1 of add-machine-wide-install).
  internal sealed record WizardFolders(
      ExistingInstall? Existing,
      string DefaultFolder,
      DataFolderPair Data,
      long NeededBytes,
      IFolderProbe Probe,
      string MachineFolder);
}
