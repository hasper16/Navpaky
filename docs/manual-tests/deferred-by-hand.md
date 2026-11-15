# Deferred checks by hand

By the owner's decision of 2026-11-14, the checks that need a person at the keyboard are done once, in a final acceptance pass after all roadmap changes, together with any fixes they lead to. Each change adds its section here instead of waiting for them; its own `docs/manual-tests/<change>.md` keeps the automated checks and points here. Each section names the commit its steps were written against; run them on the final build, and note where behaviour changed since.

## add-manual-correction

Written against f4ec2f6 (PR #17). The automated `manual-correction` run passed on that commit (run 7); these steps cover what injected keys cannot.

The app has no settings window before `add-tray-and-settings-ui`, so the Hotkey is changed in `%LOCALAPPDATA%\Rekey\settings.json` while Rekey is not running (Rekey writes the file on exit). Use a Debug build of `src/Rekey.App`. These checks use the physical keyboard, which the tool cannot. Each step stands alone: it names the Hotkey it needs (quit Rekey, set `"hotkey"`, start Rekey again when it differs from the current one) and starts with English (US) active in the window it types in.

- [ ] **Double tap of Shift (1.2, 2.1).** Start Rekey with `"autoCorrectionEnabled": false` and `"hotkey": "doubleShift"`. In Notepad type "ghbdtn" and tap Shift twice quickly. Switch back to English (US), press Enter for a new line, then type "Ghbdtn" holding Shift for the capital and tap Shift once.
  Expected: the first Word becomes "привет" and the Layout Russian; the second is left as typed (the Shift held for "G" does not count as a tap).
- [ ] **Win+Space with the physical keys (1.4).** Quit Rekey, set `"hotkey": "winSpace"`, start Rekey. In Notepad, in English (US), type "ghbdtn" and press Win+Space; then on a new empty line press Win+Space once more; then press Win alone.
  Expected: "ghbdtn" becomes "привет" and the Layout is Russian, with no Windows layout pop-up switching a second time and no Start menu; on the empty line the Layout becomes the one after Russian in Windows' order (English (US) when only those two are installed) and Start stays closed; Win alone opens Start as usual.
- [ ] **Selection a control does not expose (3.2).** Quit Rekey, set `"hotkey": "doubleShift"`, start Rekey. Open the Run dialog (Win+R), switch to English (US), type "ghbdtn", select it with Shift+Home and tap Shift twice.
  Expected: if the field exposes its selection, it becomes "привет"; if it does not, the text stays "ghbdtn" and only the Layout switches. Report which happened.
- [ ] **Password Field (2.3).** With `"hotkey": "doubleShift"`, on any sign-in page, switch to English (US), type "ghbdtn" in the password box and tap Shift twice.
  Expected: nothing happens: the box keeps its characters and the Layout stays English (US).

### Open item to evaluate

- [ ] **Keys typed during a multi-part replacement.** Since 1ded4ab, a replacement with spaces is typed in parts with a 50 ms pause after each space (design D4 of add-manual-correction), because Notepad garbles text after a space in one burst of injected input. The hook holds typed keys behind a Correction for at most 150 ms (design D1 of add-text-replacement), so keys typed after that can land between the parts, inside the replacement: in a run of two or three Deferred Words corrected while typing fast, or while typing during a long selection conversion right after the Hotkey. Kept as is for now (owner decision 2026-11-14). At the final pass, try both cases and decide whether to keep it or to let the worker extend the hook's hold while a multi-part replacement types, which changes the 150 ms rule and would be its own change.

## add-word-learning

Written against the head of the `add-word-learning` PR. The rules are covered by unit tests; these steps check the app wiring. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed and default settings. The app has no word list editors before `add-tray-and-settings-ui`; inspect `%LOCALAPPDATA%\Rekey\words.json` with Rekey closed.

- [ ] **Undo protects, and survives a restart (3.1, 4.1).** Quit Rekey, delete `words.json` if present, and start Rekey. In Notepad, in English (US), type "ntcn" and Space: it becomes "тест ". Press Ctrl+Z within 5 s: it is "ntcn " again. Wait 3 s, then type "ntcn" and Space once more. Quit Rekey from the tray, start it, switch to English (US), type "ntcn" and Space.
  Expected: the second and third "ntcn" stay as typed; `words.json` lists "ntcn" under `protected` with `"source": "auto"`.
- [ ] **Manual Correction learns (3.2).** In Notepad, in English (US), type "rhby;" and Space: it stays. Press the Hotkey: it becomes "кринж ". Switch to English (US) and type "rhby;" and Space.
  Expected: the second "rhby;" becomes "кринж" on its own; `words.json` lists "кринж" under `learned` with `"language": "ru"` once Rekey has quit.
- [ ] **Unreadable file is kept aside (1.2).** Quit Rekey, replace the content of `words.json` with `{ broken`, start Rekey.
  Expected: Rekey starts as usual; `words.json.bak` holds `{ broken`.

## add-tray-and-settings-ui

Written against the head of the `add-tray-and-settings-ui` PR. The automated `tray-and-settings` set covers the tray Language, a setting applying at once, the single settings window, the indicator and the sound; these steps cover what needs eyes, ears, real apps or a sign-out. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed. Pin the Rekey icon to the visible part of the notification area first (Settings → Personalization → Taskbar → Other system tray icons).

- [ ] **Keycap look (2.1).** Run `dotnet test tests/Rekey.App.Tests` and open `tests/Rekey.App.Tests/bin/Debug/net10.0-windows/keycap-sheet.png`.
  Expected: every row reads "EN" at 16, 20, 24 and 32 px; paused has an amber edge, off a faded legend, paused-itself a red edge and dot; the dark rows suit a dark taskbar.
- [ ] **Tray icon and menu (2.2).** Click the tray icon with the left and then the right button. Try each item: Auto Correction off and on, Pause → For 15 minutes then Resume, Pause → Until Rekey restarts then quit and start Rekey, "Turn off in Notepad" after typing in Notepad (and "Turn on in Notepad" after), Settings, About, Quit. Hover the icon while paused.
  Expected: the menu opens on both clicks and follows the Windows theme; each item does what it says; the icon's edge turns amber while paused and the legend fades while off; the tooltip says how many minutes remain; the pause until restart ends with the restart; Check for updates is greyed out (Statistics opens the statistics window since add-statistics-and-diagnostics; since add-auto-update-and-installer the update item works when Rekey was installed by its installer); the icon shows "RU" while typing in Russian.
- [ ] **Pause ends while the menu is open (2.2).** Pause → For 15 minutes, then 14 minutes later open the tray menu, move to Resume with the arrow keys and leave the menu open past the end of the Pause (the menu refreshes every 20 s). Close the menu and open it again.
  Expected: while open, the menu keeps its items and the highlighted Resume; after reopening it shows Pause instead of Resume.
- [ ] **Sensitivity (3.2).** In Correction, pick Cautious and type a short wrong-layout Word such as "yt" + Space in Notepad; pick Aggressive and type it again.
  Expected: the change applies to the next Word without restarting Rekey (report what each setting did).
- [ ] **Hotkey page (3.3).** Pick Win+Space, then record Win+Space with "My own combination", then record Ctrl+Alt+Space.
  Expected: Win+Space shows the warning that it takes over the Windows layout shortcut and keeps the previous Hotkey until "Use it anyway"; "Pick another" keeps the previous one; Ctrl+Alt+Space becomes the Hotkey at once and works in Notepad.
- [ ] **Apps and words (3.4).** Add Windows Terminal with "Add a running app", add a program with "Choose a program…", remove both. Add and remove a Protected Word and a Learned Word (with a language).
  Expected: added apps show their name and icon and Rekey stays off in them; removed ones correct again; added words appear at once and change the next Word as the word-learning spec says; removed ones disappear; `words.json` matches after Rekey quits.
- [ ] **Accessibility (3.5).** Fill in [settings-accessibility.md](settings-accessibility.md).
- [ ] **Indicator in real apps (4.1).** Type "ghbdtn" + Space in Notepad, then in VS Code, then in a browser text box.
  Expected: in Notepad the badge appears just below the caret; in VS Code and the browser (no system caret) it appears next to the mouse pointer; typing goes on without a pause or lost key; with "Show animations in Windows" off the badge disappears without fading.
- [ ] **Sounds (4.2).** In General, preview each of the three sounds, turn the sound on and correct a Word, then press Ctrl+Z.
  Expected: each preview plays its own sound; one sound per Correction, none for the Undo.
- [ ] **Launch at sign-in (4.3).** Turn on "Start Rekey when I sign in to Windows", sign out and sign in. Then turn Rekey off under Task Manager → Startup apps and open Settings.
  Expected: Rekey is running with its tray icon after sign-in; after the Task Manager change the switch shows off; turning it on again starts Rekey at the next sign-in.

## add-statistics-and-diagnostics

Written against the head of the `add-statistics-and-diagnostics` PR. Unit tests cover the counters, the Incident log, the word-analysis log (including its ACL) and the export text; these steps cover what needs eyes. Use a Debug build of `src/Rekey.App` with English (US) and Russian installed. Data lives in `%LOCALAPPDATA%\Rekey`.

- [ ] **Statistics window with seeded data (4.1).** Quit Rekey and save this as `stats.json` (change both dates to today and yesterday): `{"version":1,"days":[{"day":"2026-11-15","counts":{"wordsAnalysed":30,"autoCorrections":4,"undos":1,"keepDecisions":26,"correctDecisions":4},"durations":null},{"day":"2026-11-16","counts":{"wordsAnalysed":10,"manualCorrections":1},"durations":null}]}`. Start Rekey and choose Statistics in the tray menu. Then type "ghbdtn" + Space in Notepad and press Refresh.
  Expected: Today shows 10 Words analysed and 1 correction; Last 30 days shows 40 Words analysed, 5 corrections, 1 undo and 20 % undone; durations show "—" until the new correction, then a few milliseconds; choosing Statistics again brings the same window forward; the window follows the Windows theme and a screen reader reads each row's name.
- [ ] **Statistics off (1.2, 4.1).** In Settings → Diagnostics and privacy turn "Keep statistics" off, open Statistics, then quit Rekey.
  Expected: `stats.json` is gone at once; the window shows zeros; nothing is counted until the switch is on again.
- [ ] **Word-analysis log (3.2, 3.3).** Turn the word-analysis log on, type "ghbdtn" + Space and "hello" + Space in Notepad, then open `analysis\analysis-<today>.log` and the file's Properties → Security. Turn the log off.
  Expected: two lines with the Words, their Russian Renderings and Correct / Keep; Security lists only your account and SYSTEM; after turning off, the `analysis` folder holds no log file.
- [ ] **Export dialog (4.3).** Open Settings → Diagnostics and privacy → Export diagnostics…, read the preview, tick and untick "Include word-analysis log", choose Save to file, accept the suggested Desktop name, open the saved file; then choose Copy and paste into Notepad.
  Expected: the preview shows versions, Layouts, settings, statistics and Incidents with `%USERPROFILE%`, `%USERNAME%` and `%COMPUTERNAME%` in place of your folder, name and PC name, and no typed words unless the box is ticked; the saved file and the pasted text equal the preview character for character.
- [ ] **Open item: analysis-log write on the worker thread.** With the opt-in word-analysis log on, a slow disk write (> 150 ms) can let a later key overtake a held Trigger Key; evaluate whether to move the write off the worker thread (CodeRabbit follow-up on PR #24).
- [ ] **Open item: expired analysis-log days while idle.** Days older than 7 are deleted on start, when the log is turned on and on the first write of a new day; with the log on and no typing for days, an expired file stays until then. Evaluate a periodic prune (CodeRabbit finding on PR #25).
- [ ] **Open item: failed delete when the log is turned off.** If another app holds an analysis-log file open, turning the log off stops at that file and never retries, so typed Words stay on disk. Being fixed separately on `fix/analysis-log-delete-retry` (CodeRabbit finding on PR #25).

## add-auto-update-and-installer

Written against the head of the `add-auto-update-and-installer` PR. Unit tests cover the data folder, the version gate, the update scheduler, the tray item text and the update manager against a fake feed; these steps need an installer and a person. Build packages as `docs/release.md` describes ("Local test build"); use a standard (non-administrator) Windows account and a machine without an earlier Rekey install. The update steps need a Debug build packed twice (for example 0.1.0 and 0.2.0) into one local feed folder and signed with the test certificate from `docs/release.md`, started with `--update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>`.

- [ ] **Per-user install (2.1).** Run `key-switcher-win-Setup.exe`.
  Expected: no UAC prompt; Rekey starts with its tray icon; "Rekey" appears in the Start menu and in Settings → Apps → Installed apps; files are under `%LOCALAPPDATA%\key-switcher`, data under `%LOCALAPPDATA%\Rekey`.
- [ ] **Update from a local feed (2.2).** Install 0.1.0 (Debug), quit it, start `%LOCALAPPDATA%\key-switcher\current\Rekey.App.exe --update-feed <folder> --update-pin "<test subject>" <test CA thumbprint>` with 0.2.0 in the folder, open the tray menu; choose "Restart to update". The restarted Rekey uses GitHub Releases again, so quit it, start it once more with the same `--update-feed` and `--update-pin` arguments, and choose the item again with no newer version.
  Expected: the item reads "Checking for updates…", then "Restart to update"; Rekey restarts as 0.2.0 (About page); the next check says "Rekey is up to date"; with "Check for updates automatically" off, no check runs at start.
- [ ] **Update keeps user data (2.3).** Before the update above, change a few settings, add a Protected Word, turn on "Start Rekey when I sign in to Windows" and type a few Words for statistics.
  Expected: after the restart into 0.2.0 the settings, the Protected Word and the statistics are unchanged, and Rekey still starts at the next sign-in.
- [ ] **Tampered and unsigned packages (3.2).** In the local feed, replace the 0.2.0 `.nupkg` with a copy that has one byte changed (keep `releases.win.json`), and choose "Check for updates". Then restore a 0.2.0 package packed without signing (checksums updated by `vpk pack`) and check again.
  Expected: both times the item reads "Could not check for updates", 0.1.0 keeps running, `%LOCALAPPDATA%\key-switcher\packages` holds no 0.2.0 package, and `%LOCALAPPDATA%\Rekey\incidents.jsonl` gains `UpdateChecksumFailed`, then `UpdateSignatureFailed` with `SignatureVerdict.NotSigned`.
- [ ] **Uninstall, keeping data (4.1).** With Rekey installed and "Start Rekey when I sign in to Windows" on, uninstall it from Installed apps, wait a minute before answering the question that appears, and answer No.
  Expected: Rekey closes; `%LOCALAPPDATA%\key-switcher`, the Start menu entry and the Run entry (Task Manager → Startup apps) are gone; `%LOCALAPPDATA%\Rekey` is still there; installing again picks up the old settings and word lists.
- [ ] **Uninstall, deleting data (4.1).** Uninstall again and answer Yes.
  Expected: `%LOCALAPPDATA%\Rekey` no longer exists and Rekey does not start at the next sign-in.
- [ ] **Uninstall with a locked data file (4.1).** Install again, open `%LOCALAPPDATA%\Rekey\settings.json` in a program that locks it (for example Excel, or PowerShell `[IO.File]::Open($path, 'Open', 'Read', 'None')`), uninstall and answer Yes.
  Expected: after a few seconds a warning names the folder to delete by hand; once the lock is released the folder can be deleted.
- [ ] **Open item: reinstall while the uninstall question is open.** The data question deletes `%LOCALAPPDATA%\Rekey` on Yes without checking whether Rekey was installed again while it was open, so a new installation's data could be lost. Evaluate an installation marker check before deletion (CodeRabbit finding on PR #32, deferred by orchestrator decision).
- [ ] **Release environment and dry run (4.2).** Owner: create the `release` environment limited to `v*` tags with the `VPK_SIGN_PARAMS` secret (`docs/release.md`, One-time setup). After the change is on `main`, run Release by hand with version `0.0.1`.
  Expected: the run passes, its Pack log says no signing parameters were provided, it creates no release, and the artifact holds `key-switcher-win-Setup.exe` and the `.nupkg`.
- [ ] **Local signed test build (4.3).** Follow `docs/release.md` → Local test build on a clean checkout without help.
  Expected: two signed packages in one feed folder, which the update steps above use; note any step that needed guessing.

