# Tasks

## 1. Acceptance pass

- [x] 1.1 Pin the commit of `main` that contains changes 1–14 and run CI plus every `tools/Rekey.DesktopChecks` set on it; verify all pass and note the SHA at the top of `docs/manual-tests/deferred-by-hand.md` (passed on 1955903: CI, CodeQL, Snyk; desktop checks tray-and-settings 10/10, onboarding 9/9, full 36/36)
- [x] 1.2 Owner runs every unchecked item of `docs/manual-tests/deferred-by-hand.md` on that SHA; verify each item is either ticked or carries a finding (steps, expected, observed) (done 2026-11-17; Email sign-up and Show what was sent not run: no telemetry test backend)
- [x] 1.3 Add one task per finding to group 2, named by capability; verify every finding in the file has a matching task here

## 2. Fixes

For each finding: when the spec is wrong, revise it with `/opsx:update` first; then write a failing test, fix, and verify the test and the full automated suite pass (one commit per capability). Owner requests that need new behaviour (tray icon redesign, start-up window, install-time questions, update check on the About page, ideas from the original) are not fixed here; the orchestrator takes them to the owner as their own changes.

- [x] 2.1 layout-management: order installed Layouts by the user's language list, not the load order of their handles, so Win+Space with nothing to correct picks the Layout Windows would; verify a unit test with the owner's list (English (US); Ukrainian with Ukrainian (Enhanced) and Russian) picks English (US) after Russian
- [x] 2.2 text-replacement: hold typed keys while a multi-part replacement types (owner decision; spec delta first); verify a test where keys arrive between the parts and land after the whole replacement
- [x] 2.3 tray-and-settings, Hotkey page: "My own combination" records Ctrl+Alt+Space, and a recorded combination shows key names, not raw codes; verify unit tests for both
- [x] 2.4 tray-and-settings, Excluded Apps: a program chosen by file also matches a running Store app with the same file name, which covers the Windows 11 Notepad stub (spec delta first); verify a unit test
- [x] 2.5 tray-and-settings, accessibility: Settings section items expose their text, not icon glyphs, to screen readers; verify a test on the automation name
- [x] 2.6 tray-and-settings, indicator: the indicator window is transparent and the size of the badge; the spec already allows the caret placement seen in browsers, so it stays; verify by hand in 3.1 (the cause is WPF UI painting the application's main window, which a unit test cannot host: no `Application` in the test process)
- [x] 2.7 tray-and-settings, sounds: Preview plays the chosen sound on every press, for each of the three sounds; verify by ear in 3.1 on the speakers and the DisplayPort monitor (cause on the monitor: its audio link sleeps after a pause and drops a sound's start; typing keeps it awake with silence, see design), with SoundSchedulerTests and WaveFileTests
- [x] 2.8 tray-and-settings texts and layout (the wider section list landed with 2.5; the welcome item is "Getting started with Rekey" / "Знайомство з Rekey", the product name taken from one place for the coming rename): wider section list (no cut-off, no horizontal scroll bar in Ukrainian), clearer text for the setting that skips the Word after the user's own Layout Switch, the tray item names a Store app by its app name, and the welcome item gets a name distinct from Settings; verify unit tests where logic changes, by hand for the look (deferred to 3.1)
- [x] 2.9 statistics-and-diagnostics (SlowHookCallback: the callback only judges the key and writes the ring; one Incident per 10 minutes stands for any callback over 1 ms, which garbage-collection pauses and JIT warm-up in a Debug build cause on their own, so no Rekey-side cause was found and nothing changes there): the export preview shows the word-analysis lines in aligned columns and scrolls; the analysis-log write leaves the worker thread (owner decision); look at the SlowHookCallback Incidents and fix any cause in Rekey; verify unit tests
- [x] 2.10 localization: every window's language follows the interface language, also on a live switch, so screen readers read Ukrainian text; verify a test on a window's language after a switch
- [x] 2.11 onboarding: the welcome window scrolls with the wheel anywhere over it; the one-time tip counts as shown only once Windows reports the balloon shown, and a later Correction sends it again until then; verify unit tests and by hand in 3.1 (Do Not Disturb on, then off)
- [x] 2.12 auto-update (the publisher name is "Rekey" until the owner names another; the dry-run Release workflow with the new `vpk pack` arguments is run by the orchestrator in 3.1): exe and installed app carry the app icon and a publisher name; `vpk pack` gets `--runtime win-x64`; uninstall deletes data only when no new installation exists, and asks the data question before the "removal complete" box (spec delta first); verify unit tests and the dry-run Release workflow
- [x] 2.13 app-lifecycle: a second start with arguments while Rekey runs tells the user Rekey is already running instead of exiting silently (spec delta first); verify a unit test
- [x] 2.14 docs: `docs/release.md` names where to keep the first version and says to clear the publish folder between versions; the ZIP step publishes a single file like the Release workflow; the moved-build step renames the exe file; verify by reading

## 3. Re-check and close

- [ ] 3.1 Owner re-runs by hand the items touched by group 2 on the new head; verify each is ticked with that SHA noted
- [ ] 3.2 Tick the deferred by-hand tasks of the archived changes, each with a pointer to the run in `deferred-by-hand.md`; verify no by-hand task in `openspec/changes/archive/*/tasks.md` is left unticked

## Workflow follow-up

- Deferred by the owner (2026-11-18), own task later: switch sounds still dropped at random on DisplayPort/HDMI monitor audio (2.7; tried pinned memory, a lead-in, a silence loop, one open waveOut stream), and the Undo tip lost in Windows 11 Do Not Disturb (2.11; tried a notification-state gate, ignoring quiet time, counting it shown on NIN_BALLOONSHOW). See design, Known limitations.

- Archive the change after its PR is merged; if group 2 added delta specs, the archive syncs them to `openspec/specs/`.
