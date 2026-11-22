# Tasks

## 1. Fonts (design D3)

- [x] 1.1 Test first: a `TextBlock` in a shown `Window` uses Inter after `BrandFonts.Apply()`; `BrandFonts.Check()` finds both fonts. Then add `BrandFonts` (sets Inter on each window, menu and tooltip as it appears: a default value does not flow to children), call it at start, drop the unused WPF UI font key, and add `--check-fonts` (also run in CI's published-app step). Verify: `dotnet test tests/Navpaky.App.Tests`.

## 2. Theme and accent (design D1, D2)

- [x] 2.1 Test first: in the dark theme the primary accent is the saturated brand blue; after the app sits in the dark theme, `ThemeFollower.Refresh` with a light system theme gives the light theme and the red accent. Then implement and subscribe at start. Verify: `dotnet test tests/Navpaky.App.Tests`.

## 3. Quick panel Language key (design D4)

- [x] 3.1 The Language key keeps the panel open; the worker switches the remembered window's Layout and reports it; the tray and panel show it; the tray ignores Layout changes while a Navpaky window is in front. Verify: build and `dotnet test` (Core, Win32, App); the switch in a background app is deferred to `docs/manual-tests/fix-redesign-findings.md`.

## 4. Licences (design D5)

- [x] 4.1 `OFL-*.txt` are excluded from the single-file bundle; About names both fonts and the licence; the quick panel widens to 400 for Inter's wider text. Verify: a single-file publish has `Fonts\OFL-Inter.txt` and `Fonts\OFL-GruntGrotesk.txt` next to the exe; `SettingsAutomationTests` pass.

## 5. Verification

- [ ] 5.1 A published single-file build passes `--check-fonts`; CI runs it. Verify: the CI step passes on the PR head.
- [ ] 5.2 `docs/manual-tests/fix-redesign-findings.md` lists the owner's visual re-check unticked; `docs/roadmap.md` lists the change. Verify: files present.
