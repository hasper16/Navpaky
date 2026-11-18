# Desktop checks: add-tray-and-settings-ui

The settings hub (snapshot per change, one debounced save), the tray state and its text, the Pause presets, the exclude item, the Hotkey choice and conflict hold, the Excluded Apps and word list editors, the keycap drawing, the indicator placement, the sound rule (after a Correction, never after an Undo), the bundled sounds and the launch-at-sign-in registry rules are covered by unit tests in `Navpaky.Core.Tests`, `Navpaky.Win32.Tests` and `Navpaky.App.Tests`. What needs the real app on a Windows desktop is checked by a tool, plus a checklist by hand. Tick a task in `tasks.md` once all of its checks pass on the commit under review.

## Automated: `tools/Navpaky.DesktopChecks tray-and-settings`

Needs the English (US) and Russian Layouts installed and a Debug build. Close every running Navpaky and input probe, then run from the repository root:

```bash
dotnet build && dotnet run --project tools/Navpaky.DesktopChecks -- tray-and-settings
```

Do not touch the mouse or keyboard for about 2 minutes. The tool starts the Debug app (`src/Navpaky.App/bin/Debug/net10.0-windows/Navpaky.App.exe --check-ui`) with default settings, the welcome marked completed so its window stays closed (since `add-onboarding`), and no word lists (it keeps `settings.json`, `words.json` and the start-at-sign-in entries aside and puts them back at the end). In this mode the app writes what its tray, indicator and sound did to `%LOCALAPPDATA%\Navpaky\check-ui.txt` and takes the commands `open-settings` and `quit` from `check-ui-command.txt` in place of clicks on its tray menu. The tool's own worker only watches Layout changes; the app corrects. The tool types the spec's Word "ghbdtn" into its test window, changes switches in the settings window through UI Automation (no mouse), and prints one ✅ or ❌ line per check, a summary, and exit code 1 when any check fails. Output carries booleans, counts and codes only, never text.

| Task | What the tool checks |
|---|---|
| 2.2 | After switching the test window to Russian the tray shows "RU", and "EN" after switching back; `quit` ends the app |
| 4.1 | After "ghbdtn" + Space is corrected, the indicator window is visible, the test window stays in front and the next key lands in the box; about a second later the indicator is gone |
| 3.2 | Turning off "Correct wrong-layout words automatically" in the Correction section leaves the next "ghbdtn " as typed; turning it on corrects the next one |
| 3.1 | With the settings window open and the test window in front, Settings again brings the same window to the front and creates no second one |
| 4.2 | With "Play a sound after a correction" on, a Correction plays one sound and Ctrl+Z right after it (an Undo) plays none |

Last run: 10 of 10 at e1e3631 on the owner's desktop (prerequisites; 2.2 tray RU and EN; 4.1 badge appears and hides; 3.2 Auto Correction off and on from the settings window; 3.1 Settings reuses the open window; 4.2 one sound per Correction and none on Undo; 2.2 Quit ends the app); the language list and `settings.json` were unchanged afterwards.

## By hand

Deferred by the owner's decision of 2026-11-14 to the final acceptance pass after all roadmap changes: see the `add-tray-and-settings-ui` section of [deferred-by-hand.md](deferred-by-hand.md) and [settings-accessibility.md](settings-accessibility.md).
