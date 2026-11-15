# Desktop checks: add-onboarding

When the welcome opens, the first-start launch-at-sign-in default, the choices, the single-Layout notice, the one-time Undo tip, the location classifier, the install source and the executable watcher are covered by unit tests in `Rekey.Core.Tests`, `Rekey.Win32.Tests` and `Rekey.App.Tests`. What needs the real app on a Windows desktop is checked by a tool; what needs a person is in the `add-onboarding` section of [deferred-by-hand.md](deferred-by-hand.md). Tick a task in `tasks.md` once all of its checks pass on the commit under review.

## Automated: `tools/Rekey.DesktopChecks onboarding`

Needs the English (US) and Russian Layouts installed and a Debug build. Close every running Rekey and input probe, then run from the repository root:

```bash
dotnet build && dotnet run --project tools/Rekey.DesktopChecks -- onboarding
```

Do not touch the mouse or keyboard for about 2 minutes. The tool keeps `settings.json`, `words.json` and Rekey's start-at-sign-in entries aside and puts them back at the end, then starts the Debug app (`src/Rekey.App/bin/Debug/net10.0-windows/Rekey.App.exe --check-ui`) with no settings, as at a first start. In this mode the app writes what its tray, indicator, sound and Undo tip did to `%LOCALAPPDATA%\Rekey\check-ui.txt` and takes the commands `open-settings`, `open-welcome` and `quit` from `check-ui-command.txt` in place of clicks on its tray menu. The tool's own worker only watches Layout changes; the app corrects. The tool reads and presses the welcome window's controls through UI Automation, types only the spec's Word "ghbdtn", and prints one ✅ or ❌ line per check, a summary, and exit code 1 when any check fails. Output carries booleans, counts and codes only, never text.

| Task | What the tool checks |
|---|---|
| 1.1 | With no settings the welcome window opens at start |
| 2.2 | After switching the try-it field to English (US), "ghbdtn" + Space becomes "привет ", the confirmation appears, the app counts one Correction and shows no Undo tip |
| 1.1 | Done closes the window, `settings.json` gets `"welcomeCompleted": true`, and start at sign-in is on (no switch was touched) |
| 2.4 | The tray command opens the welcome window again, without the earlier confirmation |
| 2.5 | The first Correction in the tool's test window shows the Undo tip; the next Correction does not |
| 1.1 | After Quit and a new start, no welcome window opens |
| 3.2 | A copy of the Debug build started from `%TEMP%`, whose executable is then renamed, shows the "Rekey's files were moved or deleted" notice within 10 s |

Since this change, the `tray-and-settings` set and the watchdog checks of the default set start the app with the welcome marked completed, so the welcome window does not take the front from their test windows.

Last run: not yet run.

## By hand

The temporary-location warning skips Debug builds (design D4), so it is checked by hand with a Release build: see the `add-onboarding` section of [deferred-by-hand.md](deferred-by-hand.md).
