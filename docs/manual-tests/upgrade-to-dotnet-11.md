# Desktop checks: upgrade-to-dotnet-11

The move to .NET 11 changes no feature, so it re-runs every existing desktop-check set on the new runtime, and checks that an installed release updates to a .NET 11 package. The input clock (`InputClock`) is covered by `InputClockTests` in `Navpaky.Win32.Tests`; the sleep-and-wake part needs a person and is in the `upgrade-to-dotnet-11` section of [deferred-by-hand.md](deferred-by-hand.md).

Needs the .NET 11 SDK (11.0.100 or a later 11.0 feature band), the English (US) and Russian Layouts installed and a Debug build. Close every running Navpaky and input probe, then run from the repository root. Do not touch the mouse or keyboard while a set runs.

## Automated: `tools/Navpaky.DesktopChecks`

- [ ] Default set, 36 of 36: `dotnet build && dotnet run --project tools/Navpaky.DesktopChecks`
- [ ] `tray-and-settings`, 10 of 10: `dotnet run --project tools/Navpaky.DesktopChecks -- tray-and-settings`
- [ ] `onboarding`, 9 of 9: `dotnet run --project tools/Navpaky.DesktopChecks -- onboarding`
- [ ] The input-language list is the same before and after the runs.

The tool now finds the Debug app in `src/Navpaky.App/bin/Debug/<its own target framework>/`, today `net11.0-windows`.

## Velopack update over the installed 0.4.2

The installed 0.4.2 is a Release build: it takes only signed, non-pre-release versions from GitHub Releases (`docs/release.md`), and a release would reach every user. So the install is checked by installing over 0.4.2, and the updater on a test feed:

- [ ] **Install over 0.4.2:** run the `Release` workflow by hand (`workflow_dispatch`) on this branch with version `0.4.3`, download the `releases-0.4.3` artifact, and run its `navpaky-app-win-Setup.exe` with 0.4.2 installed. Confirm Navpaky starts as 0.4.3 and settings and word lists carried over.
- [ ] **Updater, .NET 10 to .NET 11:** follow `docs/release.md`, Local test build, packing `0.4.2` from `main` before this change and `0.4.3` from this branch into the test feed; install the 0.4.2 pack and start it with `--update-feed`. Confirm it finds 0.4.3, applies it and restarts as 0.4.3. Afterwards uninstall the test install, keeping data, and reinstall the owner's 0.4.2 from GitHub Releases.
- [ ] After the update, auto-correction, the tray and the settings window work as before (one Correction of "ghbdtn" in Notepad is enough).
- [ ] The install folder (`%LOCALAPPDATA%\navpaky-app`) is within 10% of 186 MB, the 0.4.2 size. New size: ____ MB.
