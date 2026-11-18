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

The installed 0.4.2 is a Release build: it takes updates only from GitHub Releases and only when the Navpaky publisher signed them, while a dry run packs unsigned. Pick one way:

- [ ] **In-app update (preferred):** push a signed pre-release tag from this branch's merge, such as `v0.4.3-net11.1` (`docs/release.md`, Publishing a release), then let the installed 0.4.2 find and apply it from the tray. Confirm Navpaky restarts as the new version.
- [ ] **Or install over it:** run the `Release` workflow by hand (`workflow_dispatch`) with a version above 0.4.2, download the `releases-*` artifact, and run its `navpaky-app-win-Setup.exe` with 0.4.2 installed. Confirm settings and word lists carried over.
- [ ] After the update, auto-correction, the tray and the settings window work as before (one Correction of "ghbdtn" in Notepad is enough).
- [ ] The install folder (`%LOCALAPPDATA%\navpaky-app`) is within 10% of 186 MB, the 0.4.2 size. New size: ____ MB.
