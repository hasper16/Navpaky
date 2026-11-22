# Tasks

## 1. Data shapes (design D7, D8)

- [x] 1.1 Test first: `SettingsTests` reads a settings file without `cursorIndicatorStyle` as `Pill`, round-trips `tag` and `underline`, and reads a file without `switchSound` as `Navpaky` while `chime` stays `Chime`. Then add `CursorIndicatorStyle` and the nine new `SwitchSound` members after the existing ones, default `Navpaky`. Verify: `dotnet test tests/Navpaky.Core.Tests`.

## 2. Assets (design D2, D4, D8)

- [x] 2.1 Put the app mark masters (`navpaky.svg`, `navpaky-16/24/32.svg`: Q ⇄ Й drawn as shapes, arrows opposite) in `docs/design/icons/app/` with `docs/design/icons/tools/build_icons.py`, which writes `src/Navpaky.App/Navpaky.ico` (16–256 px) and `Assets/mark-64.png`, `mark-128.png`; About and Welcome show the mark. Verify: running the script twice gives the same `.ico` (same MD5); `BrandMarkTests` loads the 128 px mark.
- [x] 2.2 `tools/build_sounds.py` generates the nine new WAVs plus `navpaky-up.wav`, `navpaky-down.wav` (44.1 kHz, 16-bit mono, fixed seed). Verify: running it twice gives identical files (MD5); `WaveFileTests` and `SwitchSoundPlayerTests` open every `SwitchSound` clip and check the format.
- [x] 2.3 Add Inter Regular and SemiBold and Grunt Grotesk Bold as resources under `src/Navpaky.App/Fonts/`; Inter replaces WPF UI's `ContentControlThemeFontFamily`, `HeadingText` uses Grunt Grotesk; both OFL texts are copied to the output. Verify: `BrandFontsTests` loads Inter and Grunt Grotesk from the resources; `Fonts/OFL-*.txt` exist in the build output.

## 3. Theme and keycap (design D1, D3)

- [x] 3.1 Test first: `KeycapTests` renders each state on light and dark taskbars at 16, 20, 24, 32 px and checks the lip and face colours at fixed pixels (red lip light, gold lip dark). Then draw the 3D keycap in `Keycap`. Verify: `dotnet test tests/Navpaky.App.Tests`; by-hand look deferred to `deferred-by-hand.md`.
- [x] 3.2 `BrandTheme` applies the accent (red light, blue dark) and `BrandSecondaryBrush` (black / gold) on start and after every theme change; `HeadingText` style (2.3). Verify: `BrandThemeTests` applies each theme and reads both resources; by-hand look deferred.

## 4. Sounds (design D8)

- [x] 4.1 Test first: `SwitchSoundPlayer` picks `navpaky-up` for a non-English Language and `navpaky-down` for English. Then add `Clip(sound, towardEnglish)` and `Play(sound, towardEnglish)` and update callers; General lists the twelve sounds with preview. Verify: `dotnet test tests/Navpaky.App.Tests`.

## 5. Cursor indicator (design D7)

- [ ] 5.1 Test first: the indicator builds a pill, tag or underline visual for the chosen style. Then replace the bitmap badge; add the style choice with previews to General. Verify: `dotnet test tests/Navpaky.App.Tests`; the desktop set for the indicator still passes; look deferred.

## 6. Settings page (design D6)

- [ ] 6.1 Test first: `SectionInView` returns the section at the top, the last section at the end, and the first section at offset 0. Then stack the pages in one scroll view with section headings and cards, wire the list both ways. Verify: `dotnet test tests/Navpaky.App.Tests`; keyboard-only pass deferred.

## 7. Quick panel (design D5)

- [ ] 7.1 Test first: `QuickPanelViewModel` exposes state text, toggles Auto Correction, applies and resumes a Pause preset, computes the kept share (hidden at zero Corrections) and hides the strip when statistics are off. Verify: `dotnet test tests/Navpaky.App.Tests`.
- [ ] 7.2 `QuickPanel` window: left click opens, right click keeps the menu, closes on Esc and deactivation, Language key hands focus back and switches the Layout. Verify: a desktop check opens the panel by left click and closes it with Esc; Layout switch from the panel deferred to `deferred-by-hand.md`.

## 8. Welcome and tray menu (design D9, D11)

- [ ] 8.1 Test first: `WelcomeViewModel` steps 0→1→2, `Back` returns, `IsLast` on step 2, closing at any step completes. Then show one step at a time; rename the menu item to First steps / Перші кроки. Verify: `dotnet test tests/Navpaky.App.Tests`.

## 9. Launch splash (design D10)

- [ ] 9.1 Test first: `SplashScript` lists the typing frames for "Navpaky", then the greeting for the UI language, the pill and the fade, about 3 s in total; with animations off it is one static frame. Then `LaunchSplash` window shown from `OnStartup` after the tray icon. Verify: `dotnet test tests/Navpaky.App.Tests`; a desktop check confirms the foreground window keeps focus while the splash shows.

## 10. Docs and verification

- [ ] 10.1 `docs/roadmap.md` lists the change; `docs/manual-tests/deferred-by-hand.md` gains the `redesign-visual-identity` checklist (look of tray, panel, settings, welcome, splash, indicator styles, sounds in both themes); `docs/manual-tests/redesign-visual-identity.md` names the desktop sets. Verify: files present.
- [ ] 10.2 All tests green on CI. Verify: CI `build` job green on the PR head.
