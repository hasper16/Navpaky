# Proposal

## Why

"Rekey" is the name of the unrelated macOS app our product is modelled on, and "Key Switcher" is taken by other Windows layout tools. On 2026-11-17 the owner chose the new name **Navpaky** (Ukrainian «навпаки», "vice versa"); a trademark and Store check found no conflict. The rename must land before the first public release, because the Velopack pack id, the install folder and the data folder become permanent once users install the app.

## What Changes

- **BREAKING** (no released users yet): the product is called Navpaky everywhere the user sees it: tray tooltip and menu, window titles, messages, notifications, the About page, the welcome window, the installer title and author (`--packTitle`, `--packAuthors`), the uninstall question, both string tables (en, uk), README and the docs.
- **BREAKING**: identities that carry the name change: Velopack pack id `key-switcher` → `navpaky` (install folder `%LOCALAPPDATA%\navpaky`, setup `navpaky-win-Setup.exe`), data folder `%LOCALAPPDATA%\Rekey` split in two (owner decision, design D7): settings and word lists in `%APPDATA%\Navpaky` (Roaming), statistics, logs and other machine-local state in `%LOCALAPPDATA%\Navpaky Data` (not `Navpaky`: that would be the install root), single-instance mutex `Local\Navpaky-<SID>`, the Run-key value for start at sign-in, the diagnostics export file name, thread names, temp-folder prefixes and crash-report types.
- Internal names follow (owner decision): solution `Navpaky.slnx`, projects, assemblies, namespaces, tests and tools `Rekey.*` → `Navpaky.*` (including `Navpaky.App.exe`), embedded-resource logical names, `NativeMethods.txt` locations, CI workflow paths, `.editorconfig`, `.gitattributes`, the pre-commit hook and agent docs. The icon file is renamed only; its content and fonts stay for the design overhaul.
- Text that names the original macOS app keeps the name Rekey. Archived changes under `openspec/changes/archive/` stay as history.
- The GitHub repository (`hasper16/key-switcher`) and the update feed URL stay; renaming the repository is an open item for the owner.
- No migration from the old names: nothing has been released.
- Behaviour is otherwise identical.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

Every capability whose requirements name the product: the requirement text changes from Rekey to Navpaky with no change in behaviour, except the identities listed above.

- `app-lifecycle`: settings and word lists in `%APPDATA%\Navpaky`, machine-local state in `%LOCALAPPDATA%\Navpaky Data`; single instance and sign-in start under the new name.
- `auto-update`: installer, install folder, Installed apps entry and uninstall data folder use the new name.
- `auto-correction`, `keyboard-capture`, `language-profiles`, `layout-management`, `localization`, `manual-correction`, `onboarding`, `statistics-and-diagnostics`, `telemetry`, `text-replacement`, `tray-and-settings`, `word-learning`, `wrong-layout-detection`: product name in requirement text.

## Impact

- Code: every project under `src/`, `tests/`, `tools/`; `Rekey.slnx`; `Directory.Build.props` consumers; `NativeMethods.txt` files.
- Build and release: `.github/workflows/ci.yml` and `release.yml` (paths, pack id, title, authors, release name), `.githooks/pre-commit`, `.editorconfig`, `.gitattributes`.
- Docs: `README.md`, `AGENTS.md`, `GLOSSARY.md`, `openspec/config.yaml`, `docs/**` except text about the original app.
- Users: none yet; the owner's own test install under the old names must be uninstalled by hand (see design).
