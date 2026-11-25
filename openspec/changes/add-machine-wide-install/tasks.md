# Tasks

## 1. Machine registration (design D3)

- [x] 1.1 Test first: `MachineInstall` under a test root in HKCU. It creates or replaces the entry with the version, location, elevating `UninstallString`, `NoModify` and `NoRepair`, copies the display values from Velopack's per-user entry, and deletes that entry. It writes or deletes the Active Setup key by the sign-in flag, with the `reg.exe` stub. `MarkActiveSetupDone` writes the per-user version. `Unregister` removes the entry and the key. `IsMachineRoot` holds for `%ProgramFiles%\Navpaky` in any case and not for a per-user folder. Verify: `dotnet test tests/Navpaky.Win32.Tests --filter MachineInstall`.
- [x] 1.2 Test first: the shared shortcut. `MachineInstall` replaces a per-user Start-menu link that points into the root with a link in a given common folder, and `Unregister` deletes the common one. Both use temporary folders in the test. Verify: `dotnet test tests/Navpaky.Win32.Tests --filter MachineInstall`.

## 2. Close in every session (design D6)

- [x] 2.1 Test first: `CloseSignal`. `IsRaised` is false with no event, and true for an event created by `Raise` in the test, which runs elevated in CI and as a plain user locally. The owner check rejects an event whose owner is the current non-administrator user. Pass the event name in, so tests don't use the real one. Verify: `dotnet test tests/Navpaky.Win32.Tests --filter CloseSignal`.
- [x] 2.2 `App` polls the signal every 2 s when started from a machine root and calls `Shutdown()`. Test first through `CloseSignal` with an injected name and a short timer. Verify: `dotnet test tests/Navpaky.App.Tests --filter CloseForUpdate`.

## 3. App hooks for machine mode (design D4, D5)

- [x] 3.1 Test first: `MachineHooks`, with fakes for registration, signal and processes. After update, it re-registers with the hook's version. Before update, it raises the signal, waits, and stops the processes left under the root, not others. Before uninstall, it closes them and unregisters. Per-user roots do nothing. Wire them into `Program.Main` with `SetAutoApplyOnStartup(!machine)`. Verify: `dotnet test tests/Navpaky.App.Tests --filter MachineHooks` and `dotnet build -c Release` with no warnings.
- [x] 3.2 Test first: in machine mode, `InstallHook` marks Active Setup done for the current user, and not in per-user mode. Verify: `dotnet test tests/Navpaky.App.Tests --filter InstallHook`.
- [x] 3.3 Test first: `--uninstall-all-users`. Unelevated, it asks to re-start itself elevated with the same argument. Elevated, it starts `<root>\Update.exe --uninstall` without waiting. Both through a fake starter. Verify: `dotnet test tests/Navpaky.App.Tests --filter AllUsersUninstall`.

## 4. Wizard mode and folder (design D1, D7)

- [x] 4.1 Test first: `InstallLookup.Existing` reports an HKLM entry as an all-users install and an HKCU one as per-user. `InstallTarget.Check` for all users accepts an unwritable empty `%ProgramFiles%\Navpaky` and still refuses a non-empty foreign one. Verify: `dotnet test tests/Navpaky.Setup.Tests --filter "InstallLookup|InstallTarget"`.
- [x] 4.2 Test first: `WizardViewModel.ForAllUsers`:
  - switches the folder and locks it;
  - switching back restores the per-user proposal;
  - an existing per-user install disables all users;
  - an existing machine install forces it.

  Verify: `dotnet test tests/Navpaky.Setup.Tests --filter WizardViewModel`.
- [x] 4.3 Add the two radio buttons, the warning and the reasons to the Folder page, with English and Ukrainian texts. The window test checks that the warning shows only for all users. Verify: `dotnet test tests/Navpaky.Setup.Tests --filter WizardWindow`.

## 5. Elevated setup (design D2, D5)

- [x] 5.1 Test first: `SetupRunner` for all users starts the wizard's own exe through the elevating launcher, with only `--all-users-setup <attempt> <on|off>`. It polls the derived log path and maps a declined prompt (`Win32Exception` 1223) to Failed. `RunHookAgain` reads the version from `current\sq.version`. Verify: `dotnet test tests/Navpaky.Setup.Tests --filter SetupRunner`.
- [x] 5.2 Test first: `MachineSetup`:
  - refuses unelevated;
  - refuses an existing work folder;
  - runs the setup with the derived arguments;
  - registers on exit 0 only;
  - deletes the extracted setup.

  Test it with fakes for elevation, launcher and registration. Add the `Program.Main` entry point that runs it before WPF. Verify: `dotnet test tests/Navpaky.Setup.Tests --filter MachineSetup` and a Release build.

## 6. Checks and docs

- [x] 6.1 Run in the NavpakyLab VM from checkpoint `staged`, with a wizard and 0.1.0 → 0.2.0 feed built from this branch:
  - an admin installs for all users: one UAC prompt naming the wizard; HKLM entry, no HKCU entry, shared link, Active Setup;
  - NavpakyTest signs in: autostart and Welcome; turns autostart off with no prompt;
  - a pending update at start: no prompt;
  - "Restart to update" as the admin while NavpakyTest runs Navpaky: the update is applied, NavpakyTest's Navpaky closed, HKLM version bumped, no HKCU entry;
  - uninstall from Installed apps: prompt names Navpaky; files, entry, link and Active Setup gone.

  Steps the owner must do (a NavpakyTest sign-in) go through the orchestrator. Verify: evidence in `C:\Users\Public\NavpakyTest\hv\out\i2\`, summarised in the shared notes and the PR.

  2026-11-25: pass, through PowerShell Direct, from `test-session`:
  - the wizard's all-users page and a declined prompt;
  - the elevated install;
  - the Active Setup stub;
  - a pending update at start;
  - an update while NavpakyTest's Navpaky ran;
  - a declined and an approved uninstall.

  The parts that need a sign-in or an administrator's approval also ran the same day, in the VM with owner-approved empty-password lab accounts, and passed. They are recorded in `deferred-by-hand.md`: the standard user's install through another administrator's consent, a second user's first sign-in, the tray update with two users signed in, and the uninstall from Installed apps.
- [x] 6.2 Add an "All-users install" section to `docs/manual-tests/deferred-by-hand.md`: an over-the-shoulder password install by a standard user, and a declined UAC at Install, at update and at uninstall. Verify: the section exists.
- [x] 6.3 Run the ponytail review of the branch and the `security-audit` review in guidance mode. Areas: installer, elevation, process execution. Record it in the PR body and fix or defer each finding. Verify: the PR body has the `## Security review` section with the reviewed commit.

## Workflow follow-up

- Archive the change on `chore/archive-add-machine-wide-install` after the PR is merged.
