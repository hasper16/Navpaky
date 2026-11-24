# Proposal

## Why

Velopack's one-click `Setup.exe` asks nothing. It always installs into `%LOCALAPPDATA%\navpaky-app` and can't offer a folder, a licence or a desktop shortcut. Uninstalling with "keep data" removes the sign-in entry, so after a reinstall launch at sign-in is silently off. The owner wants a branded install wizard (decided 2026-11-24) that asks these questions before anything is copied:
- the interface language, the licence, the folder, the desktop shortcut and launch at sign-in;
- on a reinstall that finds kept data, whether to keep it or start fresh.

The wizard then runs Velopack's setup silently. The Welcome keeps only what needs the running app.

The approved prototype is v3.1, https://claude.ai/artifact/PVqBHe6vAHB1eWrg5fyp8e. Its "All users" option is out of scope here; it moves to the later change `add-machine-wide-install`. A live run in a Windows 11 (uk-UA) VM on 2026-11-24 checked Velopack's behaviour; design.md records the findings that matter here.

## What Changes

- **New install wizard** (`NavpakySetup.exe`, WPF, Navpaky look, en/uk). Its pages:
  - Language (Same as Windows / English / Українська, switches the wizard at once).
  - Licence ("I accept").
  - Welcome back (only when kept data is found).
  - Folder (for this account only, default `%LOCALAPPDATA%\Programs\Navpaky`).
  - Options: launch at sign-in (on), desktop shortcut (off).
  - Installing, with progress.
  - Done ("Start Navpaky now", on).
- **Folder safety:**
  - The app always goes into a `Navpaky` subfolder of the chosen folder.
  - A folder that is not empty and is not a Navpaky install is refused, because Velopack's silent setup deletes its contents.
  - A reinstall reuses the folder of an existing install, because a second folder orphans the first copy.
  - The folder must be writable by the user; the wizard never elevates.
- **Hand-off file:** the wizard passes its choices to the app in a file in the user's data folder. Velopack 1.2.161 `Setup.exe` crashes on arguments after `--`, and drops them under `--silent`. The install hook applies the choices, so sign-in and the shortcut are set even when the user doesn't start Navpaky now.
- **Welcome back:** a reinstall that finds kept data asks:
  - "Use previous settings" presets the options from the kept settings, and no Welcome follows.
  - "Start fresh" deletes all kept data after a confirmation, then the full Welcome shows on first start.
- **Slimmer Welcome:** demo, try-it, then the hotkey, one telemetry consent and the tray tip. The language, sign-in and shortcut questions move to the wizard. They come back in the Welcome only when Navpaky was installed without the wizard (plain `Setup.exe`), so nothing goes unasked.
- **One telemetry consent** ("Send anonymous usage counters and crash reports") replaces the crash-report switch in the Welcome. It turns on both crash/freeze reports and Usage Digests. Settings keeps the two separate.
- **Release pipeline:** a single signed `NavpakySetup.exe` with Velopack's `Setup.exe` embedded is published next to the Velopack packages. Updates keep working as now.

## Capabilities

### New Capabilities

- `install-wizard`: the branded installer. Its pages and their choices, folder safety rules, the hand-off of choices to the app, Welcome back with Start fresh, install progress and errors, and the single signed download.

### Modified Capabilities

- `auto-update`:
  - Per-user installation: the wizard and its folder, with the default changed for new installs and existing installs kept where they are.
  - Clean uninstall: the desktop shortcut Navpaky created goes too.
- `onboarding`:
  - Welcome window on first run: not shown after "Use previous settings"; asks the install questions only when they weren't asked by the wizard.
  - Main choices up front: hotkey, one telemetry consent and the tray tip.
- `telemetry`, Nothing is sent without consent: the Welcome's one switch covers crash/freeze reports and Usage Digests.

## Impact

- **New project** `src/Navpaky.Setup` (WPF wizard) and its tests.
- **Shared look:** the brand theme, fonts and the language switcher are used by both the app and the wizard. Design D2 decides how they are shared.
- **`src/Navpaky.App`:**
  - the install hook (`Program.cs`, `Updates/InstallMarker`) reads the hand-off file;
  - new hand-off and Start-fresh code under `Updates/`;
  - the Welcome (`Onboarding/WelcomeViewModel`, `Views/WelcomeWindow.xaml`);
  - strings in en and uk.
- **`src/Navpaky.Win32`:** desktop shortcut through `IShellLink` (CsWin32).
- **`.github/workflows/release.yml`** builds, signs and uploads `NavpakySetup.exe`. **`docs/release.md`** and the user guide describe the wizard.
- **Security-sensitive** (installer, process execution, file deletion): needs a `security-audit` review before `ready`.
- **Depends on:** `add-auto-update-and-installer`, `add-onboarding`, `add-telemetry` and `fix-redesign-findings` (all merged).
- **Followed by** `add-machine-wide-install` ("All users").
