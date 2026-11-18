# Design

## Context

See proposal.md for why. The name appears in five kinds of places:

1. User-visible text: `Product.Name` (code-built text such as the tray's welcome item and message-box captions), literal names in XAML (tray tooltip, About page, welcome window), the two string tables `Strings.resx` / `Strings.uk.resx`, and the release workflow's `--packTitle`, `--packAuthors` and `--releaseName`.
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

**D1. Spelling per kind of identity.** The display name is `Navpaky`; the Velopack pack id is lower-case `navpaky` (Velopack ids are conventionally lower-case and the old id was too), so the install root is `%LOCALAPPDATA%\navpaky`. The data folder cannot be `%LOCALAPPDATA%\Navpaky`: Windows paths are case-insensitive, so it would be the install root itself, which Velopack deletes on uninstall, and `DataFolder.Resolve` refuses that layout. The data folder is therefore `%LOCALAPPDATA%\Navpaky Data` (owner to confirm; alternative: pack id `navpaky-app` and data `%LOCALAPPDATA%\Navpaky`). A test pins that the data folder lies outside the real install root. The Ukrainian UI uses the Latin brand name `Navpaky`, like every other name in the string tables.

**D2. Mechanical rename, then restore the original's references.** Replace `Rekey` → `Navpaky`, `rekey` → `navpaky`, `REKEY` → `NAVPAKY` and `key-switcher` → `navpaky` across tracked files outside the archive, rename paths with `git mv`, then put back by hand the few lines that name the macOS app (AGENTS.md, ADR 0003, `openspec/config.yaml`), the notes path `rekey-macos.md`, and every `hasper16/key-switcher` repository reference (update feed, orchestration `gh` query, release docs). Alternative considered: renaming only user-visible text. Rejected: the owner asked for internal names too, and mixed names would confuse agents and the graph.

**D3. `Product.Name` stays the one source in code; XAML and string tables carry the literal.** `Product.Name` already feeds code-built text. The XAML literal check (`XamlTextTests`) allows the product name as untranslatable text; it compares with `Product.Name` instead of a second literal, so a later rename touches one constant plus the text files. Placeholders in all 40-odd strings would change plural and format handling for no user benefit.

**D4. No migration.** Nothing has been released, so no user has `%LOCALAPPDATA%\Rekey` or the `key-switcher` install. The owner's own test installs are uninstalled by hand before installing a Navpaky build; until then the old `Rekey` Run value and data folder may remain on that machine (the by-hand checklist says so).

**D5. Main specs.** This change's deltas replace every requirement that names the product. The capability `Purpose` paragraphs and two scenario titles that name the product (`Rekey switched` in auto-correction, `Rekey falls behind the typing` in keyboard-capture; a MODIFIED block cannot rename a scenario) are not covered by a delta, so the archive step renames them along with the sync.

**D6. Win32 and threads.** No Win32 API or threading changes: the same `CreateMutex`, registry Run key and file APIs are called on the same threads with new names.

## Risks / Trade-offs

- [A missed occurrence leaves a mixed name] → after the rename, `git grep -i rekey` outside the archive must list only the restored references to the original app; the task's verification records the command.
- [Generated code (CsWin32, resx designer) still names old namespaces] → regenerate by building; `Strings.Designer.cs` is renamed with its resource base name and checked in the build.
- [The owner's machine keeps an old `Rekey` sign-in entry or data folder] → by-hand checklist item: uninstall the old build first.
- [Repository stays `key-switcher`] → the update feed URL is unchanged, so updates keep working; renaming the repo later needs a one-line change of `VelopackUpdates.Repository` (GitHub redirects old URLs meanwhile).

## Migration Plan

None for users. Owner: uninstall any `key-switcher` / Rekey test install before installing a Navpaky build.

## Open Questions

- Whether to rename the GitHub repository `hasper16/key-switcher` (owner's call; changes only `VelopackUpdates.Repository`, docs and the orchestration query).
