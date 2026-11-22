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

- [x] 5.1 Test first: `IndicatorMark` builds a pill (accent dot), tag (accent fill) or underline (no background) for the chosen style; `SettingsViewModel` stores the style. Then the indicator shows the mark instead of the bitmap badge and General offers the three styles with previews. Verify: `dotnet test tests/Navpaky.App.Tests` (`IndicatorMarkTests`, `SettingsViewModelTests`, `SettingsAutomationTests`); the indicator desktop set runs on the orchestrator's gate; look deferred.

## 6. Settings page (design D6)

- [x] 6.1 Test first: `SectionScroll.InView` returns the section whose heading reached the top, the last section at the end, and the first at offset 0. Then the six pages sit on one scrolling page, each under a Grunt Grotesk heading in a card; choosing a section scrolls to it and scrolling moves the highlight. Verify: `dotnet test tests/Navpaky.App.Tests` (`SectionScrollTests`, `SettingsAutomationTests`); keyboard-only pass deferred.

## 7. Quick panel (design D5)

- [x] 7.1 Test first: `QuickPanelViewModel` turns Auto Correction off, pauses with a preset and resumes on the same preset, turns the current app off, shows today's Corrections, Undos and the share kept (none at zero Corrections), hides the strip when statistics are off, and picks the next installed Layout. Verify: `dotnet test tests/Navpaky.App.Tests` (`QuickPanelViewModelTests`).
- [x] 7.2 `QuickPanel` window: a left click on the tray icon opens it above the tray, a right click keeps the menu; it closes on Esc, on losing focus and after an action; the Language key hands the focus back to the last app window `ForegroundApps` reported (never the taskbar; `ScreenFacts.Activate`) and switches to the next installed Layout. Verify: the build and `dotnet test tests/Navpaky.App.Tests` pass; an offscreen render of the panel was checked during the work; opening it from the tray, the Layout switch and the look are deferred to `deferred-by-hand.md` (a desktop check cannot click the notification area reliably).

## 8. Welcome and tray menu (design D9, D11)

- [x] 8.1 Test first: `WelcomeViewModel` steps 0→1→2, `Back` returns and stops at 0, `IsLastStep` on step 2; closing at any step completes (unchanged). Then the window shows one step at a time with Back, Next, Done and "Step N of 3"; the tray item becomes First steps / Перші кроки with a wand icon; the onboarding desktop set presses Next to reach the try-it field and Done. Verify: `dotnet test tests/Navpaky.App.Tests`; the onboarding set runs on the orchestrator's gate.

## 9. Launch splash (design D10)

- [x] 9.1 Test first: `SplashScript` types "Navpaky" letter by letter as wrong, then the greeting in the UI language with the pill, settles and is gone after about 3 s; with animations off it is the greeting, then gone. Then `LaunchSplash` (click-through, no activation, not the main window) plays it from `OnStartup` after input starts. Verify: `dotnet test tests/Navpaky.App.Tests` (`SplashScriptTests`); the look and "typing keeps focus during the splash" are deferred to `deferred-by-hand.md`.

## 10. Docs and verification

- [x] 10.1 `docs/roadmap.md` lists the change; `docs/manual-tests/deferred-by-hand.md` gains the `redesign-visual-identity` checklist (look of tray, panel, settings, welcome, splash, indicator styles, sounds in both themes); `docs/manual-tests/redesign-visual-identity.md` names the desktop sets. Verify: files present.
- [x] 10.2 All tests green on CI. Verify: CI `build` job green on the PR head (both `build` jobs and CodeQL pass at f1bd235); the desktop sets in `docs/manual-tests/redesign-visual-identity.md` passed at f1bd235.
