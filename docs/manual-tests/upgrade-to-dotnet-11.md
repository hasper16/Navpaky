# Desktop checks: upgrade-to-dotnet-11

The move to .NET 11 changes no feature, so it re-runs every existing desktop-check set on the new runtime, and checks that an installed release updates to a .NET 11 package. The input clock (`InputClock`) is covered by `InputClockTests` in `Navpaky.Win32.Tests`; the sleep-and-wake part needs a person and is in the `upgrade-to-dotnet-11` section of [deferred-by-hand.md](deferred-by-hand.md).

Needs the .NET 11 SDK (11.0.100 or a later 11.0 feature band), the English (US) and Russian Layouts installed and a Debug build. Close every running Navpaky and input probe, then run from the repository root. Do not touch the mouse or keyboard while a set runs.

## Automated: `tools/Navpaky.DesktopChecks`

- [x] Default set, 36 of 36: `dotnet build && dotnet run --project tools/Navpaky.DesktopChecks`
- [x] `tray-and-settings`, 10 of 10: `dotnet run --project tools/Navpaky.DesktopChecks -- tray-and-settings`
- [x] `onboarding`, 9 of 9: `dotnet run --project tools/Navpaky.DesktopChecks -- onboarding`
- [x] The input-language list is the same before and after the runs.

Last run: default 36 of 36, `tray-and-settings` 10 of 10, `onboarding` 9 of 9 at 57feed8 on the owner's desktop, input-language list unchanged; 129ac64 changed only the single-file publish settings and `ci.yml`.

The tool now finds the Debug app in `src/Navpaky.App/bin/Debug/<its own target framework>/`, today `net11.0-windows`.

## Velopack update over the installed 0.4.2

The installed 0.4.2 is a local unsigned test pack of 7c187ac, not a GitHub release. Keep `C:\RekeyTest\pack-7c187ac\releases`: its `navpaky-app-win-Setup.exe` puts 0.4.2 back.

- [x] **Pack this branch as 0.4.3** the same way, from the repository root on this branch (`.tools\vpk` as in `docs/release.md`; `<sha>` is the commit):
  ```powershell
  dotnet publish src/Navpaky.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -p:Version=0.4.3 -o C:\RekeyTest\pack-<sha>\publish
  .tools\vpk pack --packId navpaky-app --packVersion 0.4.3 --packDir C:\RekeyTest\pack-<sha>\publish --mainExe Navpaky.App.exe --packTitle Navpaky --packAuthors 'Pavlo Yeremenko' --icon src\Navpaky.App\Navpaky.ico --runtime win-x64 --delta None --exclude '\.(pdb|xml)$' --outputDir C:\RekeyTest\pack-<sha>\releases
  ```
- [x] **Install over 0.4.2:** with 0.4.2 installed, run `C:\RekeyTest\pack-<sha>\releases\navpaky-app-win-Setup.exe`. Navpaky starts as 0.4.3 (About page), and settings and word lists carried over.
- [x] After the install, auto-correction, the tray and the settings window work as before (one Correction of "ghbdtn" in Notepad is enough).
- [x] `%LOCALAPPDATA%\navpaky-app\current\Navpaky.App.exe` is within 10% of 186 MB, its 0.4.2 size. New size: 201.5 MB (211,243,403 bytes against 195,384,033; +8.1%).
- [ ] Put 0.4.2 back with `C:\RekeyTest\pack-7c187ac\releases\navpaky-app-win-Setup.exe` if the owner wants to keep it until the next release.

Owner run at 129ac64: the 0.4.3 pack from `C:\RekeyTest\pack-129ac64` installed over 0.4.2, starts as 0.4.3, kept settings and word lists, corrects "ghbdtn", and its tray and settings window work. The 0.4.3 pack from 57feed8 crashed at every start (design D6).

The in-app updater applies a package only when the pinned publisher signed it, so it refuses these unsigned packs. Checking it from .NET 10 to .NET 11 needs the signed test feed of `docs/release.md`, Local test build (0.4.2 from 7c187ac, 0.4.3 from this branch); that is a by-hand check, not a merge gate. A `workflow_dispatch` run of `release.yml` also packs this branch unsigned, without the `release` environment, if a CI-built pack is wanted instead.
