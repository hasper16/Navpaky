# Proposal

## Why

During acceptance the owner disliked the icons and the look of the settings window. The new
name Navpaky ("vice versa") needs its own visual identity with an unobtrusive Ukrainian accent.
The owner approved an interactive prototype on 2026-11-19 (DESIGN.md, EXPERIENCE.md in
`docs/design/`).

## What Changes

- New app mark: two 3D keycaps і and S (the same physical key) joined by two arrows that run in opposite directions;
  one multi-size `.ico` for the executable, windows, About and Welcome.
- Tray icon drawn as the 3D keycap from the icon set; active is red-black on a light taskbar and
  sapphire-gold on a dark one.
- Palette follows the app theme: red and black in light, blue and yellow in dark. Headings use
  Grunt Grotesk (OFL, shipped with its licence); body text stays Segoe UI Variable.
- **Quick panel**: left click on the tray icon opens a panel with the current Layout (click
  switches it), Auto Correction, Sensitivity, Pause, today's statistics strip, switch sound and
  cursor indicator toggles, Settings, First steps and Quit. Right click opens the menu.
- Settings becomes one scrolling page; the section list scrolls to a section and highlights the
  section in view.
- Cursor indicator gets three styles (pill default, tag, underline), chosen in Settings.
- Switch sounds grow from 3 to 12; the new default sound "Navpaky" rises toward a non-English
  Language and falls toward English.
- Launch splash on every start: a caret types "Navpaky", which turns into "Привіт" or "Hello"
  with a Language pill, then fades.
- Welcome becomes a three-step wizard; its tray item is renamed "First steps" ("Перші кроки").

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `tray-and-settings`: tray icon look, tray click behaviour and quick panel, single-page
  settings, cursor indicator styles, switch sound list and default, brand palette and fonts.
- `onboarding`: welcome window as three steps; reopen item name.
- `app-lifecycle`: launch splash; the background start allows it.

## Impact

- `src/Navpaky.App`: Tray (Keycap, TrayIconView, new QuickPanel), new LaunchSplash, Views (SettingsWindow and
  pages, WelcomeWindow), Feedback (CursorIndicator, SwitchSoundPlayer), Resources, fonts,
  icons, sounds, Strings (en, uk).
- `src/Navpaky.Core`: `SwitchSound` gains values; `Settings` gains `CursorIndicatorStyle`;
  telemetry setting states unchanged in shape.
- New assets: `docs/design/icons/` (sources and build script), `tools/build_sounds.py`,
  `src/Navpaky.App/Fonts/` with the OFL text.
- Depends on: all product changes 1–15 and P1 (current main).
