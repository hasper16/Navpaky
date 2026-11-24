namespace Navpaky.Setup.Install {
  // Whether the wizard may install into a folder, and if not, why (design D5).
  internal enum FolderVerdict {
    Ok,

    // Exists with files that are no Navpaky install: Velopack's silent setup would delete them.
    NotEmpty,

    // Is, lies inside, or contains one of Navpaky's data folders.
    OverlapsData,

    NotWritable,

    NotEnoughSpace,
  }
}
