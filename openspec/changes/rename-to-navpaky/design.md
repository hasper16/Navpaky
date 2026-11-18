# Design

## Context

See proposal.md for why. The name appears in five kinds of places:

1. User-visible text: `Product.Name` (code-built text such as the tray's welcome item and message-box captions), literal names in XAML (tray tooltip, About page, welcome window), the two string tables `Strings.resx` / `Strings.uk.resx`, and the release workflow's `--packTitle` and `--releaseName` (`--packAuthors` carries the publisher, "Pavlo Yeremenko", at the owner's request).
2. Identities Windows or Velopack keep: pack id `key-switcher` (install root `%LOCALAPPDATA%\key-switcher`, setup `key-switcher-win-Setup.exe`, package files `key-switcher-<version>-full.nupkg`), data folder `%LOCALAPPDATA%\Rekey` (`DataFolder`), mutex `Local\Rekey-<SID>` (`SingleInstanceGuard`), Run value `Rekey` (`SignInLaunch`), main exe `Rekey.App.exe` (`RekeyPublisher.MainExe`, `--mainExe`).
3. Internal names: solution, project folders and files, assemblies, root namespaces, `InternalsVisibleTo`, embedded-resource logical names (`Rekey.Core.Profiles.*`, `Rekey.Sounds.*`), the generated `Strings` resource base name, class names that carry the name (`RekeyPublisher`, `RekeyUpdateManager`), thread names, temp-folder prefixes, the crash-report type prefix `Rekey.Freeze.`.
4. Tooling and docs: CI paths, the pre-commit hook, `.editorconfig`, `.gitattributes`, the orchestration doc's path table, README, AGENTS, GLOSSARY, `openspec/config.yaml`, `docs/**`, the main specs (through this change's deltas).
5. References to the original macOS app named Rekey, and the owner's local notes path `.private/reverse-engineering/rekey-macos.md`. These are not our product.

## Goals / Non-Goals

**Goals:**
- One mechanical rename with no behaviour change other than the identities in item 2.
- The tree builds with warnings as errors and every test passes after the rename.

**Non-Goals:**
- Icons, fonts, colours, the "R" tray glyph: the design overhaul change owns them.
- Rewriting archived changes in `openspec/changes/archive/`.
- Migrating data or installs from the old names.
- Renaming the GitHub repository or the update feed URL.
- Turning every name in the string tables into a `{0}` placeholder for `Product.Name`.

## Decisions

**D1. Spelling per kind of identity.** The display name is `Navpaky` everywhere the user reads it (installer title, Start menu, Installed apps, the interface). The Velopack pack id is `navpaky-app`, so the install root is `%LOCALAPPDATA%\navpaky-app`; it holds only Velopack's files and goes on uninstall. It cannot be `navpaky`: Velopack deletes `%LOCALAPPDATA%\{packId}` entirely, and Windows paths are case-insensitive, so that root would be the local data folder `%LOCALAPPDATA%\Navpaky` (D7). The Ukrainian UI uses the Latin brand name `Navpaky`, like every other name in the string tables.

**D7. Two data folders named after the product (owner decision, 2026-11-18).** The layout matches what users see from apps such as 1Password: a Local and a Roaming folder, both named after the product. Data is split by whether it may follow the user:
- `%APPDATA%\Navpaky` (Roaming): `settings.json` (including Excluded Apps) and `words.json` (Protected and Learned Words), plus their kept-aside unreadable copies. A roaming profile may carry these to another PC, which is what a user expects of preferences.
- `%LOCALAPPDATA%\Navpaky` (Local): statistics, the word-analysis log folder, Incidents, restart state, word counts, telemetry's sent log and the Debug check files. These are machine facts or can hold typed words, so they must not roam.

`DataFolders.Resolve` returns both folders from one place and refuses any layout where either lies inside the install root; tests pin both paths, the `navpaky-app` root and the refusal. Alternatives considered: one folder `%LOCALAPPDATA%\Navpaky Data` with pack id `navpaky` (one folder, nothing roams), or pack id `navpaky` with a local folder `Navpaky Data`. The owner was told the split costs more than one folder (two roots in code, the uninstall question and the desktop checks handle both) and chose it, with the product-named folders.

Consequences:
- The uninstall hook asks once when either folder exists; the question process deletes both, each only after an explicit Yes and once the old installation is gone; the new-installation check (an `Update.exe` back in the install root) keeps both. A folder it cannot delete is named in the warning.
- The owner-only ACL (design D4 of add-statistics-and-diagnostics) stays on the word-analysis log folder, which is in the Local folder. Neither root gets an ACL of its own: `%APPDATA%` and `%LOCALAPPDATA%` already grant only the user, SYSTEM and administrators, and adding one is a behaviour change outside this rename.
- The desktop checks set aside and restore each file in the folder it now lives in.

**D2. Mechanical rename, then restore the original's references.** Replace `Rekey` → `Navpaky`, `rekey` → `navpaky`, `REKEY` → `NAVPAKY` and `key-switcher` → `navpaky` (then `navpaky-app` for the pack id, D1) across tracked files outside the archive, rename paths with `git mv`, then put back by hand the few lines that name the macOS app (AGENTS.md, ADR 0003, `openspec/config.yaml`), the notes path `rekey-macos.md`, and every `hasper16/key-switcher` repository reference (update feed, orchestration `gh` query, release docs). Alternative considered: renaming only user-visible text. Rejected: the owner asked for internal names too, and mixed names would confuse agents and the graph.

**D3. `Product.Name` stays the one source in code; XAML and string tables carry the literal.** `Product.Name` already feeds code-built text. The XAML literal check (`XamlTextTests`) allows the product name as untranslatable text; it compares with `Product.Name` instead of a second literal, so a later rename touches one constant plus the text files. Placeholders in all 40-odd strings would change plural and format handling for no user benefit.

**D4. No migration.** Nothing has been released, so no user has `%LOCALAPPDATA%\Rekey` or the `key-switcher` install. The owner's own test installs are uninstalled by hand before installing a Navpaky build; until then the old `Rekey` Run value and data folder may remain on that machine (the by-hand checklist says so).

**D5. Main specs.** This change's deltas replace every requirement that names the product. The capability `Purpose` paragraphs and two scenario titles that name the product (`Rekey switched` in auto-correction, `Rekey falls behind the typing` in keyboard-capture; a MODIFIED block cannot rename a scenario) are not covered by a delta, so the archive step renames them along with the sync.

**D6. Win32 and threads.** No Win32 API or threading changes: the same `CreateMutex`, registry Run key and file APIs are called on the same threads with new names.

**D8. A reinstall is detected by an install marker, not by the install root (owner's by-hand finding on c435388).** Since fix 2.12 of add-final-acceptance the question deleted data only once `Update.exe` had left the install root, and kept it when `Update.exe` was there after 30 s. But the uninstall hook waits up to 50 s for the question, and Velopack removes the install root only after the hook returns: a quick yes waited 30 s for a removal that could not start, then reported a reinstall and kept the data. The fix removes the wait:
- Velopack's install hook (`OnAfterInstallFastCallback`, no UI, 30 s limit) writes a new random id to `install-id.txt` in the Local data folder.
- The question reads that file before it asks; after yes it keeps both folders only when the id changed (Navpaky was installed again while the question was open) and otherwise deletes them at once. A missing file reads as empty, so an installation made before this build is not taken for a reinstall.
- The hook still waits at most 50 s; after a yes or no the question ends at once, so the hook returns and Velopack removes the install root.
- Alternatives considered: comparing `Update.exe` times (NTFS keeps the creation time of a file recreated under the same name within seconds, and a copy keeps the source's write time), or the version in `sq.version` (a reinstall of the same version looks the same).
- A test runs the real script in Windows PowerShell with the message boxes replaced by a stub and an install root still in place: yes deletes both folders; yes after a marker change keeps them.

## Risks / Trade-offs

- [A missed occurrence leaves a mixed name] → after the rename, `git grep -i rekey` outside the archive must list only the restored references to the original app; the task's verification records the command.
- [Generated code (CsWin32, resx designer) still names old namespaces] → regenerate by building; `Strings.Designer.cs` is renamed with its resource base name and checked in the build.
- [The owner's machine keeps an old `Rekey` sign-in entry or data folder] → by-hand checklist item: uninstall the old build first.
- [Repository stays `key-switcher`] → the update feed URL is unchanged, so updates keep working; renaming the repo later needs a one-line change of `VelopackUpdates.Repository` (GitHub redirects old URLs meanwhile).

## Migration Plan

None for users. Owner: uninstall any `key-switcher` / Rekey test install before installing a Navpaky build.

## Open Questions

- Whether to rename the GitHub repository `hasper16/key-switcher` (owner's call; changes only `VelopackUpdates.Repository`, docs and the orchestration query).
