# Design

## Context

See proposal.md for why.

The wizard (`src/Navpaky.Setup`) runs unelevated. `SetupRunner` writes the hand-off file `install-choices.json` into the user's `%LOCALAPPDATA%\Navpaky`, extracts Velopack's `Setup.exe` from its own resources and runs `Setup.exe --silent --log <log> --installto <folder>`. It counts the install as successful when the setup exits 0 and the install hook (`Navpaky.App.exe --veloapp-install`, `InstallHook`) has written the acknowledgement. When the acknowledgement is missing, it runs the hook once more itself.

The 2026-11-24 live run in the NavpakyLab VM (notes `install-wizard-hyperv-run.md`) installed Velopack 1.2.161 elevated into `C:\Program Files\Navpaky` and found:

1. Velopack writes the Installed apps entry into the approving administrator's HKCU, and its `UninstallString` runs `Update.exe --uninstall` unelevated. That uninstall exits 0 and leaves every file.
2. The Start-menu shortcut goes into that administrator's profile only.
3. A standard user can't turn off an HKLM Run entry without an administrator's password.
4. With a pending update, Velopack applies it when the app starts (`autoApply: True`), which raises a UAC or credential prompt at every start and sign-in.
5. An update fails ("one or more running processes prevented it") while another user's Navpaky runs.
6. Every update re-creates the HKCU entry in the approving administrator's profile and leaves the HKLM version stale.

Velopack 1.2.161 source, read at tag `1.2.161`:
- `install.rs`: shortcuts, then the install hook, **then** `write_uninstall_entry`. A hook can't remove the HKCU entry of the install it runs in.
- `apply_windows_impl.rs`: an unwritable root re-launches `Update.exe apply` as administrator. The elevated child runs `--veloapp-obsolete` (old version, 15 s), force-stops the package, swaps `current`, runs `update_uninstall_entry`, **then** `--veloapp-updated` (new version, 15 s), then refreshes shortcuts. Both hooks run elevated, as the approving administrator, and the updated hook runs after the HKCU entry is rewritten.
- `SetAutoApplyOnStartup(bool)` exists on `VelopackApp` in the 1.2.161 package (checked in its XML docs; ctx7 `/velopack/velopack.docs` describes it).

## Goals / Non-Goals

**Goals:**
- One elevation for the install, an elevating uninstall that names Navpaky, and updates that don't break while two users are signed in.
- No elevated code path that runs or writes something at a path the unelevated user controls.

**Non-Goals:**
- Silent all-users installs for IT (`--all-users` switches, Intune, GPO): roadmap item C3.
- Moving an existing per-user install to all users in place, or the reverse.
- Restarting other users' Navpaky after an update. They get it back at their next sign-in, or by starting it.
- Removing other users' data or sign-in entries on uninstall.
- Removing the single-file `%TEMP%` extraction (roadmap V2), see Risks.

## Decisions

### D1. Mode on the Folder page
The Folder page gets two radio buttons above the folder: "Just for me" (default) and "For all users". No new page: the mode decides the folder, so it belongs there.

`WizardViewModel.ForAllUsers` switches `Folder` between the per-user check and the fixed `%ProgramFiles%\Navpaky` (`Environment.SpecialFolder.ProgramFiles`), and resets the extras confirmation.

`InstallTarget.Check` gains a `forAllUsers` flag. It skips the writability check (D2 does the writing) and keeps the overlap, non-empty, extras and space checks.

An existing install fixes the mode (`InstallLookup.Existing` returns the folder and whether its entry is in HKLM). `UninstallEntry` reads HKCU first, then HKLM; key `navpaky-app` in both.

*Alternative:* a separate mode page. It adds a step to every install, while most users take the default.

### D2. The elevated wizard
For all users, `SetupRunner` doesn't extract or start `Setup.exe` itself. It starts its own executable through `ShellExecuteEx` with the `runas` verb (`ElevatedSetupLauncher`, `ProcessStartInfo { UseShellExecute = true, Verb = "runas" }`), with the arguments:

```
--all-users-setup <attempt:N> <sign-in: on|off>
```

The UAC prompt names the signed Navpaky wizard.

The wizard's entry point moves to a `Program.Main` (`StartupObject`) that checks for `--all-users-setup` before any WPF object exists. The elevated path (`MachineSetup.Run`, worker thread = main thread, no window):
1. Exits with 1 unless the token is elevated.
2. Creates `%TEMP%\navpaky-setup-<attempt>`, refusing one that already exists. This is the elevated user's own temp folder.
3. Extracts its own embedded `Setup.exe` there and runs it: `--silent --log <that folder>\setup.log --installto %ProgramFiles%\Navpaky`. It waits up to 5 minutes, then kills it (exit 1).
4. On exit 0, registers the machine install (D3) and exits 0.
5. Deletes `Setup.exe` in every case. The log stays for the failure message.

Only the attempt id and a boolean cross the elevation boundary. The folder, the setup and the log path are all derived by the elevated side.

The unelevated runner keeps its loop. It polls `%TEMP%\navpaky-setup-<attempt>\setup.log` for progress and shows that path on failure. When the administrator is the same account, it's the same file; with an over-the-shoulder administrator, the file isn't there and progress stays on Copying.

A declined or timed-out prompt makes `Process.Start` throw `Win32Exception` (1223). The view model already maps that to Failed, with Retry. The unelevated runner can't kill the elevated process. As today, `KillTree` failing leaves the outcome `MayStillRun`.

*Alternatives:*
- Elevate Velopack's `Setup.exe` directly. It would run from the user's temp folder, the prompt would say "Velopack", and nothing elevated would run after the setup writes its HKCU entry.
- Pass the log or setup path to the elevated side: that is a classic elevated-write primitive for a standard user whose administrator types a password.

### D3. Machine registration (`MachineInstall`, Navpaky.Win32)
One class owns every machine-wide artifact. Its keys sit under an injectable `RegistryKey` root, as in `SignInLaunch`, so tests use HKCU.

- **Installed apps**: `RegisterEntry(root, version)` copies `DisplayName`, `DisplayIcon` and `Publisher` from Velopack's HKCU entry, if any. It writes `HKLM\…\Uninstall\navpaky-app` with:
  - `DisplayVersion`;
  - `InstallLocation`;
  - `UninstallString` = `"<root>\current\Navpaky.App.exe" --uninstall-all-users`;
  - `NoModify` = 1, `NoRepair` = 1.

  It then deletes the HKCU entry. The same key name keeps `UninstallEntry` lookups simple.
- **Shortcuts**: deletes Velopack's per-user `Programs\Navpaky.lnk` when it points into the root. It creates `%ProgramData%\Microsoft\Windows\Start Menu\Programs\Navpaky.lnk` → `<root>\Navpaky.exe` with the existing `DesktopShortcut.Create` (a shell link).
- **Active Setup**: when sign-in is on, writes `HKLM\SOFTWARE\Microsoft\Active Setup\Installed Components\Navpaky` with:
  - `(default)` = `Navpaky`;
  - `Version` = `1,0,0,0`;
  - `IsInstalled` = 1;
  - `StubPath` = `reg.exe add HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v Navpaky /t REG_SZ /d "\"<root>\Navpaky.exe\"" /f`.

  This is the value `SignInLaunch` writes. When sign-in is off, it deletes the key.
- **Unregister**: deletes the HKLM entry, the Active Setup key and the shared shortcut.

`reg.exe` keeps sign-in fast: Active Setup runs the stub synchronously before the desktop appears, and a cold Navpaky start extracts about 160 MB. `Version` never changes, so an update never re-runs the stub, and a user who turned the sign-in entry off keeps it off.

The installing user's own choice comes from the install hook (D5). The hook also writes `HKCU\…\Active Setup\Installed Components\Navpaky\Version` = `1,0,0,0` (`MarkActiveSetupDone`), so the stub never overrides that choice at the next sign-in.

*Alternative:* HKLM Run. The live run showed a standard user can't turn it off (Context 3).

### D4. Machine mode in the app
`Program.Main` computes `machine = MachineInstall.IsMachineRoot(InstallRoot())`: the root equals `%ProgramFiles%\Navpaky` (`SpecialFolder.ProgramFiles`), case-insensitive. Only the wizard's all-users mode installs there.

Then:
- `VelopackApp.Build().SetAutoApplyOnStartup(!machine)`. Per-user installs keep today's behaviour.
- `OnBeforeUpdateFastCallback` (the obsolete hook, elevated, 15 s): in machine mode, closes Navpaky in every session (D6).
- `OnAfterUpdateFastCallback` (elevated, after Velopack rewrote its entry): in machine mode, `RegisterEntry(root, version)` again. That bumps the HKLM version and deletes the approving administrator's HKCU entry.
- `OnBeforeUninstallFastCallback`: in machine mode, closes Navpaky in other sessions (D6) and calls `Unregister()`, then runs today's `Uninstaller`. The data question is for the elevated account.
- `--uninstall-all-users`, checked before `VelopackApp`:
  - unelevated: re-starts itself with `runas` and the same argument, and exits. A declined prompt exits with 1223 and changes nothing.
  - elevated: starts `<root>\Update.exe --uninstall` and exits at once, so it doesn't hold a file Velopack deletes.

### D5. Per-user choices for all users
The hand-off file stays in the wizard user's `%LOCALAPPDATA%\Navpaky`, unchanged.

When the administrator is the same account, Velopack's install hook (elevated, same profile) finds it and applies it, as today. With an over-the-shoulder administrator, the hook runs in the administrator's profile, finds no hand-off and only writes the marker. The wizard then runs the hook again, unelevated, as the wizard user (the existing `RunHookAgain` path), and that run applies the choices.

`RunHookAgain` reads the version from `<root>\current\sq.version` (the `<version>` element Velopack writes) instead of the setup log, which the wizard may not be able to read (D2). In machine mode, the hook also calls `MarkActiveSetupDone` and sets the user's sign-in entry per the choice (`SignInLaunch`, HKCU, as today).

### D6. Closing Navpaky in every session
The obsolete and uninstall hooks run elevated. In machine mode they:
1. Create the event `Global\NavpakyCloseForUpdate` (`EventWaitHandleAcl.Create`):
   - SYSTEM and Administrators get full control;
   - Authenticated Users get `Synchronize` only;
   - it is signalled at creation.
2. Wait up to 10 s (design decision, inside the 15 s hook limit) for every other `Navpaky.App` process whose image is under the install root to exit.
3. Kill the ones left, then close the event.

Every Navpaky started from a machine root polls `EventWaitHandle.TryOpenExisting` every 2 s (design decision) on a `DispatcherTimer`. That is one kernel call, and only in machine mode. An event that exists and is owned by Administrators or SYSTEM (`GetAccessControl().GetOwner`) calls the normal `Shutdown()`, so `OnExit` saves settings, word lists and statistics. An event owned by anyone else is ignored, so a standard user can't close other users' Navpaky by creating the name first.

Velopack's own force-stop then finds nothing running, and the `current` swap succeeds.

*Alternatives:*
- Kill only: loses up to 60 s of statistics, the save period, and the debounced word lists.
- A per-session `Local\` event: the elevated hook would have to open other sessions' namespaces.

### D7. Wizard texts
New strings in `Strings.resx` and `Strings.uk.resx`:
- the two mode labels;
- the all-users warning: UAC now, a password for non-administrators, UAC at every update, the prompt cancels itself after about 2 minutes, and it accepts the active keyboard layout;
- the two "existing install" reasons.

The Options page's sign-in label stays. In all-users mode the hint says it applies to every user's first sign-in.

## Risks / Trade-offs

- [The elevated wizard and the elevated hooks are single-file builds that extract native libraries into the elevated user's `%TEMP%\.net`.] With the same account, unelevated code of that account could plant files there. UAC is not a security boundary between one account's two tokens. With an over-the-shoulder administrator, that folder is in the administrator's profile, out of the standard user's reach. → Deferred to V2 (folder build), noted in the PR's security review.
- [The wizard runs elevated from the download folder, which the user can write to.] This is the usual exposure of downloaded installers: a DLL planted next to the executable. → The elevated path loads no WPF. Signed, non-extracting binaries come with V2 and V3. Recorded as a deferred finding.
- [Any user can create `Global\NavpakyCloseForUpdate` first.] → The owner check (D6) ignores it. The worst case is that the elevated hook can't create the event, and the update fails as it does today.
- [Other users' Navpaky stays closed after an update.] → Accepted (Non-Goals). The update keeps their data and sign-in entries.
- [The over-the-shoulder flow shows no progress phases.] → Accepted. The page still shows Installing and the outcome.
- [Machine mode is decided by the root's path.] → Only the wizard's all-users mode installs there, and the folder can't be changed in that mode.

## Migration Plan

Nothing to migrate: no all-users install exists yet. Per-user installs keep auto-apply on start and every current behaviour.
