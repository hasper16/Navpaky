# Proposal

## Why

The install wizard installs into a folder that already holds a Navpaky install: the existing install's own folder, or a picked folder with an orphaned copy. Velopack 1.2.161's setup then renames that folder and deletes it after installing (`commands/install.rs`). Any file or folder a user put there is lost without a word. CodeRabbit flagged this as Major on PR #56, and the owner decided: warn with a list.

## What Changes

- **Folder page warning.** Before reinstalling into a folder that holds a Navpaky install, the Folder page lists the entries at that folder's root that are not part of the install, and says they will be deleted.
  - Part of the install means Velopack's layout: `current\`, `packages\`, `Update.exe`, plus the root launcher `Navpaky.exe`.
  - The list is bounded: the first entries, then "and N more".
- **Explicit confirmation.** The page shows a check box to delete those entries and reinstall, off by default. Next stays disabled until it is checked.
  - Not checking it, going back or closing the wizard changes nothing.
  - With no extra entries, the page looks as it does today.
- **Test-only fix:** the temporary-folder cleanup in `SetupRunnerTests` retries while a stopped child process still holds a file. This is a CI flake (`IOException` in `Dispose`), not a behaviour change.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `install-wizard`: adds a requirement that the wizard lists the entries a reinstall would delete and asks for confirmation before using that folder.

## Impact

- **Code:**
  - `src/Navpaky.Setup/Install`: `InstallTarget`, `FolderCheck` and `IFolderProbe`/`DiskFolders`, which gain a list of root entries.
  - `src/Navpaky.Setup/Wizard`: `WizardViewModel` gets the confirmation and the Next gate; `WizardWindow` shows the list and the check box.
  - `Strings.resx` and `Strings.uk.resx` get new texts.
- **Tests:** `tests/Navpaky.Setup.Tests` (`InstallTargetTests`, `WizardViewModelTests`, `WizardWindowTests`, and the `SetupRunnerTests` cleanup).
- **Docs:** a VM check in `docs/manual-tests/deferred-by-hand.md`.
- **Security:** this touches the install area, so a `security-audit` review in guidance mode is required. No new process starts, network access or elevation. The wizard reads only folder entry names.
