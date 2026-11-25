# Design

## Context

See proposal.md for why.

The Folder page's check (`InstallTarget.Check`) already treats a folder holding `Update.exe` and `current\Navpaky.App.exe` as a Navpaky install. It allows the existing install's locked folder, and a picked folder with such an install. The check runs on the UI thread when the page opens and after each pick. It uses `IFolderProbe`, whose disk version is `DiskFolders`.

Velopack 1.2.161's setup, given a non-empty target, renames it and deletes the renamed folder after installing (`commands/install.rs`, the overwrite branch). The wizard can't change that, so it must warn before Install.

## Goals / Non-Goals

**Goals:**
- Name what a reinstall would delete, before anything is deleted, and get an explicit yes.

**Non-Goals:**
- Moving or keeping the extra entries. The owner chose a warning.
- Checking inside `current\` and `packages\`: Velopack replaces both on every update anyway.
- The machine-wide install (change I2).

## Decisions

### D1. What is part of the install
These top-level names count as the install, compared case-insensitively:
- `current` and `packages` (Velopack's folders);
- `Update.exe` (Velopack's updater);
- `Navpaky.exe` (the root launcher our pack extracts).

Every other top-level file or folder is extra, hidden ones included.

*Alternatives:*
- Read the package's file list: it describes `current\` only, so it adds nothing at the top level.
- Check below the top level: everything below `current\` and `packages\` already belongs to Velopack.

### D2. Where the list is made
`IFolderProbe` gains `Entries(folder)`: the top-level names, or `null` when the folder can't be listed. `DiskFolders` uses `Directory.EnumerateFileSystemEntries` and returns `null` on `IOException` or `UnauthorizedAccessException`.

When the target holds a Navpaky install, `InstallTarget.Check` puts the extra names, sorted, on `FolderCheck.Extras`. A folder it can't list gets the verdict NotWritable. It couldn't be checked, and the setup couldn't replace it either. This keeps the folder rules in one place, testable with the in-memory probe.

All of this runs on the UI thread, as the check does today. No Win32 or UI Automation API is involved; listing one folder's top level is cheap.

### D3. Confirmation
`WizardViewModel` holds `ExtrasConfirmed`, false by default, and resets it whenever a new folder check replaces the current one. On the Folder page, `CanGoNext` needs the verdict Ok and either no extras or `ExtrasConfirmed`.

`WizardWindow` shows, in the Folder page, a warning text and the first 8 names, one per line. With more than 8, a line follows: "and N more". Then a check box, "Delete them and reinstall", bound to `ExtrasConfirmed`.

8 is a design decision: it keeps the page within the window at 1024×768. Nothing is deleted by the wizard itself; the setup deletes only after Install.

*Alternative:* an in-page confirmation step, as Start fresh has. Rejected: the check box keeps the list and the decision on one page, the way the Licence page works.

### D4. When the list is taken
The list is taken when the check runs: on entering the page and on each pick. It isn't taken again at Install.

Entries added between the Folder page and Install are deleted unlisted. That window is the user's own minute in the wizard, and a second check would need a second confirmation.

### D5. Test cleanup
`SetupRunnerTests.Dispose` retries `Directory.Delete` on `IOException`, up to 10 times with 200 ms between tries. A stopped `cmd.exe` child can still hold `seen-args.txt` for a moment after the kill.

## Risks / Trade-offs

- **An entry added after the check is deleted without a warning** → the window is short and the action is the user's (D4).
- **A future Velopack may add another top-level name** → it would be listed as extra: a false warning, not data loss. `Directory.Packages.props` pins Velopack 1.2.161, and the list sits in one place (`InstallTarget`).
- **Long names** → each name is one line with ellipsis trimming, so the page keeps its layout.
