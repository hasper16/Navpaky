# Deferred checks by hand

By the owner's decision of 2026-11-14, the checks that need a person at the keyboard are done once, in a final acceptance pass after all roadmap changes, together with any fixes they lead to. Each change adds its section here instead of waiting for them; its own `docs/manual-tests/<change>.md` keeps the automated checks and points here. Each section names the commit its steps were written against; run them on the final build, and note where behaviour changed since.

The sections up to `add-ui-localization` were run while the product was called Rekey; `rename-to-navpaky` renamed the names, paths and commands in them without re-running them. Before that rename the install root was `%LOCALAPPDATA%\key-switcher` and the data folder `%LOCALAPPDATA%\Rekey`.

## Final acceptance run (add-final-acceptance)

- **Pinned commit:** `main` at `1955903405dc6b77cb70a90f16d7f1a2026c57b0` (merge of PR #42; contains changes 1–14). CI, CodeQL and Snyk passed on it. Desktop-check sets passed on it (orchestrator run on the owner's PC): tray-and-settings 10/10, onboarding 9/9, full 36/36.
- **Build:** a Debug build of `src/Navpaky.App` from that commit (`dotnet build src/Navpaky.App`), unless a step asks for a Release build or the installer. Since `add-tray-and-settings-ui` most settings that steps change in `settings.json` can also be changed in the Settings window; either way works.
- **How to record:** a passing item gets `[x]`; leave its `Observed:` empty or add a short note. A failing or unclear item stays `[ ]` and its `Observed:` line says what happened (steps that differed, what was seen). Use only the invented Words of the step, no other typed text or window titles. An "Open item" is ticked once decided; write the decision on its `Observed:` line.
- **Re-runs:** items re-checked after fixes add `Re-run on <sha>: pass` to their `Observed:` line.

## add-manual-correction

Written against f4ec2f6 (PR #17). The automated `manual-correction` run passed on that commit (run 7); these steps cover what injected keys cannot.

The steps change the Hotkey in `%APPDATA%\Navpaky\settings.json` while Navpaky is not running (Navpaky writes the file on exit); on the final build Settings → Hotkey and the tray's Auto Correction item do the same at once. Use a Debug build of `src/Navpaky.App`. These checks use the physical keyboard, which the tool cannot. Each step stands alone: it names the Hotkey it needs (quit Navpaky, set `"hotkey"`, start Navpaky again when it differs from the current one) and starts with English (US) active in the window it types in.

- [x] **Double tap of Shift (1.2, 2.1).** Start Navpaky with `"autoCorrectionEnabled": false` and `"hotkey": "doubleShift"`. In Notepad type "ghbdtn" and tap Shift twice quickly. Switch back to English (US), press Enter for a new line, then type "Ghbdtn" holding Shift for the capital and tap Shift once.
  Expected: the first Word becomes "привет" and the Layout Russian; the second is left as typed (the Shift held for "G" does not count as a tap).
  Observed:
- [x] **Win+Space with the physical keys (1.4).** Quit Navpaky, set `"hotkey": "winSpace"`, start Navpaky. In Notepad, in English (US), type "ghbdtn" and press Win+Space; then on a new empty line press Win+Space once more; then press Win alone.
  Expected: "ghbdtn" becomes "привет" and the Layout is Russian, with no Windows layout pop-up switching a second time and no Start menu; on the empty line the Layout becomes the one after Russian in Windows' order (English (US) when only those two are installed) and Start stays closed; Win alone opens Start as usual.
  Observed: Finding (task 2.1): on an empty line after Russian, Win+Space switched to Ukrainian instead of English (US). Layouts installed: English (US); Ukrainian with Ukrainian (Enhanced) and Russian keyboards. Navpaky follows the load order of the layout handles, not the user's language list that Windows uses. The other steps passed. Re-run on 359c730 (task 2.1): pass, Win+Space after Russian goes to English (US).
- [x] **Selection a control does not expose (3.2).** Quit Navpaky, set `"hotkey": "doubleShift"`, start Navpaky. Open the Run dialog (Win+R), switch to English (US), type "ghbdtn", select it with Shift+Home and tap Shift twice.
  Expected: if the field exposes its selection, it becomes "привет"; if it does not, the text stays "ghbdtn" and only the Layout switches. Report which happened.
  Observed: The Run field does not expose its selection: only the Layout switched.
- [x] **Password Field (2.3).** With `"hotkey": "doubleShift"`, on any sign-in page, switch to English (US), type "ghbdtn" in the password box and tap Shift twice.
  Expected: nothing happens: the box keeps its characters and the Layout stays English (US).
  Observed:

### Open item to evaluate

- [x] **Keys typed during a multi-part replacement.** Since 1ded4ab, a replacement with spaces is typed in parts with a 50 ms pause after each space (design D4 of add-manual-correction), because Notepad garbles text after a space in one burst of injected input. The hook holds typed keys behind a Correction for at most 150 ms (design D1 of add-text-replacement), so keys typed after that can land between the parts, inside the replacement: in a run of two or three Deferred Words corrected while typing fast, or while typing during a long selection conversion right after the Hotkey. Kept as is for now (owner decision 2026-11-14). At the final pass, try both cases and decide whether to keep it or to let the worker extend the hook's hold while a multi-part replacement types, which changes the 150 ms rule and would be its own change.
  Observed: Decision (owner): hold typed keys while a multi-part replacement types; this changes the 150 ms rule (task 2.2).

## add-word-learning

Written against the head of the `add-word-learning` PR. The rules are covered by unit tests; these steps check the app wiring. Use a Debug build of `src/Navpaky.App` with English (US) and Russian installed and default settings. The app has no word list editors before `add-tray-and-settings-ui`; inspect `%APPDATA%\Navpaky\words.json` with Navpaky closed.

- [x] **Undo protects, and survives a restart (3.1, 4.1).** Quit Navpaky, delete `words.json` if present, and start Navpaky. In Notepad, in English (US), type "ntcn" and Space: it becomes "тест ". Press Ctrl+Z within 5 s: it is "ntcn " again. Wait 3 s, then type "ntcn" and Space once more. Quit Navpaky from the tray, start it, switch to English (US), type "ntcn" and Space.
  Expected: the second and third "ntcn" stay as typed; `words.json` lists "ntcn" under `protected` with `"source": "auto"`.
  Observed:
- [x] **Manual Correction learns (3.2).** In Notepad, in English (US), type "rhby;" and Space: it stays. Press the Hotkey: it becomes "кринж ". Switch to English (US) and type "rhby;" and Space.
  Expected: the second "rhby;" becomes "кринж" on its own; `words.json` lists "кринж" under `learned` with `"language": "ru"` once Navpaky has quit.
  Observed:
- [x] **Unreadable file is kept aside (1.2).** Quit Navpaky, replace the content of `words.json` with `{ broken`, start Navpaky.
  Expected: Navpaky starts as usual; `words.json.bak` holds `{ broken`.
  Observed:

## add-tray-and-settings-ui

Written against the head of the `add-tray-and-settings-ui` PR. The automated `tray-and-settings` set covers the tray Language, a setting applying at once, the single settings window, the indicator and the sound; these steps cover what needs eyes, ears, real apps or a sign-out. Use a Debug build of `src/Navpaky.App` with English (US) and Russian installed. Pin the Navpaky icon to the visible part of the notification area first (Settings → Personalization → Taskbar → Other system tray icons).

- [x] **Keycap look (2.1).** Run `dotnet test tests/Navpaky.App.Tests` and open `tests/Navpaky.App.Tests/bin/Debug/net11.0-windows/keycap-sheet.png`.
  Expected: every row reads "EN" at 16, 20, 24 and 32 px; paused has an amber edge, off a faded legend, paused-itself a red edge and dot; the dark rows suit a dark taskbar.
  Observed: Matches the spec. The owner wants the tray icons redesigned: its own change, not a fix here.
- [x] **Tray icon and menu (2.2).** Click the tray icon with the left and then the right button. Try each item: Auto Correction off and on, Pause → For 15 minutes then Resume, Pause → Until Navpaky restarts then quit and start Navpaky, "Turn off in Notepad" after typing in Notepad (and "Turn on in Notepad" after), Settings, About, Quit. Hover the icon while paused.
  Expected: the menu opens on both clicks and follows the Windows theme; each item does what it says; the icon's edge turns amber while paused and the legend fades while off; the tooltip says how many minutes remain; the pause until restart ends with the restart; Check for updates is greyed out (Statistics opens the statistics window since add-statistics-and-diagnostics; since add-auto-update-and-installer the update item works when Navpaky was installed by its installer); the icon shows "RU" while typing in Russian.
  Observed: On the first try a menu item took effect only after more than 10 s (Pause, Quit); not reproduced on later tries (Settings, About, Pause by mouse and keyboard all at once). No Incident.
- [x] **Pause ends while the menu is open (2.2).** Pause → For 15 minutes, then 14 minutes later open the tray menu, move to Resume with the arrow keys and leave the menu open past the end of the Pause (the menu refreshes every 20 s). Close the menu and open it again.
  Expected: while open, the menu keeps its items and the highlighted Resume; after reopening it shows Pause instead of Resume.
  Observed:
- [x] **Sensitivity (3.2).** In Correction, pick Cautious and type a short wrong-layout Word such as "yt" + Space in Notepad; pick Aggressive and type it again.
  Expected: the change applies to the next Word without restarting Navpaky (report what each setting did).
  Observed:
- [x] **Hotkey page (3.3).** Pick Win+Space, then record Win+Space with "My own combination", then record Ctrl+Alt+Space.
  Expected: Win+Space shows the warning that it takes over the Windows layout shortcut and keeps the previous Hotkey until "Use it anyway"; "Pick another" keeps the previous one; Ctrl+Alt+Space becomes the Hotkey at once and works in Notepad.
  Observed: Finding (task 2.3): "My own combination" does not record Ctrl+Alt+Space and keeps the previous combination, shown as "Ctrl+Shift+0x6B" (a raw key code instead of a key name). The Win+Space warning, "Use it anyway" and "Pick another" pass. Re-run on 4d98b6c (task 2.3): pass, Ctrl+Alt+Space records (another app had it as a global hotkey; Navpaky now takes it from its keyboard hook) and the conflict warning shows.
- [x] **Apps and words (3.4).** Add Windows Terminal with "Add a running app", add a program with "Choose a program…", remove both. Add and remove a Protected Word and a Learned Word (with a language).
  Expected: added apps show their name and icon and Navpaky stays off in them; removed ones correct again; added words appear at once and change the next Word as the word-learning spec says; removed ones disappear; `words.json` matches after Navpaky quits.
  Observed: Passes with "Add a running app". Finding (task 2.4): a program chosen with "Choose a program…" as C:\Windows\notepad.exe does not exclude Notepad: on Windows 11 that file starts the Store Notepad, whose path differs.
- [x] **Accessibility (3.5).** Fill in [settings-accessibility.md](settings-accessibility.md).
  Observed: Finding (task 2.5): with the Ukrainian interface Narrator reads "////" for each Settings section (icon glyphs). The other Narrator rows wait for the re-check after task 2.10. Re-run on b50d5bb (tasks 2.5, 2.10): every Settings control has its name and no icon glyph reaches Narrator (SettingsAutomationTests); with the English interface Narrator reads names correctly. Narrator cannot speak Ukrainian on this PC: only an English voice is installed and Windows ships no Ukrainian one, so the Ukrainian names and Narrator's own Ukrainian position text ("Елемент 2 з 6") come out garbled. Environment limit.
- [x] **Indicator in real apps (4.1).** Type "ghbdtn" + Space in Notepad, then in VS Code, then in a browser text box.
  Expected: in Notepad the badge appears just below the caret; in VS Code and the browser (no system caret) it appears next to the mouse pointer; typing goes on without a pause or lost key; with "Show animations in Windows" off the badge disappears without fading.
  Observed: Finding (task 2.6): the badge sits in an opaque box of about 460x320 px (light or dark grey) in every app. Placement: under the caret in Notepad and the browser, next to the pointer in VS Code. No pause, no lost keys, no fade with animations off. Re-run on 438164f (task 2.6): pass, small badge at the Notepad caret, next to the pointer in Firefox (no system caret there).
- [x] **Sounds (4.2).** In General, preview each of the three sounds, turn the sound on and correct a Word, then press Ctrl+Z.
  Expected: each preview plays its own sound; one sound per Correction, none for the Undo.
  Observed: Finding (task 2.7): Preview plays only every second or third press, and only the bell sound is ever heard; the other two are silent on Preview. One sound per Correction, none on Undo: pass. Deferred (owner decision 2026-11-18): plays on the Realtek speakers; on the LG monitor's DisplayPort audio sounds are still dropped at random after four attempts (design, Known limitations).
- [x] **Launch at sign-in (4.3).** Turn on "Start Navpaky when I sign in to Windows", sign out and sign in. Then turn Navpaky off under Task Manager → Startup apps and open Settings.
  Expected: Navpaky is running with its tray icon after sign-in; after the Task Manager change the switch shows off; turning it on again starts Navpaky at the next sign-in.
  Observed:

## add-statistics-and-diagnostics

Written against the head of the `add-statistics-and-diagnostics` PR. Unit tests cover the counters, the Incident log, the word-analysis log (including its ACL) and the export text; these steps cover what needs eyes. Use a Debug build of `src/Navpaky.App` with English (US) and Russian installed. Data lives in `%LOCALAPPDATA%\Navpaky`.

- [x] **Statistics window with seeded data (4.1).** Quit Navpaky and save this as `stats.json` (change both dates to today and yesterday): `{"version":1,"days":[{"day":"2026-11-15","counts":{"wordsAnalysed":30,"autoCorrections":4,"undos":1,"keepDecisions":26,"correctDecisions":4},"durations":null},{"day":"2026-11-16","counts":{"wordsAnalysed":10,"manualCorrections":1},"durations":null}]}`. Start Navpaky and choose Statistics in the tray menu. Then type "ghbdtn" + Space in Notepad and press Refresh.
  Expected: Today shows 10 Words analysed and 1 correction; Last 30 days shows 40 Words analysed, 5 corrections, 1 undo and 20 % undone; durations show "—" until the new correction, then a few milliseconds; choosing Statistics again brings the same window forward; the window follows the Windows theme and a screen reader reads each row's name.
  Observed: Seed dates were a day off (run on 2026-11-17), so Today showed only real typing; every total matched seed plus typing.
- [x] **Statistics off (1.2, 4.1).** In Settings → Diagnostics and privacy turn "Keep statistics" off, open Statistics, then quit Navpaky.
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
- [x] **Failed delete when the log is turned off.** Fixed by PR #26 (CodeRabbit finding on PR #25): Navpaky now retries files it could not delete. With the log on and a few Words typed, hold today's `analysis\analysis-<today>.log` open (PowerShell `$f = [IO.File]::Open($path, 'Open', 'Read', 'None')`), turn the log off, then release it (`$f.Close()`) and wait a few seconds.
  Expected: right after turning off, the held file is still there and every other log file is gone; a few seconds after the release the held file is gone too.
  Observed: Run from a Start-menu PowerShell (the Claude app terminal sees a virtualized AppData).

## add-auto-update-and-installer

Written against the head of the `add-auto-update-and-installer` PR. Unit tests cover the data folder, the version gate, the update scheduler, the tray item text and the update manager against a fake feed; these steps need an installer and a person. Build packages as `docs/release.md` describes ("Local test build"); use a standard (non-administrator) Windows account and a machine without an earlier Navpaky install. The update steps need a Debug build packed twice (for example 0.1.0 and 0.2.0) into one local feed folder and signed with the test certificate from `docs/release.md`, started with `--update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>`.

- [x] **Per-user install (2.1).** Run `navpaky-app-win-Setup.exe`.
  Expected: no UAC prompt; Navpaky starts with its tray icon; "Navpaky" appears in the Start menu and in Settings → Apps → Installed apps; files are under `%LOCALAPPDATA%\navpaky-app`, settings and word lists under `%APPDATA%\Navpaky`, other data under `%LOCALAPPDATA%\Navpaky`.
  Observed: Notes (task 2.12, 2.11): the welcome window scrolls with the wheel only over its scroll bar; the exe and the Installed apps entry have no icon; the publisher reads "navpaky-app".
- [x] **Update from a local feed (2.2).** Install 0.1.0 (Debug), quit it, start `%LOCALAPPDATA%\navpaky-app\current\Navpaky.App.exe --update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>` with 0.2.0 in the folder, open the tray menu; choose "Restart to update". The restarted Navpaky uses GitHub Releases again, so quit it, start it once more with the same `--update-feed` and `--update-pin` arguments, and choose the item again with no newer version.
  Expected: the item reads "Checking for updates…", then "Restart to update"; Navpaky restarts as 0.2.0 (About page); the next check says "Navpaky is up to date"; with "Check for updates automatically" off, no check runs at start.
  Observed: The first try said "up to date": the installer-started Navpaky was still running, so the second launch with --update-feed exited without notice (task 2.13). Not checked: no check at start with automatic checks off.
- [x] **Update keeps user data (2.3).** Before the update above, change a few settings, add a Protected Word, turn on "Start Navpaky when I sign in to Windows" and type a few Words for statistics.
  Expected: after the restart into 0.2.0 the settings, the Protected Word and the statistics are unchanged, and Navpaky still starts at the next sign-in.
  Observed:
- [x] **Tampered and unsigned packages (3.2).** In the local feed, replace the 0.2.0 `.nupkg` with a copy that has one byte changed (keep `releases.win.json`), and choose "Check for updates". Then restore a 0.2.0 package packed without signing (checksums updated by `vpk pack`) and check again.
  Expected: both times the item reads "Could not check for updates", 0.1.0 keeps running, `%LOCALAPPDATA%\navpaky-app\packages` holds no 0.2.0 package, and `%LOCALAPPDATA%\Navpaky\incidents.jsonl` gains `UpdateChecksumFailed`, then `UpdateSignatureFailed` with `SignatureVerdict.NotSigned`.
  Observed: Note (task 2.8): the tray item reads "Turn off in Windows Terminal Host" (process description) instead of the app name.
- [x] **Uninstall, keeping data (4.1).** With Navpaky installed and "Start Navpaky when I sign in to Windows" on, uninstall it from Installed apps, wait a minute before answering the question that appears, and answer No.
  Expected: Navpaky closes; `%LOCALAPPDATA%\navpaky-app`, the Start menu entry and the Run entry (Task Manager → Startup apps) are gone; `%APPDATA%\Navpaky` and `%LOCALAPPDATA%\Navpaky` are still there; installing again picks up the old settings and word lists.
  Observed: After reinstalling, "Start Navpaky when I sign in" is off although it was on: uninstall removes the Run entry. Owner decision: restore it through an install-time question, in its own change.
- [x] **Uninstall, deleting data (4.1).** Uninstall again and answer Yes.
  Expected: neither `%APPDATA%\Navpaky` nor `%LOCALAPPDATA%\Navpaky` exists and Navpaky does not start at the next sign-in.
  Observed:
- [x] **Uninstall with a locked data file (4.1).** Install again, open `%APPDATA%\Navpaky\settings.json` in a program that locks it (for example Excel, or PowerShell `[IO.File]::Open($path, 'Open', 'Read', 'None')`), uninstall and answer Yes.
  Expected: after a few seconds a warning names the folder to delete by hand; once the lock is released the folder can be deleted.
  Observed:
- [x] **Open item: reinstall while the uninstall question is open.** The data question deletes `%LOCALAPPDATA%\Navpaky` on Yes without checking whether Navpaky was installed again while it was open, so a new installation's data could be lost. Evaluate an installation marker check before deletion (CodeRabbit finding on PR #32, deferred by orchestrator decision).
  Observed: Decision (owner): check the installation marker before deleting data (task 2.12).
- [x] **Release environment and dry run (4.2).** Owner: create the `release` environment limited to `v*` tags with the `VPK_SIGN_PARAMS` secret (`docs/release.md`, One-time setup). After the change is on `main`, run Release by hand with version `0.0.1`.
  Expected: the run passes, its Pack log says no signing parameters were provided, it creates no release, and the artifact holds `navpaky-app-win-Setup.exe` and the `.nupkg`.
  Observed: Run on main: green, no release, artifact with Setup and .nupkg. Finding (task 2.12): Pack warns "No architecture specified with --runtime, defaulting to x86" while publish uses win-x64.
- [x] **Local signed test build (4.3).** Follow `docs/release.md` → Local test build on a clean checkout without help.
  Expected: two signed packages in one feed folder, which the update steps above use; note any step that needed guessing.
  Observed: Notes (task 2.14): the "keep a copy" step names no place (the C:\ root is not writable for a standard user), and the doc does not say to clear the publish folder between the two versions.


## add-onboarding

Written against the head of the `add-onboarding` PR. Unit tests cover when the welcome opens, the first-start sign-in default, the choices, the Layout notice, the one-time tip and the location classifier; the automated `onboarding` set covers the welcome opening at first start, the try-it Correction and its confirmation, and reopening from the tray. These steps need eyes, a physical keyboard or an installed build. Use a Debug build of `src/Navpaky.App` with English (US) and Russian installed unless a step says otherwise. To see the first start again, quit Navpaky and delete `%APPDATA%\Navpaky\settings.json`. The location steps need a Release build published as one file, as the Release workflow does: `dotnet publish src/Navpaky.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o <folder>` (a multi-file publish cannot run from inside a ZIP: Explorer extracts only the exe). Since add-final-acceptance the tray item was named "Getting started with Navpaky" ("Знайомство з Navpaky"); since redesign-visual-identity it is "First steps" ("Перші кроки") and the welcome has three steps.

- [x] **Look and keyboard (2.1).** At a first start, look at the welcome window in light mode, then switch Windows to dark mode (Settings → Personalization → Colors) with the window open. Then use only the keyboard: Tab through every control and press Space or Enter on each.
  Expected: keycap, description, Undo hint, privacy statement, try-it field, Auto Correction and sign-in switches, Hotkey choice, tray tip and Done read well in both modes and follow the switch at once; every control is reachable in reading order with a visible focus mark; Enter or Space on a focused Done closes the window, while Enter in the try-it field does not; Narrator reads the try-it hint with the field.
  Observed: Finding (task 2.10): themes, Tab order and Enter pass; with the Ukrainian interface Narrator reads only the English words and skips Cyrillic. Re-run on b50d5bb (task 2.10): window languages follow the interface (test); Narrator cannot speak Ukrainian on this PC, no Ukrainian voice. Environment limit.
- [x] **Try it with the physical keyboard (2.2).** At a first start, with English (US) active, type "ghbdtn" and Space in the try-it field.
  Expected: the field shows "привет ", the Layout is Russian and the green confirmation appears; no "Navpaky corrected a word" notification follows.
  Observed:
- [x] **Tray item (2.4).** Complete the welcome, then open the tray menu with a click on the Navpaky icon.
  Expected: the menu lists "First steps" (before redesign-visual-identity: "Welcome and setup") above "About Navpaky"; choosing it opens the welcome window with the current settings, and choosing it again while open brings the same window to the front.
  Observed:
- [x] **One Layout installed (2.3).** Remove every Layout but English (US) (keep a note to add Russian back), then open "First steps" ("Welcome and setup" before redesign-visual-identity) from the tray. Choose "Open language settings". Add Russian back.
  Expected: an amber notice says Navpaky needs at least two Layouts; the button opens Settings → Time & language → Language & region; once Russian is back the notice disappears without reopening the window.
  Observed:
- [x] **One-time notification (2.5).** Complete the welcome, then in Notepad type "ghbdtn" and Space; type "ghbdtn" and Space again; quit and start Navpaky and correct once more.
  Expected: only the first Correction shows the Windows notification "Navpaky corrected a word — Press Ctrl+Z to undo."; later ones, also after the restart, show none.
  Observed: Passes with Do Not Disturb off. Finding (task 2.11): with Do Not Disturb on, the tip was marked shown although Windows showed nothing, so it never came again.
- [x] **Closing without changes (spec: launch at sign-in default).** At a first start close the welcome window with its X without changing anything; open Task Manager → Startup apps.
  Expected: Navpaky is listed and enabled; the welcome does not open at the next start.
  Observed:
- [x] **Run from inside a ZIP (3.1).** Zip the Release publish folder, open the ZIP in Explorer and double-click `Navpaky.App.exe` inside it (choose Run if Explorer asks). Choose Install.
  Expected: a "Navpaky is not installed" message says this copy stops working when removed and offers Install and Not now; Install opens the latest release page in the browser (no setup in the ZIP). Repeat with `navpaky-app-win-Setup.exe` placed next to `Navpaky.App.exe` in the ZIP: Install starts that setup.
  Observed: Finding (task 2.14): the ZIP held a plain multi-file publish; Explorer extracts only the exe, which fails at once (Navpaky.App.dll missing), so no warning can show. Release builds publish a single file; the step now does the same. Re-run on 0.3.1 (2c31514; this path unchanged since) with a single-file publish: pass, the "not installed" message with Install and Not now.
- [x] **Run from Downloads and from the installed location (3.1).** Copy the publish folder to your Downloads folder and start it there; then install Navpaky with its setup and start the installed one.
  Expected: the Downloads copy shows the warning; the installed Navpaky shows none.
  Observed:
- [x] **Release build moved while running (3.2).** Start the Release build from a folder of its own, then rename `Navpaky.App.exe` in Explorer (Windows does not let a folder be renamed while an exe in it runs).
  Expected: within a minute "Navpaky's files were moved or deleted" explains that Navpaky will not start again and offers Install; Navpaky keeps correcting until you quit it.
  Observed: Finding (task 2.14): Windows does not let a folder be renamed while the exe in it runs; renaming the exe file instead showed the notice with Install and Close. Passes with the step as now written (rename the exe), on 1955903.

## add-telemetry

Written against the head of PR #39. Precondition for the network checks: set the endpoints in `src/Navpaky.App/Telemetry/TelemetryEndpoints.cs` to a test backend (none exists yet, so a shipped build sends nothing) and capture traffic with a local HTTPS proxy such as Fiddler, trusting its root certificate only for this test.

- [x] **Welcome consent (4.2).** Start with a fresh data folder; in the welcome window look at the diagnostics checkbox, then close the window without ticking it.
  Expected: the checkbox reads "Send anonymous crash reports and daily usage counts…", is unticked, and the proxy shows no request from Navpaky.
  Observed: Visual only (no test backend).
- [x] **Settings toggles (4.2).** Open Settings → Diagnostics and privacy.
  Expected: "Send crash and freeze reports" and "Send daily usage counts" switch independently, the note under them states what reports carry, and the page reads well in light and dark themes and with Narrator.
  Observed: Visual only (no test backend).
- [x] **Email sign-up (3.3).** With the proxy running, enter an invented address, tick "Tell me about new versions" and press Sign up.
  Expected: exactly one request to the sign-up endpoint with that address and purpose; the field empties and the thanks message shows; `sent.jsonl` in the data folder does not contain the address.
  Observed: Not run: no telemetry test backend. Deferred (owner decision 2026-11-18) to the first telemetry test backend: none exists yet.
- [x] **Show what was sent (4.1).** With usage counts on, wait for the daily digest (or set the delay low in a debug build), then expand "Show what was sent".
  Expected: the last entry equals, byte for byte, the request body the proxy captured.
  Observed: Not run: no telemetry test backend. Deferred (owner decision 2026-11-18) to the first telemetry test backend: none exists yet.

## add-ui-localization

Written against the head of PR #41. The interface has English and Ukrainian only (owner's decision); there is no Russian interface. Unit tests cover the default mapping, plural forms, translation completeness, that XAML shows no literal text, and that bound text follows a language change. Record the sign-off of 3.1 (reviewer, date, build) in `docs/manual-tests/localization.md` when these pass.

- [x] **Native-speaker review (3.1).** A native Ukrainian speaker reads every string of `src/Navpaky.App/Strings.uk.resx` in place: the tray tooltip and menu, each settings page, statistics, export, welcome, the correction tip, the location and moved notices, and the uninstall questions.
  Expected: wording is natural and consistent (one term per concept, such as "розкладка", "виправлення", "гаряча клавіша"); nothing reads as machine translation.
  Observed: Passes except the tray item "Вітання та налаштування", which reads like the Settings item: rename (task 2.8).
- [x] **Layouts in Ukrainian (3.1).** With the interface in Ukrainian, open every window and settings page at 100 % and 150 % scaling.
  Expected: no text is cut off, overlaps or forces a horizontal scroll bar; buttons fit their labels.
  Observed: Finding (task 2.8): the Settings section list cuts "Діагностика та конфіденційність" and shows a horizontal scroll bar; everything else fits at 100 % and 150 %. Re-run on 359c730 (task 2.8): pass, the section list fits its longest name with no scroll bar.
- [x] **Default language (1.1).** Start Navpaky with `"uiLanguage"` absent from `settings.json` on Windows with the display language Ukrainian, then Russian, then German (or English).
  Expected: Ukrainian, Ukrainian, English.
  Observed: Not run (owner decision): unit tests cover the mapping, and "Як у Windows" gave Ukrainian in Live switching.
- [x] **Live switching (2.4).** Open Settings, Statistics and the welcome window. In Settings → General pick English, then Українська, then Як у Windows.
  Expected: each pick changes the text of all three open windows, the section list, the combo boxes (including the selected entry), the tray tooltip and the tray menu at once, without a restart; the choice persists after a restart.
  Observed:
- [x] **Plural forms (1.2).** In Ukrainian, pause for 15 minutes and hover the tray icon as the time runs down.
  Expected: "залишилося 15 хвилин", … "залишилося 2 хвилини", "залишилася 1 хвилина".
  Observed:
- [x] **Uninstall questions (installer prompts).** Uninstall Navpaky on Ukrainian or Russian Windows.
  Expected: the questions about deleting data are in Ukrainian. They follow the Windows display language, not the setting picked in Navpaky (uninstall does not read settings).
  Observed: Note (task 2.12): the "removal complete" box appears together with the keep-data question; the question should come first. Re-run on 0.3.2 (b50d5bb, task 2.12): pass, Velopack's progress box stays during the open question and "removal complete" shows only after the wait; after No and a reinstall the settings were kept.
- [x] **Screen reader.** With Narrator, move through the language combo box and the settings section list in both languages.
  Expected: each entry is read by its text in the current language, not by a type name.
  Observed: Moved to the re-check after task 2.10. Re-run on b50d5bb: closed as the same environment limit as Accessibility (no Ukrainian Narrator voice).

## rename-to-navpaky

Written against the head of the `rename-to-navpaky` PR. Uninstall any test install of the old Rekey (`%LOCALAPPDATA%\key-switcher`) first and turn off its "Rekey" entry under Task Manager → Startup apps if one is left. Build packages as `docs/release.md` describes ("Local test build").

- [x] **Install under the new name (1.1, 1.2).** Run `navpaky-app-win-Setup.exe`.
  Expected: the installer and Settings → Apps → Installed apps say "Navpaky" (publisher Pavlo Yeremenko); the Start menu entry reads "Navpaky"; files are under `%LOCALAPPDATA%\navpaky-app`; settings appear under `%APPDATA%\Navpaky` and statistics under `%LOCALAPPDATA%\Navpaky` after the first change; the tray tooltip, About page and welcome window say Navpaky in English and in Ukrainian.
  Observed: pass on c435388 (0.4.0) and 7c187ac: Installed apps reads "Navpaky", publisher "Pavlo Yeremenko" (after de7c796); icon, Start menu, tray, both data folders, settings and word lists carried over.
- [x] **Start at sign-in (1.1).** Turn on "Start Navpaky when I sign in to Windows", then sign out and in.
  Expected: Task Manager → Startup apps lists Navpaky and Navpaky is running after sign-in.
  Observed: pass on c435388.
- [ ] **Single instance (1.1).** With Navpaky running, start `Navpaky.App.exe` a second time.
  Expected: the second start exits and one tray icon remains.
- [x] **Uninstall keeps data apart (1.1).** Uninstall and answer No to deleting data.
  Expected: `%LOCALAPPDATA%\navpaky-app` is gone and both `%APPDATA%\Navpaky` and `%LOCALAPPDATA%\Navpaky` are still there.
  Observed: pass on c435388: both data folders kept.
- [x] **Uninstall deleting data (5.1).** Install, start and quit Navpaky once, uninstall, and answer Yes to deleting data within a few seconds.
  Expected: no message says Navpaky was installed again; "removal complete" follows at once; `%APPDATA%\Navpaky`, `%LOCALAPPDATA%\Navpaky` and `%LOCALAPPDATA%\navpaky-app` are all gone.
  Observed: failed on c435388 (reported as a reinstall, data kept; fixed in 7c187ac, design D8); pass on 7c187ac: both data folders and the install root deleted.
- [ ] **Reinstall while the question is open (5.1).** Uninstall, leave the data question open for more than a minute, run the setup again, then answer Yes.
  Expected: a message says Navpaky was installed again and its settings and word lists were kept; both data folders are still there.

## upgrade-to-dotnet-11

- [ ] **Watchdog and focus after sleep (keyboard-capture, Input timing survives sleep).** With Navpaky running, put the PC to sleep for at least a minute and wake it. Select English (US), type "ghbdtn" + Space in a Notepad window, switch with Alt+Tab to a second Notepad window, select English (US) again and type the same keys.
  Expected: both become "привет " as before sleep; the diagnostics page shows no new hook-drop Incident.
- [ ] **Updater from .NET 10 to .NET 11 (auto-update).** Follow `docs/release.md`, Local test build: sign and pack 0.4.2 from 7c187ac and 0.4.3 from the merged `upgrade-to-dotnet-11` into the test feed, install 0.4.2 and start it with `--update-feed` and `--update-pin`.
  Expected: it finds 0.4.3, verifies the signature, applies it and restarts as 0.4.3 with settings and word lists kept.

## redesign-visual-identity

Written against the head of the `redesign-visual-identity` PR. Unit tests cover the settings shapes, the sound clips, the keycap colours, the brand fonts and accent, the indicator marks, the section scrolling, the welcome steps, the splash script and the quick panel's model. The owner approved the look in an interactive prototype; these steps check the built app against it in both themes (switch Windows between light and dark for each). Use a Debug build with English (US), Ukrainian and Russian installed.

- [ ] **Launch splash:** start Navpaky. A card in the middle of the screen types "Navpaky" in red with a dotted underline, it turns into a green "Привіт" (Ukrainian interface) or "Hello" (English) with a pill in the corner, then fades within about three seconds. Typing in Notepad during the splash goes to Notepad. With "Animation effects" off in Windows, the card shows the greeting without motion.
- [ ] **App icon:** File Explorer, the Start menu and Settings → Apps show the Q ⇄ Й mark; the About section and the welcome window show it too.
- [ ] **Tray keycap:** on a light taskbar the active key is white with a black skirt and a red lip; on a dark taskbar it is sapphire with gold; paused is amber, off is grey with a faded code, stopped after failures is crimson with a dot; at 100 %, 125 %, 150 % and 200 % scaling the code stays readable.
- [ ] **Quick panel:** a left click on the tray icon opens the panel above the tray; a right click opens the menu. The Language key closes the panel and switches the Layout of the app you typed in (the tray shows the new code). With "Let me use a different input method for each app window" on: type in app A with EN, switch to app B with UK, open the panel and press the Language key once; app B switches from UK to EN at the first press. Auto Correction, the current-app switch ("In Telegram" while typing in Telegram), Sensitivity, every Pause button (a second click resumes), the sound and indicator switches work at once. The statistics strip opens Statistics and is hidden when statistics are off. Esc and a click elsewhere close it.
- [ ] **Tray menu:** "First steps" / "Перші кроки" with a magic-wand icon opens the welcome window at step 1.
- [ ] **Welcome:** three steps with Back, Next and Done; the try-it field on step 2 corrects "ghbdtn"; closing at any step does not open it again at the next start.
- [ ] **Settings:** one scrolling page with Grunt Grotesk headings and a card per section; choosing a section scrolls to it, scrolling moves the highlight; Tab and arrow keys reach every control with a visible focus.
- [ ] **Brand colours:** switches, the selected section marker and primary buttons are red in the light theme and blue in the dark theme; text is Inter.
- [ ] **Cursor indicator:** pill (default), tag and underline each appear next to the caret after a Correction and fade; the choice in General shows a preview of each.
- [ ] **Sounds:** each of the twelve sounds previews from General; with "Navpaky" chosen, a Correction into Ukrainian plays a rising pair and one into English a falling pair.
- [ ] **Licences:** the install folder holds `Fonts/OFL-Inter.txt` and `Fonts/OFL-GruntGrotesk.txt`.

## add-install-wizard

Written against the head of the `add-install-wizard` PR. Unit tests cover the folder rules, kept-data detection, the hand-off file, the install hook, the setup runner against a stub setup, Start-fresh backups and every page's buttons; these steps need a real install. Use a throwaway Windows 11 VM (Hyper-V, checkpoint `clean-os`, and the scripts of the 2026-11-24 run) with a standard user account, and a wizard built and test-signed as `docs/release.md` "Local test build" describes. Revert to `clean-os` between scenarios unless a step builds on the one before.

**Run of 2026-11-25** (Hyper-V VM NavpakyLab, Windows 11 26300 uk-UA, standard user NavpakyTest, wizard and Debug pack of commit 2fe6f89 test-signed with a throwaway CA). The extra drives were T: (1 GB) and S: (100 MB), since D: and E: held DVDs. The wizard was driven through UI Automation in the user's session, and screenshots and logs were kept outside the repo. No UAC or credential prompt appeared in any scenario. Findings, apart from the per-item notes:
- **Fixed during the run (2fe6f89):** the hook took the install root from `AppContext.BaseDirectory`, which a single-file app that extracts native libraries places under `%TEMP%`. The sign-in entry and the remembered folder pointed into `%TEMP%`, and Velopack's desktop link stayed although the shortcut was off.
- **Fixed afterwards (task group 9, rechecked in the VM on 2026-11-25 with the build of 6768b7c):**
  - **Hook timeout on a never-seen binary (6768b7c).** Velopack gives the install hook 30 s, and Defender's scan of a freshly written `Navpaky.App.exe` made it miss that limit 2 times in 9 installs. The wizard reported "install failed" although the app was installed. The wizard now runs the hook again itself. Recheck: the hook process was suspended so that Velopack logged "timed out after 30s and was killed"; the wizard reached Done after 42 s, with the sign-in entry, the settings and no desktop link.
  - **Space needed (4a39f59).** The page said "about 82 MB", while Velopack refuses below its package's compressed plus extracted size plus 50 MB (297.92 MB). The page now reads that figure from the embedded setup. Recheck: it shows "about 298 MB" and refuses the 90 MB drive.
  - **Plain-setup desktop link (621d67d).** The pack now uses `--shortcuts StartMenuRoot`. Recheck: after the plain setup there is no desktop link, and the Welcome's switch reads Off.
  - **Welcome step 3 at 1024×768 (182be68).** Recheck: the window spans y 36–720 of a 720 px work area, and Done is visible.
  - **Unreadable-settings claim (dbf3820).** Recheck: with only an unrelated folder in `%LOCALAPPDATA%\Navpaky`, Welcome back lists nothing false. Its "Everything stays as it was:" line is then followed by an empty list, a cosmetic leftover.

- [x] **New install with defaults.** Run `NavpakySetup.exe`, keep every default, Finish.
  Expected: no UAC prompt and no Velopack window; the pages are Language, Licence, Folder, Options, Installing, Done (step counter "of 6"); Next on Licence is off until the box is checked; Navpaky starts from `%LOCALAPPDATA%\Programs\Navpaky`, appears in Installed apps and the Start menu, starts at the next sign-in, and opens the Welcome with no language, sign-in or shortcut questions.
  2026-11-25: pass after the 2fe6f89 fix. The Run entry is `<root>\Navpaky.exe`, and the Welcome's step 3 holds only the Hotkey and the telemetry switch. The sign-in start was checked through the Run entry; no actual sign-in was done.
- [x] **Language.** On a uk-UA Windows pick English on the first page.
  Expected: every wizard text switches at once; after the install Navpaky's interface is English.
  2026-11-25: pass; the folder picker's title also follows.
- [x] **Custom folder and shortcut.** Pick `D:\Tools` (or another drive), turn the shortcut on, uncheck "Start Navpaky now", Finish.
  Expected: the Folder page shows `D:\Tools\Navpaky`, the space needed and free; Navpaky is not running, yet the desktop shortcut starts it, sign-in start is on and the chosen language applies.
  2026-11-25: pass with `T:\Tools`. The first attempt hit the hook timeout, and Retry passed.
- [x] **Refused folders.** Pick a folder holding a text file, then `%LOCALAPPDATA%\Navpaky\Tools`, then a folder under `C:\Program Files`.
  Expected: Next is off with "not empty", "Navpaky's data folder" and "administrator rights" in turn; the text file is unchanged.
  2026-11-25: pass. A folder holding a file is accepted, because the wizard installs into a new `Navpaky` subfolder of it. "Not empty" appears when that `Navpaky` subfolder already holds a file, and the file is unchanged.
- [x] **Remembered folder.** After the custom-folder install, uninstall keeping data and run the wizard again.
  Expected: the Folder page proposes `D:\Tools\Navpaky`, and Change… still works.
  2026-11-25: pass (`T:\Tools\Navpaky`).
- [x] **Not enough space.** Attach a small virtual disk (for example 100 MB) and pick a folder on it.
  Expected: Next is off and the page shows the space needed.
  2026-11-25: pass on the 100 MB drive (90 MB free). The page reads "needs about 82 MB, free 90 MB" and refuses; see the space finding above.
- [x] **Reinstall over an existing install.** Install with the plain `navpaky-app-win-Setup.exe` (folder `%LOCALAPPDATA%\navpaky-app`), then run the wizard.
  Expected: the Folder page shows `navpaky-app`, Change… is off and the page says why; afterwards one Installed apps entry, one copy.
  2026-11-25: pass.
- [x] **Use previous settings.** Change the Hotkey and add an Excluded App, uninstall keeping data, run the wizard and keep "Use previous settings".
  Expected: Welcome back lists the Hotkey, words, apps and statistics; Options show the earlier sign-in and shortcut with "was on/off before"; Navpaky starts with the earlier settings and no Welcome.
  2026-11-25: pass. The Hotkey was set in the Welcome; the Excluded App, one invented learned word and a statistics file were written to the data files.
- [x] **Start fresh.** As above, choose Start fresh, Next, Cancel; then Start fresh, Next, Delete and continue.
  Expected: Cancel returns to the choice and nothing is deleted; after the install no earlier settings, words, statistics or logs remain, no `*.fresh-*` folder is left, and the Welcome opens at step 1.
  2026-11-25: pass; Cancel was checked by file hashes.
- [x] **Install failure.** Start fresh again, and make the install fail (for example lock the install folder with a file open in another process, or end `Setup.exe` in Task Manager while Installing).
  Expected: the page says the install failed, shows the log path, offers Retry and Close; the kept data is back byte for byte; Retry succeeds once the cause is gone.
  2026-11-25: pass twice. Setup was ended before the hook in one case, and Setup and the hook were ended while the `.fresh-*` backup existed in the other; both times the hashes were identical and no backup was left. The first Retry then failed on the space check, and the next one passed after space was freed.
- [x] **Plain Setup.exe fallback.** On `clean-os` run `navpaky-app-win-Setup.exe` instead of the wizard.
  Expected: the Welcome asks the language on step 1 and sign-in and the shortcut on step 3.
  2026-11-25: pass, with two open findings above: the desktop link and the height at 1024×768.
- [x] **Uninstall removes the shortcut.** With the shortcut on, uninstall from Installed apps.
  Expected: the desktop shortcut is gone.
  2026-11-25: pass. The uninstall string from the Installed apps entry was run, and "No" was answered to the data question.
- [x] **Second start.** Double-click `NavpakySetup.exe` twice quickly.
  Expected: one wizard window, brought to the front.
  2026-11-25: pass; there was one process and one window.
- [x] **Cold start time.** On a fresh VM, time from double-click to the first wizard window.
  Expected: about 8 s cold, under 1 s warm; note the numbers.
  2026-11-25: 15.3 s, 12.5 s and 13.3 s cold (the 154 MB bundle with the embedded setup); 0.8–1.0 s warm.
- [x] **Licences.** The Licence page shows the GPL followed by the Inter and Grunt Grotesk font licences.
  2026-11-25: pass. The texts are in this order: the GPL, the Inter OFL from character 36146, and Grunt Grotesk from character 40344.
- [x] **Release dry run.** Run **Release** with **Run workflow**.
  Expected: the artifact holds `NavpakySetup.exe`; the "Sign the install wizard" step is skipped.
  2026-11-24: pass (run 45479093919).

## warn-reinstall-extra-files

Use the same VM, scripts and test-signed builds as add-install-wizard.

- [x] **Reinstall with extra files.** Install Navpaky with the wizard, put a file and a folder into its install folder, and run the wizard again up to the Folder page. Close the wizard; then run it again, check the box and install.
  Expected: the Folder page lists the file and the folder, says the reinstall deletes them, and keeps Next off until "Delete them and reinstall" is checked. Closing keeps both; the confirmed reinstall removes them and installs.
  2026-11-25: pass with the wizard of c15c053 (Hyper-V NavpakyLab, standard user, uk-UA).
  - The Folder page listed `backup` and `notes.txt` with the box off and Next disabled.
  - After closing, both were still there.
  - After confirming, Next was enabled and the reinstall reached Done in 16 s, leaving only `current`, `packages`, `Navpaky.exe` and `Update.exe`.

## add-machine-wide-install

Use the same VM, scripts and test-signed builds as add-install-wizard, with the wizard and the 0.1.0/0.2.0 feed built from this change. On 2026-11-25 the VM check passed the elevated install, the pending update at start, the update while another session ran Navpaky, and the declined and approved uninstall, all driven through PowerShell Direct. The steps below need someone to type an administrator's password or sign in.

- [ ] **Over-the-shoulder install.** Sign in as the standard user, choose "For all users" and Install. Type the administrator's password at the prompt, switching it to the English layout first.
  Expected:
  - the prompt names "Navpaky Setup";
  - the wizard reaches Done;
  - Installed apps lists Navpaky once, for every user;
  - the standard user's interface language, sign-in and desktop shortcut follow the wizard;
  - the administrator's own Installed apps shows no second entry.
- [ ] **Declined prompt at Install.** Same as above, but answer No.
  Expected: the wizard says the install failed and offers Retry and Close; nothing is installed.
- [ ] **Second user's first sign-in.** After an all-users install with sign-in on, sign in as a user who never signed in since.
  Expected:
  - Navpaky starts and shows its Welcome window;
  - turning off "Start Navpaky when I sign in to Windows" asks for no administrator;
  - after the next update and sign-in, Navpaky still doesn't start for that user.
- [ ] **Update from the tray while two users are signed in.** Sign in both accounts and start Navpaky in each. In the administrator's session choose "Restart to update" and approve.
  Expected:
  - the other user's Navpaky closes;
  - the update applies and the administrator's Navpaky restarts in the new version;
  - Installed apps shows the new version once.
  Repeat, answering No: Navpaky comes back in the old version, and the update is still offered.
- [ ] **Uninstall from Installed apps.** Settings → Installed apps → Navpaky → Uninstall, then approve.
  Expected:
  - the prompt names "Navpaky";
  - Program Files\Navpaky, the entry and the shared Start-menu shortcut are gone;
  - other users' Navpaky closes;
  - a user who signs in for the first time afterwards gets no Navpaky sign-in entry.
