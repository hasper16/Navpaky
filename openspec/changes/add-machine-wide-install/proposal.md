# Proposal

## Why

The install wizard installs Navpaky for the current Windows account only. On a PC shared by several people, each of them installs and updates their own copy. IT and default AppLocker rules also prefer one copy in Program Files. The 2026-11-24 live VM run showed that Velopack's setup can install into Program Files, but the result is broken in six ways (see `design.md`, Context). This change adds an "For all users" mode that works around each of them (roadmap item I2; owner decision).

## What Changes

- The wizard's Folder page offers two modes: "Just for me" (today's per-user install, the default) and "For all users". "For all users" warns that an administrator's approval (UAC) is needed now and for every update. It installs into `%ProgramFiles%\Navpaky`, a folder the user can't change.
- An all-users install asks for elevation once, at Install. The wizard restarts itself elevated to run the setup and register the install for the whole PC. It never runs anything elevated from a path the unelevated user controls.
- The Installed apps entry is machine-wide (HKLM), and its uninstall elevates and names Navpaky. The per-user entry that Velopack writes into the approving administrator's profile is removed after the install and after every update.
- One Start-menu shortcut is shared by all users.
- Launch at sign-in for every user goes through Active Setup, which writes each user's own sign-in entry once. A standard user can then turn it off without an administrator.
- An all-users install never applies a downloaded update when Navpaky starts. Updates are applied only from the tray's "Restart to update".
- Before an update is applied, Navpaky closes in every session, saving each user's data first. An update no longer fails because another signed-in user has Navpaky open.
- Uninstalling an all-users install removes the machine-wide entry, the shared shortcut and the Active Setup registration.
- When an install already exists, the wizard keeps its mode: a per-user install can't be turned into an all-users one in place, and the reverse.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `install-wizard`: the wizard offers the "For all users" mode, asks for elevation only in that mode, and keeps the mode of an existing install.
- `auto-update`: all-users installation, its Installed apps entry, shared shortcut and per-user sign-in launch; updates of an all-users install (no apply on start, closing Navpaky in every session, the per-administrator entry cleanup); and its uninstall.

## Impact

- `src/Navpaky.Setup`: mode choice in `WizardViewModel` and the Folder page, an elevated headless path in the wizard's entry point, the setup runner's elevated launch, and HKLM lookup of an existing install.
- `src/Navpaky.Win32`: a new `MachineInstall` that owns the HKLM entry, the shared shortcut and Active Setup, and a close-for-update signal.
- `src/Navpaky.App`: `Program` turns off apply on start for all-users installs, adds the update hooks, the elevating uninstall argument and the machine cleanup on uninstall. `App` closes on the close-for-update signal.
- Security-sensitive: installer, elevation, a cross-session signal. The PR gets a `security-audit` review.
- No new package dependencies. The ACL types come with .NET.
