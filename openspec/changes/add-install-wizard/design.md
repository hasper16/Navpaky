# Design

## Context

Navpaky ships as Velopack 1.2.161 packages. Installing means running Velopack's one-click `Setup.exe` (`navpaky-app-win-Setup.exe`), which installs into `%LOCALAPPDATA%\navpaky-app`, creates a Start-menu link and starts the app. Our hooks run inside it:
- `OnAfterInstallFastCallback` writes the install marker;
- `OnBeforeUninstallFastCallback` turns sign-in off and asks the data question (`Updates/Uninstaller`, `DataQuestion`).

The Welcome asks the hotkey and crash reports, and the app turns launch at sign-in on at the first start.

The owner approved the branded wizard flow in prototype v3.1 (https://claude.ai/artifact/PVqBHe6vAHB1eWrg5fyp8e). Its "All users" option is out of scope here; the change `add-machine-wide-install` owns it.

On 2026-11-24 we checked Velopack's behaviour live in a throwaway Windows 11 (uk-UA, build 26300) Hyper-V VM, using Debug builds of main 025783e signed with a test certificate. The findings that shape this change, with the evidence in Velopack 1.2.161's source (`src/bins/src/setup.rs`, `commands/install.rs`, `commands/uninstall.rs`, `windows/registry.rs`, `windows/shortcuts.rs`):

| # | Finding | Consequence here |
|---|---|---|
| F1 | `Setup.exe … -- <args>` **panics** (clap: "Mismatch between definition and access of `EXE_ARGS`") and hangs. Under `--silent`, the source also drops the arguments, because the app is started only when not silent. | Choices go through a hand-off file (D4). |
| F2 | `Setup.exe --silent --installto <dir>` works in 7–24 s, starts nothing and shows no window. It writes the HKCU Installed apps entry (`InstallLocation` = dir), a Start-menu link to `current\Navpaky.App.exe` and the root launcher **`Navpaky.exe`**. Our install hook runs before the entry is written, with a 30 s budget. | The wizard runs Setup silently and starts the app itself (D3). |
| F3 | With a **non-empty** `--installto` folder, silent setup answers its overwrite question with yes, renames the folder and deletes it after the install, so a user file was lost. | Folder safety (D5). |
| F4 | Installing into a **second folder** moves the Installed apps entry (one key per pack id) to it and leaves the first copy orphaned. | Reuse the existing install's folder (D5). |
| F5 | Updates in a user-writable folder apply with **no UAC**. | No elevation anywhere in this change. |
| F6 | Velopack's uninstall removes **every** `.lnk` under the user's Desktop, Startup, Start menu and pinned folders that targets the install root, including a desktop link we created to the root `Navpaky.exe`. | The desktop shortcut needs no extra uninstall code (D6). |
| F7 | Our uninstall data question stays open until it's answered, and a parent that waits for the whole process tree waits for it too. | Nothing in the wizard waits on uninstall processes. |
| F8 | A blank self-contained WPF .NET 11 single-file app (compressed, all content extracted, as WPF 11 needs) is 67 MB and starts in 7.7 s cold and 0.6 s warm. Velopack's `Setup.exe` is about 82 MB (Debug). | Download size and start-up (D8, risks). |
| F9 | On a uk-UA Windows the group names (`Administrators`) and tool output are translated. | Never parse localized output; use SIDs and APIs. |

## Goals / Non-Goals

**Goals:**
- One branded, signed `NavpakySetup.exe` that asks every install question before copying anything, then installs through Velopack without showing Velopack.
- Choices that take effect even if the app is not started after the install.
- Safe folder handling, so no data loss and no orphaned copies.
- Reinstall with kept data: keep it, or start fresh after a confirmation.
- A Welcome that asks only what the running app needs, and still asks the install questions when the wizard wasn't used.

**Non-Goals:**
- Installing for all users, Program Files, or any elevation. That is `add-machine-wide-install`.
- Replacing Velopack's uninstall. It stays: Installed apps runs `Update.exe --uninstall`, and every update rewrites that entry.
- An MSI.
- Changing how updates are found, verified or applied.

## Decisions

### D1. A separate wizard project, `src/Navpaky.Setup`
The wizard is a new WPF .NET 11 executable, published self-contained single-file like the app (`IncludeAllContentForSelfExtract`, compression on). It holds Velopack's `Setup.exe` as an embedded resource. At install time it extracts that to a per-run folder under `%TEMP%`, runs it, then deletes it.

It references `Navpaky.Core` for the settings model and `UiLanguage`. It never loads the keyboard hook or any `Navpaky.Win32` capture code.

*Alternative:* a mode of `Navpaky.App.exe`. Rejected: the app would have to ship Setup.exe inside itself, and the installer would carry the hook code.

### D2. Shared look: a `Navpaky.Ui` class library
These move from `Navpaky.App/Views` and `Localization` into a new WPF class library `src/Navpaky.Ui`, referenced by both the app and the wizard:
- `BrandTheme`, `BrandFonts`, `BrandMark` and `ThemeFollower`;
- the fonts and the row, card, pill and switch styles;
- `Localizer`.

The app's behaviour doesn't change: same resources, same keys. Each executable keeps its own `Strings.resx`; the wizard has its own en/uk strings.

*Alternative:* linking the source files into both projects. Rejected: two copies of the XAML dictionaries to keep in step.

### D3. Running Velopack's setup
On Install, the wizard starts the extracted setup with `--silent --log <per-run log> --installto <folder>`, and **no `--` arguments** (F1). It runs on a background `Task`; the UI thread only shows progress.

- **Progress:** Velopack reports none under `--silent`, so the Installing page shows an indeterminate bar. It moves to the next phase when the log file shows "Extracting", "Starting process install hook" and "Installation completed".
- **Result:** the wizard waits for **the setup process only**, never its descendants (F7). Success needs both: exit code 0, **and** the hook's completion acknowledgement for this attempt (D4). Exit 0 alone isn't enough: Velopack 1.2.161 kills a hook that overruns its 30 s and still exits 0. Anything else is a failure.
- **Hook rerun:** an overrun is not rare. In the 2026-11-25 VM run, the first start of a never-seen `Navpaky.App.exe` waited for Defender's scan and overran 2 times in 9 installs, while a warm hook takes about 3 s. So when the setup exits 0, `<folder>\current\Navpaky.App.exe` exists and the acknowledgement is missing, the wizard starts `Navpaky.App.exe --veloapp-install <version>` from `current` itself. The version comes from the setup log's "Package Version" line. The wizard waits up to 2 minutes, the Applying phase stays shown, and it checks for the acknowledgement again.
  - It is the same hook with the same hand-off file, which is seconds old, so the 10-minute staleness rule (D4) is unaffected and nothing is left for a later start of the app.
  - A hook that fails again, or a missing `Navpaky.App.exe`, is a failure as before.
  - The hook is safe to run twice. Start fresh moves the kept data aside only when no backup of this attempt exists yet, so a rerun never moves the data of the stopped run's own install.
  - *Alternative:* leave the choices for the app's first start. Rejected: the choices must be in effect before Done even when Navpaky isn't started (spec "Choices reach the app"), and a file waiting for a later start would need its own staleness rule against steering a different install. The wizard keeps the per-run log for diagnostics. The log holds no typed text, but it does hold paths, so it isn't sent anywhere.
- **Timeout:** no exit after 5 minutes (design decision) is a failure too. The wizard first stops the setup it started (`Process.Kill(entireProcessTree: true)`, then up to 30 s for it to exit). **Retry is offered only once that process is confirmed gone.** If the wizard can't confirm it, it offers only Close and says a setup may still be running, so two setups never write the same folder.
- **On failure:** the wizard restores the Start-fresh backup if there is one (D4) and leaves any earlier install as Velopack left it.
- **Finish:** when "Start Navpaky now" is checked, the wizard starts `<folder>\Navpaky.exe` (the root launcher, F2) with `Process.Start` (`UseShellExecute = true`) and exits.

### D4. Hand-off file
Before running setup, the wizard writes `%LOCALAPPDATA%\Navpaky\install-choices.json`, overwriting any earlier one:
```json
{ "version": 1, "attempt": "<guid>", "uiLanguage": "system|en|uk",
  "launchAtSignIn": true, "desktopShortcut": false, "data": "new|keep|fresh",
  "installFolder": "<path>" }
```
`attempt` is new for every press of Install.

**The install hook** (`OnAfterInstallFastCallback`, which runs as the installing user inside setup, F2) reads the file and, within its 30 s budget:
1. **If `data` is `fresh`,** it **moves** the kept data aside instead of deleting it. `%APPDATA%\Navpaky` is renamed to `%APPDATA%\Navpaky.fresh-<attempt>`, and every entry of `%LOCALAPPDATA%\Navpaky` except `install-choices.json` moves into `%LOCALAPPDATA%\Navpaky.fresh-<attempt>`. Renames within one volume are quick and can be undone. Nothing is deleted yet.
2. Writes the install marker, as today. It stays for the uninstall data question's reinstall check.
3. Applies `launchAtSignIn` through `SignInLaunch` (HKCU Run + StartupApproved). The entry points to the root `Navpaky.exe`.
4. Creates or removes the desktop shortcut (D6).
5. Writes `UiLanguage`, `InstallFolder`, `DesktopShortcut` and `InstallQuestionsAnswered = true` into settings, creating `settings.json` with defaults when there is none. For `keep` it also sets `WelcomeCompleted`, so no Welcome opens; `new` and `fresh` leave it to open.
6. **Last,** writes the completion acknowledgement `%LOCALAPPDATA%\Navpaky\install-done-<attempt>`.

**The wizard's part.** After setup exits 0 and the acknowledgement exists (the install is complete, including the Installed apps entry), the wizard deletes both `*.fresh-<attempt>` backups. On any failure it restores them: it deletes what the hook wrote and moves the backups back.

**Leftovers.** A backup left behind by a crashed wizard is restored by the next wizard run before it looks for kept data, so the user's data is never lost silently. After a confirmed success the wizard also deletes `install-choices.json` and the acknowledgement.

**On first start** the app reads nothing from the wizard: the hook already put everything in settings. Its first-start default of launch at sign-in on applies only when `InstallQuestionsAnswered` is false, so the wizard's answer stands.

**No choices file** at install time means the plain Velopack setup or a portable copy: the hook writes only the marker, `InstallQuestionsAnswered` stays false, and the Welcome asks the install questions itself (spec `onboarding`).

*Alternatives:* command-line arguments (F1 rules them out); the wizard writing `settings.json` itself (it would couple the installer to the settings schema and race the app).

### D5. Folder rules
The folder logic lives in a pure, unit-tested class in `Navpaky.Setup` (`InstallTarget`). It takes:
- the user's pick;
- the **existing install**: only a real one, meaning HKCU `Software\Microsoft\Windows\CurrentVersion\Uninstall\navpaky-app` → `InstallLocation` whose folder still holds `Update.exe`;
- the **remembered folder**: `InstallFolder` from kept settings. It's only an editable default, because after an uninstall it names a folder that no longer holds Navpaky;
- the two data folders (`%APPDATA%\Navpaky`, `%LOCALAPPDATA%\Navpaky`);
- a file-system port.

It returns the target folder and a verdict:
- **Ok.**
- **NotEmpty:** the folder exists, has entries, and has no `Update.exe` plus `current\Navpaky.App.exe`.
- **OverlapsData:** the target equals, lies inside, or contains either data folder. Velopack would rename a non-empty `%LOCALAPPDATA%\Navpaky` before the hook runs, and Start fresh moves everything in it. The default `%LOCALAPPDATA%\Programs\Navpaky` doesn't overlap.
- **NotWritable:** creating and deleting a probe file under the nearest existing parent fails.
- **NotEnoughSpace:** compared with exactly what Velopack's setup requires (`commands/install.rs`): the package zip's compressed size plus its extracted size plus 50 MB.
  - The wizard reads the package from its embedded `Setup.exe`. Velopack's bundle header there holds the zip's offset and length, then a fixed 32-byte signature. The wizard sums the zip's central directory entries.
  - For the current build that's about 298 MB; the setup file's own size plus 50 MB (about 132 MB) let the page accept a drive the setup then refused (2026-11-25 VM run).
  - A setup without a readable bundle gives 0, as a build without a payload does.
- **Locked to the existing install.**

**Folder name.** A pick whose last segment isn't `Navpaky` (case-insensitive) gets `\Navpaky` appended. This applies only when there is no existing install: an existing install's folder is used exactly as it is, for example `%LOCALAPPDATA%\navpaky-app`.

**Existing install.** It always wins: the folder field is read-only with a one-line reason. Moving an install means uninstalling it first; after that, the remembered folder is proposed but can be changed.

**Default.** For new installs it is `%LOCALAPPDATA%\Programs\Navpaky`, Windows' place for per-user apps (`FOLDERID_UserProgramFiles`). Installs from before this change stay in `navpaky-app` through the existing-install rule.

### D6. Desktop shortcut
The pack keeps `--shortcuts StartMenuRoot`, so Velopack makes no desktop link.

On "yes", the hook creates `Navpaky.lnk` on the user's desktop (`SHGetKnownFolderPath(FOLDERID_Desktop)`) through `IShellLinkW` + `IPersistFile` (CsWin32). The target is the root `Navpaky.exe`, the working folder is the install folder, and the icon is the app's. On "no" it removes a `Navpaky.lnk` that targets the install folder. The calls run on the hook's thread, an STA created for the call.

Uninstall needs no new code: Velopack removes links into the install root (F6). The uninstall hook still removes ours, in case Velopack's search misses it.

### D7. Welcome back and Start fresh
**Detecting kept data.** The wizard treats data as kept when `%APPDATA%\Navpaky\settings.json` exists, or `%LOCALAPPDATA%\Navpaky` has entries other than `install-choices.json` and `install-done-*`. Before it looks, it restores any `*.fresh-*` backup a crashed earlier run left (D4).

**What it shows.** It reads `settings.json` with `Navpaky.Core`'s settings reader (read-only) to:
- preset launch at sign-in and the interface language;
- show the kept items: the hotkey, the number of learned words and apps, and whether statistics exist.

A corrupt or unreadable file still counts as kept data, but without details.

**Shortcut preset.** Velopack's uninstall removed the old desktop link (F6), so its state comes from settings (`DesktopShortcut`, written by the hook). It's off when unknown.

**Confirmation.** It's an in-page view (prototype v3.1), not a modal dialog. Cancel returns to the choice; "Delete and continue" moves on with `data = fresh`.

### D8. Release pipeline
`release.yml` changes after `vpk pack`:
1. `dotnet publish src/Navpaky.Setup` with `-p:EmbeddedSetupPath=<releases>/navpaky-app-win-Setup.exe -p:Version=<v>`.
2. Sign `NavpakySetup.exe` with signtool, using the same `VPK_SIGN_PARAMS` (Windows SDK signtool on the runner).
3. `Get-AuthenticodeSignature` must be `Valid` with the pinned publisher subject, otherwise the run fails.
4. `gh release upload <tag> NavpakySetup.exe` after `vpk upload`.

A dry run builds the wizard unsigned and keeps it as an artifact. Velopack's own `Setup.exe` stays in the release, because updates and the plain-setup fallback need it. The README and `docs/release.md` point to `NavpakySetup.exe`.

### D9. Threads and Win32 calls
- **Wizard:** all UI on the WPF dispatcher. Setup runs and the log is tailed on a `Task`, marshalled back with `Dispatcher.InvokeAsync`. The folder picker is WPF's `OpenFolderDialog` (.NET 8+). Free space comes from `DriveInfo`. Registry reads use `Microsoft.Win32.Registry` (HKCU).
- **App hook:** `SignInLaunch` (registry) and `IShellLinkW` on the hook thread, which runs before the app's UI starts.

### D10. Telemetry consent in the Welcome
The Welcome's one switch sets both `DiagnosticsConsent.CrashReports` and `DiagnosticsConsent.UsageDigest` (`DiagnosticsConsent.All`). Turning it off clears both. Settings keeps its two switches. The setup signal follows `UsageDigest`, as it always has, so the one switch covers it too; no third consent is added.

## Risks / Trade-offs

- **Download about 150 MB** (F8 plus Setup.exe), roughly 1.8× today → accepted by the owner. A native wizard would be smaller but loses the WPF look.
- **Cold start of the wizard about 8 s** while the single-file bundle extracts (F8). Nothing is shown before the WPF window, so the user may double-click again → the wizard is single-instance (named mutex). The README says it can take a few seconds.
- **The 30 s hook budget** must cover the Start-fresh renames, the marker, the Run entry, the shortcut and settings → keep it fast; renames, not deletions. Velopack 1.2.161 kills an overrunning hook and still exits 0, so the wizard relies on the per-attempt acknowledgement (D3, D4). When the setup succeeded but the hook was stopped, the wizard reruns the hook without that limit (D3); only no acknowledgement after that is a failed, partial install, and the Start-fresh backup is restored.
- **The plain Setup.exe stays downloadable** and bypasses the wizard → the Welcome fallback asks the install questions (spec `onboarding`).
- **Velopack upgrades** can change `--silent`, `--installto` or the panic in F1 → Velopack is pinned (`Directory.Packages.props`, `VPK_VERSION`); one live install run per Velopack bump (deferred by-hand checklist).
- **SmartScreen:** a new signed exe needs reputation like `Setup.exe` does. No change in kind.
- **Existing users** (0.4.x in `navpaky-app`) never see the wizard on update. A reinstall through the wizard keeps their folder.

## Migration Plan

Nothing migrates on update. The first wizard install of an existing user reuses `navpaky-app` (D5). Settings gain `InstallFolder` and `DesktopShortcut`, both optional, so older settings files read as unknown.
