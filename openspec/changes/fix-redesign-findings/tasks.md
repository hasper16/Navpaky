# Tasks

## 1. Fonts (design D3)

- [x] 1.1 Test first: a `TextBlock` in a shown `Window` uses Inter after `BrandFonts.Apply()`; `BrandFonts.Check()` finds both fonts. Then add `BrandFonts` (sets Inter on each window, menu and tooltip as it appears: a default value does not flow to children), call it at start, drop the unused WPF UI font key, and add `--check-fonts` (also run in CI's published-app step). Verify: `dotnet test tests/Navpaky.App.Tests`.

## 2. Theme and accent (design D1, D2)

- [x] 2.1 Test first: in the dark theme the primary accent is the saturated brand blue; after the app sits in the dark theme, `ThemeFollower.Refresh` with a light system theme gives the light theme and the red accent. Then implement and subscribe at start. Verify: `dotnet test tests/Navpaky.App.Tests`.

## 3. Quick panel Language key (design D4)

- [x] 3.1 The Language key keeps the panel open; the worker switches the remembered window's Layout and reports it; the tray and panel show it; the tray ignores Layout changes while a Navpaky window is in front. Verify: build and `dotnet test` (Core, Win32, App); the switch in a background app is deferred to `docs/manual-tests/fix-redesign-findings.md`.

## 4. Licences (design D5)

- [x] 4.1 `OFL-*.txt` are excluded from the single-file bundle; the quick panel widens to 400 for Inter's wider text. Verify: a single-file publish has `Fonts\OFL-Inter.txt` and `Fonts\OFL-GruntGrotesk.txt` next to the exe; `SettingsAutomationTests` pass.

## 4b. Owner's check of 0.4.5 (design D5, D6)

- [x] 4.2 Test first: every sound name in en and uk has no explanation in brackets, and About shows no font-licence line. Then rename the Navpaky sound to "Navpaky" / "Навпаки" and drop the About line. Verify: `OwnerTextsTests` pass.

## 4c. Match the approved prototype (design D7)

- [x] 4.3 Shared row, card and pill styles and the prototype's surfaces per theme; Settings pages, the Settings window chrome and sidebar, the quick panel and the Welcome window rebuilt to the prototype; Ukrainian texts from the prototype. Verify: `dotnet test` (Core, Win32, App, DesktopChecks) passes; offscreen renders of Settings, Welcome (three steps) and the quick panel in light and dark were compared with prototype v24 during the work; the look is in `docs/manual-tests/fix-redesign-findings.md`.
- [x] 4.4 Second pass over every prototype screen in light and dark (quick panel, tray menu and keycaps, all Settings sections, Welcome steps 1-3, About, splash, indicator, Statistics): window surfaces kept after theme changes, sidebar background, pill radius, white on accent, one-line statistics, tray menu order, Welcome tray tip, export row. Test first: white on the accent in the dark theme. Verify: `dotnet test tests/Navpaky.App.Tests` passes; renders compared with prototype v24; remaining differences listed in design D7.

- [x] 4.5 Test first: the Welcome view model's switch agrees to crash reports only; step 1 has no privacy line; step 3 has the Shift/Ctrl twice chips and one switch, crash reports. Then rebuild step 1 and step 3 as the prototype (design D8) with keycap chips shared with the Hotkey page. Verify: `dotnet test tests/Navpaky.App.Tests` passes.

- [x] 4.6 Test first: every Hotkey preset shows keycaps and keeps its name in words for automation. Then draw the options with `HotkeyChip` (design D9) and update the user guide. Verify: `dotnet test tests/Navpaky.App.Tests` passes.

- [x] 4.7 Test first: Protected and Learned Words lay out as chips, and each row's Add opens its field. Then rebuild Apps and words as the prototype (design D9) with its Ukrainian button texts. Verify: `dotnet test tests/Navpaky.App.Tests` passes; a render was compared with prototype v24.

## 5. Verification

- [x] 5.1 A published single-file build passes `--check-fonts`; CI runs it. Verify: the CI step passes on the PR head.
- [x] 5.2 `docs/manual-tests/fix-redesign-findings.md` lists the owner's visual re-check unticked; `docs/roadmap.md` lists the change. Verify: files present.
