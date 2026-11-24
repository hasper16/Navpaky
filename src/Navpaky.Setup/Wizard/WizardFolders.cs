using Navpaky.Setup.Install;

namespace Navpaky.Setup.Wizard {
  // What the Folder page needs (design D5): the install to reuse, the default for a new one, the
  // data folders the target must not overlap, the room the install takes, and the file system.
  internal sealed record WizardFolders(
      string? ExistingInstall,
      string DefaultFolder,
      DataFolderPair Data,
      long NeededBytes,
      IFolderProbe Probe);
}
