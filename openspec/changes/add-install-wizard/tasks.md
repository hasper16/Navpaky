# Tasks

## 1. Shared look (design D2)

- [x] 1.1 Create `src/Navpaky.Ui` (WPF class library, namespace `Navpaky.Ui`, XAML prefix `u:`) and move these into it, with every resource key kept:
  - `BrandTheme`, `BrandFonts`, `BrandMark`, `ThemeFollower` and `SettingRow`;
  - the brand fonts, the mark and the shared styles (`Brand.xaml`, merged by the app's `Resources.xaml`);
  - `Localizer`, `LocExtension` and `UiCulture`. `Localizer` serves the strings each executable adds; the app adds its own when its assembly loads.

  `Navpaky.App` references it; nothing else changes. Verify: `dotnet build` and `dotnet test` (Core, Win32, App, DesktopChecks) pass; `--check-fonts` passes on a single-file publish of the app.

## 2. Data shapes (design D4, D7)

- [x] 2.1 Test first: `InstallChoices` (in `Navpaky.Core`) round-trips the version-1 JSON of design D4. An unknown `version`, a missing file or malformed JSON reads as "no choices", without throwing. Then implement it. Verify: `dotnet test tests/Navpaky.Core.Tests`.
- [x] 2.2 Test first: settings gain optional `InstallFolder`, `DesktopShortcut` and `InstallQuestionsAnswered`, and a settings file from 0.4 (without them) reads with all three unknown. Then add them. Verify: `dotnet test tests/Navpaky.Core.Tests`.

## 3. Folder rules (design D5)

- [ ] 3.1 Test first, through a fake file-system port: `InstallTarget` covers each of these cases. Then implement it in `src/Navpaky.Setup`.
  - It appends `\Navpaky` to a pick with another name, case-insensitive, but uses an existing install's folder as it is (for example `navpaky-app`).
  - It returns NotEmpty for a folder with foreign files.
  - It returns OverlapsData for a target equal to, inside or containing `%APPDATA%\Navpaky` or `%LOCALAPPDATA%\Navpaky`, and accepts `%LOCALAPPDATA%\Programs\Navpaky`.
  - It accepts an empty folder and an existing Navpaky install (`Update.exe` + `current\Navpaky.App.exe`).
  - It returns NotWritable when the probe fails.
  - It returns NotEnoughSpace below package size + 50 MB.
  - It locks to an existing install's folder.

  Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 3.2 Test first: the existing install is HKCU `Uninstall\navpaky-app` → `InstallLocation` only while that folder holds `Update.exe`, and it locks the folder. `InstallFolder` from kept settings is only the proposed, editable default. With neither, the default is `%LOCALAPPDATA%\Programs\Navpaky`. Registry and file reads go through ports. Then implement it. Verify: `dotnet test tests/Navpaky.Setup.Tests`.

## 4. Choices reach the app (design D4, D6, D10)

- [ ] 4.1 Test first, in temp folders: for each `data` value the install hook does the following. Then wire it into `OnAfterInstallFastCallback`.
  - `fresh` moves `%APPDATA%\Navpaky` to `%APPDATA%\Navpaky.fresh-<attempt>` and every `%LOCALAPPDATA%\Navpaky` entry except `install-choices.json` into `%LOCALAPPDATA%\Navpaky.fresh-<attempt>`, deleting nothing (design D4).
  - It writes the marker.
  - It applies launch at sign-in to the root `Navpaky.exe`.
  - It writes `UiLanguage`, `InstallFolder` and `DesktopShortcut` into settings, creating them with defaults when absent.
  - `keep` and `new` move nothing.
  - The last step writes `install-done-<attempt>`; a step that throws leaves no acknowledgement.
  - No choices file means today's behaviour (marker only).

  Verify: `dotnet test tests/Navpaky.App.Tests`.
- [ ] 4.2 Test first, in a temp folder: `DesktopShortcut.Create` writes a `.lnk` whose target is the given exe and whose working folder is its folder, and `Remove` deletes only a link that targets the install folder. Then implement it in `Navpaky.Win32` with `IShellLinkW` + `IPersistFile` through CsWin32, on an STA thread. Verify: `dotnet test tests/Navpaky.Win32.Tests`.
- [ ] 4.3 Test first: the uninstall hook also removes our desktop link (Velopack usually already has, design F6). Then add it to `Uninstaller`. Verify: `dotnet test tests/Navpaky.App.Tests`.
- [ ] 4.4 Test first, through the app's start-up path: the first start reads the choices file. Then implement it.
  - `keep` marks the Welcome completed, so no Welcome opens.
  - `new` and `fresh` open it.
  - Each sets `InstallQuestionsAnswered`, and the file is then deleted.
  - Without a file, `InstallQuestionsAnswered` stays unknown.

  Verify: `dotnet test tests/Navpaky.App.Tests`.

## 5. Welcome (spec onboarding, telemetry)

- [ ] 5.1 Test first: the Welcome's one switch, "Send anonymous usage counters and crash reports" (en and uk), turns crash reports and Usage Digests on together and off together; the setup signal stays off. Then replace the crash-report switch. Verify: `dotnet test tests/Navpaky.App.Tests`.
- [ ] 5.2 Test first:
  - When `InstallQuestionsAnswered` is unknown, step 1 shows the interface-language chips (Same as Windows, English, Українська) that switch the window at once, and step 3 shows launch at sign-in (on) and the desktop shortcut (off).
  - When it is set, they're absent.
  - Closing without changes leaves launch at sign-in on.

  Then implement it as in prototype v2/v3.1. Verify: `dotnet test tests/Navpaky.App.Tests`; offscreen renders of both forms in light and dark compared with the prototype.

## 6. The wizard (spec install-wizard, design D1, D3, D7)

- [ ] 6.1 Create `src/Navpaky.Setup` and `tests/Navpaky.Setup.Tests`. The wizard window has the brand header, step bar, step counter and Back/Next, follows the Windows theme, and is single-instance (named mutex). Test first: a second start exits and brings the first window to the front. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.2 Test first: the Language page's choice switches every wizard text at once (en/uk) and becomes `uiLanguage`; "Same as Windows" is the default. Then build the page. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.3 Test first: Next on the Licence page stays disabled until "I accept the licence" is checked; the text comes from the repository `LICENSE`, embedded at build. Then build the page. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.4 Test first:
  - Kept data (a `settings.json`, or local entries other than the choices file) shows Welcome back with "Use previous settings" selected and the kept items listed.
  - A corrupt `settings.json` still counts as kept, without details.
  - "Start fresh" then Next shows the confirmation; Cancel returns to the choice; "Delete and continue" sets `data = fresh`.

  Then build the page. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.5 Test first: the Folder page shows `InstallTarget`'s folder and verdict, disables Next for each refusal with its reason, shows needed and free space, and is read-only when locked. The picker is `OpenFolderDialog`. Then build the page. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.6 Test first: the Options page defaults (sign-in on, shortcut off) and its presets plus "was on/off" hints after "Use previous settings". Then build the page. Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.7 Test first, with a stub setup executable built for the test: `SetupRunner` does the following. Then implement it.
  - It writes the choices file before starting setup.
  - It extracts the embedded setup to a per-run temp folder.
  - It runs it with `--silent --log <file> --installto <folder>` and no `--` arguments.
  - It moves through the phases as the log grows.
  - It waits for the setup process only.
  - It reports success only for exit 0 **and** `install-done-<attempt>`; exit 0 without the acknowledgement is a failure.
  - On a 5-minute timeout it kills the setup's process tree and waits up to 30 s. It reports whether the process is confirmed gone, and Retry is allowed only then. A stub that ignores the kill reports "may still be running".
  - It deletes the temp copy.

  Verify: `dotnet test tests/Navpaky.Setup.Tests`.
- [ ] 6.8 Test first: the Installing page has no Back; a failure shows Retry and Close and keeps the log path, and only Close (with the "may still be running" text) when the setup could not be confirmed stopped; Done offers "Start Navpaky now" (on), and Finish starts `<folder>\Navpaky.exe` when it's checked. Then build the pages. Verify: `dotnet test tests/Navpaky.Setup.Tests`; offscreen renders of every page in light and dark, en and uk, compared with prototype v3.1.

- [ ] 6.9 Test first, in temp folders: after a successful attempt the wizard deletes both `*.fresh-<attempt>` backups. After a failed one it removes what the hook wrote and moves the backups back, so the data is byte-for-byte as before. A wizard start finding a leftover backup restores it before looking for kept data. Then implement it (design D4, D7). Verify: `dotnet test tests/Navpaky.Setup.Tests`.

## 7. Release (design D8, spec install-wizard)

- [ ] 7.1 `Navpaky.Setup.csproj` embeds `$(EmbeddedSetupPath)` as a resource and fails the build when it is missing in Release. Verify: a local Release publish with a test-signed pack produces `NavpakySetup.exe` that installs per `docs/release.md` "Local test build".
- [ ] 7.2 `release.yml` does the following after `vpk pack`:
  - publishes the wizard with the fresh `Setup.exe`;
  - signs it with `VPK_SIGN_PARAMS`;
  - fails unless `Get-AuthenticodeSignature` is `Valid` with the pinned publisher;
  - uploads it with `gh release upload`.

  A dry run keeps it unsigned as an artifact. Verify: a workflow dry run (Run workflow) produces the `NavpakySetup.exe` artifact and its log shows the signing step skipped.

## 8. Docs and checks

- [ ] 8.1 Update `docs/release.md` (the wizard asset, the local test build), `docs/user-guide.md` (installing, Welcome back, Start fresh) and the README download link. Verify: the docs name `NavpakySetup.exe` and no step still tells users to run `navpaky-app-win-Setup.exe`.
- [ ] 8.2 Append a section for this change to `docs/manual-tests/deferred-by-hand.md`, for a Windows VM (Hyper-V and the scripts of the 2026-11-24 run). It covers:
  - a new install with defaults and with a custom folder and shortcut;
  - a non-empty folder refused;
  - a reinstall over an existing `navpaky-app` install;
  - Use previous settings (no Welcome);
  - Start fresh with Cancel, then confirmed;
  - an install failure;
  - the plain `Setup.exe` fallback Welcome;
  - uninstall removing the shortcut;
  - a standard user;
  - wizard cold-start time.

  Verify: the section exists and every scenario of spec `install-wizard` maps to an item.
- [ ] 8.3 Run `/ponytail-review branch` and the `security-audit` review in guidance mode (areas: auto-update and installer, process execution, CI). Record it in the PR body; fix or defer each finding. Verify: the PR body has the `## Security review` section with the reviewed commit.
