# Deferred checks by hand

By the owner's decision of 2026-11-14, the checks that need a person at the keyboard are done once, in a final acceptance pass after all roadmap changes, together with any fixes they lead to. Each change adds its section here instead of waiting for them; its own `docs/manual-tests/<change>.md` keeps the automated checks and points here. Each section names the commit its steps were written against; run them on the final build, and note where behaviour changed since.

## Final acceptance run (add-final-acceptance)

- **Pinned commit:** `main` at `1955903405dc6b77cb70a90f16d7f1a2026c57b0` (merge of PR #42; contains changes 1–14). CI, CodeQL and Snyk passed on it. Desktop-check sets passed on it (orchestrator run on the owner's PC): tray-and-settings 10/10, onboarding 9/9, full 36/36.
- **Build:** a Debug build of `src/Rekey.App` from that commit (`dotnet build src/Rekey.App`), unless a step asks for a Release build or the installer. Since `add-tray-and-settings-ui` most settings that steps change in `settings.json` can also be changed in the Settings window; either way works.
- **How to record:** a passing item gets `[x]`; leave its `Observed:` empty or add a short note. A failing or unclear item stays `[ ]` and its `Observed:` line says what happened (steps that differed, what was seen). Use only the invented Words of the step, no other typed text or window titles. An "Open item" is ticked once decided; write the decision on its `Observed:` line.
- **Re-runs:** items re-checked after fixes add `Re-run on <sha>: pass` to their `Observed:` line.

## add-manual-correction

Written against f4ec2f6 (PR #17). The automated `manual-correction` run passed on that commit (run 7); these steps cover what injected keys cannot.

The steps change the Hotkey in `%LOCALAPPDATA%\Rekey\settings.json` while Rekey is not running (Rekey writes the file on exit); on the final build Settings → Hotkey and the tray's Auto Correction item do the same at once. Use a Debug build of `src/Rekey.App`. These checks use the physical keyboard, which the tool cannot. Each step stands alone: it names the Hotkey it needs (quit Rekey, set `"hotkey"`, start Rekey again when it differs from the current one) and starts with English (US) active in the window it types in.

- [x] **Double tap of Shift (1.2, 2.1).** Start Rekey with `"autoCorrectionEnabled": false` and `"hotkey": "doubleShift"`. In Notepad type "ghbdtn" and tap Shift twice quickly. Switch back to English (US), press Enter for a new line, then type "Ghbdtn" holding Shift for the capital and tap Shift once.
  Expected: the first Word becomes "привет" and the Layout Russian; the second is left as typed (the Shift held for "G" does not count as a tap).
  Observed:
- [ ] **Win+Space with the physical keys (1.4).** Quit Rekey, set `"hotkey": "winSpace"`, start Rekey. In Notepad, in English (US), type "ghbdtn" and press Win+Space; then on a new empty line press Win+Space once more; then press Win alone.
  Expected: "ghbdtn" becomes "привет" and the Layout is Russian, with no Windows layout pop-up switching a second time and no Start menu; on the empty line the Layout becomes the one after Russian in Windows' order (English (US) when only those two are installed) and Start stays closed; Win alone opens Start as usual.
  Observed: Finding (task 2.1): on an empty line after Russian, Win+Space switched to Ukrainian instead of English (US). Layouts installed: English (US); Ukrainian with Ukrainian (Enhanced) and Russian keyboards. Rekey follows the load order of the layout handles, not the user's language list that Windows uses. The other steps passed.
- [x] **Selection a control does not expose (3.2).** Quit Rekey, set `"hotkey": "doubleShift"`, start Rekey. Open the Run dialog (Win+R), switch to English (US), type "ghbdtn", select it with Shift+Home and tap Shift twice.
  Expected: if the field exposes its selection, it becomes "привет"; if it does not, the text stays "ghbdtn" and only the Layout switches. Report which happened.
  Observed: The Run field does not expose its selection: only the Layout switched.
- [x] **Password Field (2.3).** With `"hotkey": "doubleShift"`, on any sign-in page, switch to English (US), type "ghbdtn" in the password box and tap Shift twice.
  Expected: nothing happens: the box keeps its characters and the Layout stays English (US).
  Observed:

### Open item to evaluate

- [x] **Keys typed during a multi-part replacement.** Since 1ded4ab, a replacement with spaces is typed in parts with a 50 ms pause after each space (design D4 of add-manual-correction), because Notepad garbles text after a space in one burst of injected input. The hook holds typed keys behind a Correction for at most 150 ms (design D1 of add-text-replacement), so keys typed after that can land between the parts, inside the replacement: in a run of two or three Deferred Words corrected while typing fast, or while typing during a long selection conversion right after the Hotkey. Kept as is for now (owner decision 2026-11-14). At the final pass, try both cases and decide whether to keep it or to let the worker extend the hook's hold while a multi-part replacement types, which changes the 150 ms rule and would be its own change.
  Observed: Decision (owner): hold typed keys while a multi-part replacement types; this changes the 150 ms rule (task 2.2).

## add-word-learning

Written against the head of the `add-word-learning` PR. The rules are covered by unit tests; these steps check the app wiring. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed and default settings. The app has no word list editors before `add-tray-and-settings-ui`; inspect `%LOCALAPPDATA%\Rekey\words.json` with Rekey closed.

- [x] **Undo protects, and survives a restart (3.1, 4.1).** Quit Rekey, delete `words.json` if present, and start Rekey. In Notepad, in English (US), type "ntcn" and Space: it becomes "тест ". Press Ctrl+Z within 5 s: it is "ntcn " again. Wait 3 s, then type "ntcn" and Space once more. Quit Rekey from the tray, start it, switch to English (US), type "ntcn" and Space.
  Expected: the second and third "ntcn" stay as typed; `words.json` lists "ntcn" under `protected` with `"source": "auto"`.
  Observed:
- [x] **Manual Correction learns (3.2).** In Notepad, in English (US), type "rhby;" and Space: it stays. Press the Hotkey: it becomes "кринж ". Switch to English (US) and type "rhby;" and Space.
  Expected: the second "rhby;" becomes "кринж" on its own; `words.json` lists "кринж" under `learned` with `"language": "ru"` once Rekey has quit.
  Observed:
- [x] **Unreadable file is kept aside (1.2).** Quit Rekey, replace the content of `words.json` with `{ broken`, start Rekey.
  Expected: Rekey starts as usual; `words.json.bak` holds `{ broken`.
  Observed:

## add-tray-and-settings-ui

Written against the head of the `add-tray-and-settings-ui` PR. The automated `tray-and-settings` set covers the tray Language, a setting applying at once, the single settings window, the indicator and the sound; these steps cover what needs eyes, ears, real apps or a sign-out. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed. Pin the Rekey icon to the visible part of the notification area first (Settings → Personalization → Taskbar → Other system tray icons).

- [x] **Keycap look (2.1).** Run `dotnet test tests/Rekey.App.Tests` and open `tests/Rekey.App.Tests/bin/Debug/net10.0-windows/keycap-sheet.png`.
  Expected: every row reads "EN" at 16, 20, 24 and 32 px; paused has an amber edge, off a faded legend, paused-itself a red edge and dot; the dark rows suit a dark taskbar.
  Observed: Matches the spec. The owner wants the tray icons redesigned: its own change, not a fix here.
- [x] **Tray icon and menu (2.2).** Click the tray icon with the left and then the right button. Try each item: Auto Correction off and on, Pause → For 15 minutes then Resume, Pause → Until Rekey restarts then quit and start Rekey, "Turn off in Notepad" after typing in Notepad (and "Turn on in Notepad" after), Settings, About, Quit. Hover the icon while paused.
  Expected: the menu opens on both clicks and follows the Windows theme; each item does what it says; the icon's edge turns amber while paused and the legend fades while off; the tooltip says how many minutes remain; the pause until restart ends with the restart; Check for updates is greyed out (Statistics opens the statistics window since add-statistics-and-diagnostics; since add-auto-update-and-installer the update item works when Rekey was installed by its installer); the icon shows "RU" while typing in Russian.
  Observed: On the first try a menu item took effect only after more than 10 s (Pause, Quit); not reproduced on later tries (Settings, About, Pause by mouse and keyboard all at once). No Incident.
- [x] **Pause ends while the menu is open (2.2).** Pause → For 15 minutes, then 14 minutes later open the tray menu, move to Resume with the arrow keys and leave the menu open past the end of the Pause (the menu refreshes every 20 s). Close the menu and open it again.
  Expected: while open, the menu keeps its items and the highlighted Resume; after reopening it shows Pause instead of Resume.
  Observed:
- [x] **Sensitivity (3.2).** In Correction, pick Cautious and type a short wrong-layout Word such as "yt" + Space in Notepad; pick Aggressive and type it again.
  Expected: the change applies to the next Word without restarting Rekey (report what each setting did).
  Observed:
- [ ] **Hotkey page (3.3).** Pick Win+Space, then record Win+Space with "My own combination", then record Ctrl+Alt+Space.
  Expected: Win+Space shows the warning that it takes over the Windows layout shortcut and keeps the previous Hotkey until "Use it anyway"; "Pick another" keeps the previous one; Ctrl+Alt+Space becomes the Hotkey at once and works in Notepad.
  Observed: Finding (task 2.3): "My own combination" does not record Ctrl+Alt+Space and keeps the previous combination, shown as "Ctrl+Shift+0x6B" (a raw key code instead of a key name). The Win+Space warning, "Use it anyway" and "Pick another" pass.
- [x] **Apps and words (3.4).** Add Windows Terminal with "Add a running app", add a program with "Choose a program…", remove both. Add and remove a Protected Word and a Learned Word (with a language).
  Expected: added apps show their name and icon and Rekey stays off in them; removed ones correct again; added words appear at once and change the next Word as the word-learning spec says; removed ones disappear; `words.json` matches after Rekey quits.
  Observed: Passes with "Add a running app". Finding (task 2.4): a program chosen with "Choose a program…" as C:\Windows\notepad.exe does not exclude Notepad: on Windows 11 that file starts the Store Notepad, whose path differs.
- [ ] **Accessibility (3.5).** Fill in [settings-accessibility.md](settings-accessibility.md).
  Observed: Finding (task 2.5): with the Ukrainian interface Narrator reads "////" for each Settings section (icon glyphs). The other Narrator rows wait for the re-check after task 2.10.
- [ ] **Indicator in real apps (4.1).** Type "ghbdtn" + Space in Notepad, then in VS Code, then in a browser text box.
  Expected: in Notepad the badge appears just below the caret; in VS Code and the browser (no system caret) it appears next to the mouse pointer; typing goes on without a pause or lost key; with "Show animations in Windows" off the badge disappears without fading.
  Observed: Finding (task 2.6): the badge sits in an opaque box of about 460x320 px (light or dark grey) in every app. Placement: under the caret in Notepad and the browser, next to the pointer in VS Code. No pause, no lost keys, no fade with animations off.
- [ ] **Sounds (4.2).** In General, preview each of the three sounds, turn the sound on and correct a Word, then press Ctrl+Z.
  Expected: each preview plays its own sound; one sound per Correction, none for the Undo.
  Observed: Finding (task 2.7): Preview plays only every second or third press, and only the bell sound is ever heard; the other two are silent on Preview. One sound per Correction, none on Undo: pass.
- [x] **Launch at sign-in (4.3).** Turn on "Start Rekey when I sign in to Windows", sign out and sign in. Then turn Rekey off under Task Manager → Startup apps and open Settings.
  Expected: Rekey is running with its tray icon after sign-in; after the Task Manager change the switch shows off; turning it on again starts Rekey at the next sign-in.
  Observed:

## add-statistics-and-diagnostics

Written against the head of the `add-statistics-and-diagnostics` PR. Unit tests cover the counters, the Incident log, the word-analysis log (including its ACL) and the export text; these steps cover what needs eyes. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed. Data lives in `%LOCALAPPDATA%\Rekey`.

- [x] **Statistics window with seeded data (4.1).** Quit Rekey and save this as `stats.json` (change both dates to today and yesterday): `{"version":1,"days":[{"day":"2026-11-15","counts":{"wordsAnalysed":30,"autoCorrections":4,"undos":1,"keepDecisions":26,"correctDecisions":4},"durations":null},{"day":"2026-11-16","counts":{"wordsAnalysed":10,"manualCorrections":1},"durations":null}]}`. Start Rekey and choose Statistics in the tray menu. Then type "ghbdtn" + Space in Notepad and press Refresh.
  Expected: Today shows 10 Words analysed and 1 correction; Last 30 days shows 40 Words analysed, 5 corrections, 1 undo and 20 % undone; durations show "—" until the new correction, then a few milliseconds; choosing Statistics again brings the same window forward; the window follows the Windows theme and a screen reader reads each row's name.
  Observed: Seed dates were a day off (run on 2026-11-17), so Today showed only real typing; every total matched seed plus typing.
- [x] **Statistics off (1.2, 4.1).** In Settings → Diagnostics and privacy turn "Keep statistics" off, open Statistics, then quit Rekey.
  Expected: `stats.json` is gone at once; the window shows zeros; nothing is counted until the switch is on again.
  Observed:
- [x] **Word-analysis log (3.2, 3.3).** Turn the word-analysis log on, type "ghbdtn" + Space and "hello" + Space in Notepad, then open `analysis\analysis-<today>.log` and the file's Properties → Security. Turn the log off.
  Expected: two lines with the Words, their Russian Renderings and Correct / Keep; Security lists only your account and SYSTEM; after turning off, the `analysis` folder holds no log file.
  Observed: A Word typed within 2 s of the owner's own Layout Switch was neither corrected nor logged, as the auto-correction spec requires. Owner decision: keep the rule, make its setting text clearer (task 2.8).
- [x] **Export dialog (4.3).** Open Settings → Diagnostics and privacy → Export diagnostics…, read the preview, tick and untick "Include word-analysis log", choose Save to file, accept the suggested Desktop name, open the saved file; then choose Copy and paste into Notepad.
  Expected: the preview shows versions, Layouts, settings, statistics and Incidents with `%USERPROFILE%`, `%USERNAME%` and `%COMPUTERNAME%` in place of your folder, name and PC name, and no typed words unless the box is ticked; the saved file and the pasted text equal the preview character for character.
  Observed: Notes: the word-analysis part of the preview is tab-separated, so columns misalign and the decision runs into the Rendering, and long lines are cut with no scroll (task 2.9); 17 SlowHookCallback Incidents over two days (task 2.9 looks at them).
- [x] **Open item: analysis-log write on the worker thread.** With the opt-in word-analysis log on, a slow disk write (> 150 ms) can let a later key overtake a held Trigger Key; evaluate whether to move the write off the worker thread (CodeRabbit follow-up on PR #24).
  Observed: Decision (owner): move the write off the worker thread (task 2.9).
- [x] **Open item: expired analysis-log days while idle.** Days older than 7 are deleted on start, when the log is turned on and on the first write of a new day; with the log on and no typing for days, an expired file stays until then. Evaluate a periodic prune (CodeRabbit finding on PR #25).
  Observed: Decision (owner): keep as is, no periodic prune.
- [x] **Failed delete when the log is turned off.** Fixed by PR #26 (CodeRabbit finding on PR #25): Rekey now retries files it could not delete. With the log on and a few Words typed, hold today's `analysis\analysis-<today>.log` open (PowerShell `$f = [IO.File]::Open($path, 'Open', 'Read', 'None')`), turn the log off, then release it (`$f.Close()`) and wait a few seconds.
  Expected: right after turning off, the held file is still there and every other log file is gone; a few seconds after the release the held file is gone too.
  Observed: Run from a Start-menu PowerShell (the Claude app terminal sees a virtualized AppData).

## add-auto-update-and-installer

Written against the head of the `add-auto-update-and-installer` PR. Unit tests cover the data folder, the version gate, the update scheduler, the tray item text and the update manager against a fake feed; these steps need an installer and a person. Build packages as `docs/release.md` describes ("Local test build"); use a standard (non-administrator) Windows account and a machine without an earlier Rekey install. The update steps need a Debug build packed twice (for example 0.1.0 and 0.2.0) into one local feed folder and signed with the test certificate from `docs/release.md`, started with `--update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>`.

- [x] **Per-user install (2.1).** Run `key-switcher-win-Setup.exe`.
  Expected: no UAC prompt; Rekey starts with its tray icon; "Rekey" appears in the Start menu and in Settings → Apps → Installed apps; files are under `%LOCALAPPDATA%\key-switcher`, data under `%LOCALAPPDATA%\Rekey`.
  Observed: Notes (task 2.12, 2.11): the welcome window scrolls with the wheel only over its scroll bar; the exe and the Installed apps entry have no icon; the publisher reads "key-switcher".
- [x] **Update from a local feed (2.2).** Install 0.1.0 (Debug), quit it, start `%LOCALAPPDATA%\key-switcher\current\Rekey.App.exe --update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>` with 0.2.0 in the folder, open the tray menu; choose "Restart to update". The restarted Rekey uses GitHub Releases again, so quit it, start it once more with the same `--update-feed` and `--update-pin` arguments, and choose the item again with no newer version.
  Expected: the item reads "Checking for updates…", then "Restart to update"; Rekey restarts as 0.2.0 (About page); the next check says "Rekey is up to date"; with "Check for updates automatically" off, no check runs at start.
  Observed: The first try said "up to date": the installer-started Rekey was still running, so the second launch with --update-feed exited without notice (task 2.13). Not checked: no check at start with automatic checks off.
- [x] **Update keeps user data (2.3).** Before the update above, change a few settings, add a Protected Word, turn on "Start Rekey when I sign in to Windows" and type a few Words for statistics.
  Expected: after the restart into 0.2.0 the settings, the Protected Word and the statistics are unchanged, and Rekey still starts at the next sign-in.
  Observed:
- [x] **Tampered and unsigned packages (3.2).** In the local feed, replace the 0.2.0 `.nupkg` with a copy that has one byte changed (keep `releases.win.json`), and choose "Check for updates". Then restore a 0.2.0 package packed without signing (checksums updated by `vpk pack`) and check again.
  Expected: both times the item reads "Could not check for updates", 0.1.0 keeps running, `%LOCALAPPDATA%\key-switcher\packages` holds no 0.2.0 package, and `%LOCALAPPDATA%\Rekey\incidents.jsonl` gains `UpdateChecksumFailed`, then `UpdateSignatureFailed` with `SignatureVerdict.NotSigned`.
  Observed: Note (task 2.8): the tray item reads "Turn off in Windows Terminal Host" (process description) instead of the app name.
- [x] **Uninstall, keeping data (4.1).** With Rekey installed and "Start Rekey when I sign in to Windows" on, uninstall it from Installed apps, wait a minute before answering the question that appears, and answer No.
  Expected: Rekey closes; `%LOCALAPPDATA%\key-switcher`, the Start menu entry and the Run entry (Task Manager → Startup apps) are gone; `%LOCALAPPDATA%\Rekey` is still there; installing again picks up the old settings and word lists.
  Observed: After reinstalling, "Start Rekey when I sign in" is off although it was on: uninstall removes the Run entry. Owner decision: restore it through an install-time question, in its own change.
- [x] **Uninstall, deleting data (4.1).** Uninstall again and answer Yes.
  Expected: `%LOCALAPPDATA%\Rekey` no longer exists and Rekey does not start at the next sign-in.
  Observed:
- [x] **Uninstall with a locked data file (4.1).** Install again, open `%LOCALAPPDATA%\Rekey\settings.json` in a program that locks it (for example Excel, or PowerShell `[IO.File]::Open($path, 'Open', 'Read', 'None')`), uninstall and answer Yes.
  Expected: after a few seconds a warning names the folder to delete by hand; once the lock is released the folder can be deleted.
  Observed:
- [x] **Open item: reinstall while the uninstall question is open.** The data question deletes `%LOCALAPPDATA%\Rekey` on Yes without checking whether Rekey was installed again while it was open, so a new installation's data could be lost. Evaluate an installation marker check before deletion (CodeRabbit finding on PR #32, deferred by orchestrator decision).
  Observed: Decision (owner): check the installation marker before deleting data (task 2.12).
- [x] **Release environment and dry run (4.2).** Owner: create the `release` environment limited to `v*` tags with the `VPK_SIGN_PARAMS` secret (`docs/release.md`, One-time setup). After the change is on `main`, run Release by hand with version `0.0.1`.
  Expected: the run passes, its Pack log says no signing parameters were provided, it creates no release, and the artifact holds `key-switcher-win-Setup.exe` and the `.nupkg`.
  Observed: Run on main: green, no release, artifact with Setup and .nupkg. Finding (task 2.12): Pack warns "No architecture specified with --runtime, defaulting to x86" while publish uses win-x64.
- [x] **Local signed test build (4.3).** Follow `docs/release.md` → Local test build on a clean checkout without help.
  Expected: two signed packages in one feed folder, which the update steps above use; note any step that needed guessing.
  Observed: Notes (task 2.14): the "keep a copy" step names no place (the C:\ root is not writable for a standard user), and the doc does not say to clear the publish folder between the two versions.


## add-onboarding

Written against the head of the `add-onboarding` PR. Unit tests cover when the welcome opens, the first-start sign-in default, the choices, the Layout notice, the one-time tip and the location classifier; the automated `onboarding` set covers the welcome opening at first start, the try-it Correction and its confirmation, and reopening from the tray. These steps need eyes, a physical keyboard or an installed build. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed unless a step says otherwise. To see the first start again, quit Rekey and delete `%LOCALAPPDATA%\Rekey\settings.json`. The location steps need a Release build published as one file, as the Release workflow does: `dotnet publish src/Rekey.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o <folder>` (a multi-file publish cannot run from inside a ZIP: Explorer extracts only the exe). Since add-final-acceptance the tray item is named "Getting started with Rekey" ("Знайомство з Rekey").

- [ ] **Look and keyboard (2.1).** At a first start, look at the welcome window in light mode, then switch Windows to dark mode (Settings → Personalization → Colors) with the window open. Then use only the keyboard: Tab through every control and press Space or Enter on each.
  Expected: keycap, description, Undo hint, privacy statement, try-it field, Auto Correction and sign-in switches, Hotkey choice, tray tip and Done read well in both modes and follow the switch at once; every control is reachable in reading order with a visible focus mark; Enter or Space on a focused Done closes the window, while Enter in the try-it field does not; Narrator reads the try-it hint with the field.
  Observed: Finding (task 2.10): themes, Tab order and Enter pass; with the Ukrainian interface Narrator reads only the English words and skips Cyrillic.
- [x] **Try it with the physical keyboard (2.2).** At a first start, with English (US) active, type "ghbdtn" and Space in the try-it field.
  Expected: the field shows "привет ", the Layout is Russian and the green confirmation appears; no "Rekey corrected a word" notification follows.
  Observed:
- [x] **Tray item (2.4).** Complete the welcome, then open the tray menu with a click on the Rekey icon.
  Expected: the menu lists "Welcome and setup" above "About Rekey"; choosing it opens the welcome window with the current settings, and choosing it again while open brings the same window to the front.
  Observed:
- [x] **One Layout installed (2.3).** Remove every Layout but English (US) (keep a note to add Russian back), then open "Welcome and setup" from the tray. Choose "Open language settings". Add Russian back.
  Expected: an amber notice says Rekey needs at least two Layouts; the button opens Settings → Time & language → Language & region; once Russian is back the notice disappears without reopening the window.
  Observed:
- [x] **One-time notification (2.5).** Complete the welcome, then in Notepad type "ghbdtn" and Space; type "ghbdtn" and Space again; quit and start Rekey and correct once more.
  Expected: only the first Correction shows the Windows notification "Rekey corrected a word — Press Ctrl+Z to undo."; later ones, also after the restart, show none.
  Observed: Passes with Do Not Disturb off. Finding (task 2.11): with Do Not Disturb on, the tip was marked shown although Windows showed nothing, so it never came again.
- [x] **Closing without changes (spec: launch at sign-in default).** At a first start close the welcome window with its X without changing anything; open Task Manager → Startup apps.
  Expected: Rekey is listed and enabled; the welcome does not open at the next start.
  Observed:
- [ ] **Run from inside a ZIP (3.1).** Zip the Release publish folder, open the ZIP in Explorer and double-click `Rekey.App.exe` inside it (choose Run if Explorer asks). Choose Install.
  Expected: a "Rekey is not installed" message says this copy stops working when removed and offers Install and Not now; Install opens the latest release page in the browser (no setup in the ZIP). Repeat with `key-switcher-win-Setup.exe` placed next to `Rekey.App.exe` in the ZIP: Install starts that setup.
  Observed: Finding (task 2.14): the ZIP held a plain multi-file publish; Explorer extracts only the exe, which fails at once (Rekey.App.dll missing), so no warning can show. Release builds publish a single file; the step now does the same.
- [x] **Run from Downloads and from the installed location (3.1).** Copy the publish folder to your Downloads folder and start it there; then install Rekey with its setup and start the installed one.
  Expected: the Downloads copy shows the warning; the installed Rekey shows none.
  Observed:
- [ ] **Release build moved while running (3.2).** Start the Release build from a folder of its own, then rename `Rekey.App.exe` in Explorer (Windows does not let a folder be renamed while an exe in it runs).
  Expected: within a minute "Rekey's files were moved or deleted" explains that Rekey will not start again and offers Install; Rekey keeps correcting until you quit it.
  Observed: Finding (task 2.14): Windows does not let a folder be renamed while the exe in it runs; renaming the exe file instead showed the notice with Install and Close.

## add-telemetry

Written against the head of PR #39. Precondition for the network checks: set the endpoints in `src/Rekey.App/Telemetry/TelemetryEndpoints.cs` to a test backend (none exists yet, so a shipped build sends nothing) and capture traffic with a local HTTPS proxy such as Fiddler, trusting its root certificate only for this test.

- [x] **Welcome consent (4.2).** Start with a fresh data folder; in the welcome window look at the diagnostics checkbox, then close the window without ticking it.
  Expected: the checkbox reads "Send anonymous crash reports and daily usage counts…", is unticked, and the proxy shows no request from Rekey.
  Observed: Visual only (no test backend).
- [x] **Settings toggles (4.2).** Open Settings → Diagnostics and privacy.
  Expected: "Send crash and freeze reports" and "Send daily usage counts" switch independently, the note under them states what reports carry, and the page reads well in light and dark themes and with Narrator.
  Observed: Visual only (no test backend).
- [ ] **Email sign-up (3.3).** With the proxy running, enter an invented address, tick "Tell me about new versions" and press Sign up.
  Expected: exactly one request to the sign-up endpoint with that address and purpose; the field empties and the thanks message shows; `sent.jsonl` in the data folder does not contain the address.
  Observed: Not run: no telemetry test backend.
- [ ] **Show what was sent (4.1).** With usage counts on, wait for the daily digest (or set the delay low in a debug build), then expand "Show what was sent".
  Expected: the last entry equals, byte for byte, the request body the proxy captured.
  Observed: Not run: no telemetry test backend.

## add-ui-localization

Written against the head of PR #41. The interface has English and Ukrainian only (owner's decision); there is no Russian interface. Unit tests cover the default mapping, plural forms, translation completeness, that XAML shows no literal text, and that bound text follows a language change. Record the sign-off of 3.1 (reviewer, date, build) in `docs/manual-tests/localization.md` when these pass.

- [x] **Native-speaker review (3.1).** A native Ukrainian speaker reads every string of `src/Rekey.App/Strings.uk.resx` in place: the tray tooltip and menu, each settings page, statistics, export, welcome, the correction tip, the location and moved notices, and the uninstall questions.
  Expected: wording is natural and consistent (one term per concept, such as "розкладка", "виправлення", "гаряча клавіша"); nothing reads as machine translation.
  Observed: Passes except the tray item "Вітання та налаштування", which reads like the Settings item: rename (task 2.8).
- [ ] **Layouts in Ukrainian (3.1).** With the interface in Ukrainian, open every window and settings page at 100 % and 150 % scaling.
  Expected: no text is cut off, overlaps or forces a horizontal scroll bar; buttons fit their labels.
  Observed: Finding (task 2.8): the Settings section list cuts "Діагностика та конфіденційність" and shows a horizontal scroll bar; everything else fits at 100 % and 150 %.
- [x] **Default language (1.1).** Start Rekey with `"uiLanguage"` absent from `settings.json` on Windows with the display language Ukrainian, then Russian, then German (or English).
  Expected: Ukrainian, Ukrainian, English.
  Observed: Not run (owner decision): unit tests cover the mapping, and "Як у Windows" gave Ukrainian in Live switching.
- [x] **Live switching (2.4).** Open Settings, Statistics and the welcome window. In Settings → General pick English, then Українська, then Як у Windows.
  Expected: each pick changes the text of all three open windows, the section list, the combo boxes (including the selected entry), the tray tooltip and the tray menu at once, without a restart; the choice persists after a restart.
  Observed:
- [x] **Plural forms (1.2).** In Ukrainian, pause for 15 minutes and hover the tray icon as the time runs down.
  Expected: "залишилося 15 хвилин", … "залишилося 2 хвилини", "залишилася 1 хвилина".
  Observed:
- [x] **Uninstall questions (installer prompts).** Uninstall Rekey on Ukrainian or Russian Windows.
  Expected: the questions about deleting data are in Ukrainian. They follow the Windows display language, not the setting picked in Rekey (uninstall does not read settings).
  Observed: Note (task 2.12): the "removal complete" box appears together with the keep-data question; the question should come first.
- [ ] **Screen reader.** With Narrator, move through the language combo box and the settings section list in both languages.
  Expected: each entry is read by its text in the current language, not by a type name.
  Observed: Moved to the re-check after task 2.10.
