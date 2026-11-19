# Design

## Context

The look is set by `docs/design/DESIGN.md` and the behaviour by `docs/design/EXPERIENCE.md`,
both from the prototype the owner approved on 2026-11-19. The current UI is WPF with WPF UI
(Fluent windows, Mica), H.NotifyIcon for the tray, a bitmap keycap drawn at run time
(`Tray/Keycap`), a settings window that swaps one page at a time, a single-page welcome window,
three embedded WAV sounds and a bitmap cursor badge. Icons made by another tool (Antigravity)
exist outside the repo with 3D keycap tokens and SVG masters.

## Goals / Non-Goals

**Goals:** the approved look in both themes; quick panel; one-page settings; cursor indicator
styles; twelve sounds; three-step welcome; everything keyboard- and screen-reader-usable.

**Non-Goals:** installer (Setup.exe) icon, Microsoft Store tile assets, per-layout chips,
excluded-apps search, import/export of learned words, richer statistics screens (later
changes). No change to correction logic.

## Decisions

### D1. Tray keycap drawn as vectors at run time
`Keycap.Render` keeps its signature and draws the icon set's 3D keycap (rim, skirt, bottom facet,
chamfers, top face, catch-light, front lip) with `DrawingContext` on a 32-unit grid scaled to the
requested size, then the legend in Segoe UI Variable Display SemiBold. Colours are one table per
state and taskbar theme, taken from the icon set; active-light is recoloured red-black (white
face, black skirt, red lip). No PNG templates ship: one source for every DPI. UI thread.
*Alternative:* ship the 32 PNG templates; rejected, as it doubles the source of truth.

### D2. App mark from the icon set, letters that share a key
`docs/design/icons/app/` holds the mark masters derived from the icon set: Q on the graphite
keycap and Й on the sapphire-gold keycap (one physical key), drawn as shapes, with the bottom
arrow the top arrow rotated 180°. `docs/design/icons/tools/build_icons.py` (resvg, Pillow)
renders `src/Navpaky.App/Navpaky.ico` (16–256 px, PNG frames, each from the closest master) and
`Assets/mark-64.png`, `mark-128.png`, embedded like the sounds. Generated files are committed;
the script is run by hand (`uv run`), not in CI. The rest of the icon set (setup, store, tray
PNG templates) is not imported.

### D3. Theme palette through WPF UI accents
On start and on every theme change (`SystemThemeWatcher` already runs per window, App applies
the system theme), App calls `ApplicationAccentColorManager.Apply(accent, theme)` with red
`#C8102E` for light and blue `#4C86F0` for dark. A second brush `BrandSecondaryBrush`
(black / gold) is set in application resources for lips and the indicator dot. UI thread.

### D4. Brand fonts as WPF resources
`Inter-Regular.ttf`, `Inter-SemiBold.ttf` (Inter 4.1, rsms/inter) and `GruntGrotesk-Bold.otf` go to
`src/Navpaky.App/Fonts/` as `Resource`. Inter replaces WPF UI's `ContentControlThemeFontFamily`
key in `Resources.xaml`, so every WPF UI control and window takes it from one place; the
`HeadingText` style uses `HeadingFontFamily` (Grunt Grotesk). Each `FontFamily` lists Segoe UI
Variable as the fallback. `Fonts/OFL-Inter.txt` and `Fonts/OFL-GruntGrotesk.txt` are copied to
the output folder. Grunt Grotesk is not used below 14 px.

### D5. Quick panel window
`Tray/QuickPanel` is a borderless, topmost, `ShowInTaskbar=false` WPF UI window, placed above
the tray in the work area of the screen holding the mouse pointer. `TrayIconView` sets
`MenuActivation = RightClick` and opens the panel on `TrayLeftMouseUp`. Before showing it, the
panel records the foreground window (`GetForegroundWindow`). It closes on `Deactivated` and Esc.
Clicking the Language key closes the panel, gives focus back to the recorded window
(`SetForegroundWindow`, allowed because our process is in the foreground) and asks the input
worker to switch to the next installed Layout (`InputWorker.SwitchLayout`, posted to the worker
thread). A `QuickPanelViewModel` binds to the existing `TrayViewModel`, `SettingsHub` and
statistics store; the current-app switch reuses `TrayViewModel`'s exclude item for the recorded
foreground app; the share kept is `(Corrections − Undos) / Corrections`, hidden at zero
Corrections. All on the UI thread except the switch, which the worker runs.

### D6. One-page settings
`SettingsWindow` puts the six existing page controls in one `StackPanel` inside a
`ScrollViewer`, each preceded by a heading. Choosing a section scrolls to its heading.
`ScrollChanged` picks the highlighted section with a pure function `SectionInView(tops,
offset, viewport, extent)`: the last section whose top is at or above the offset plus a small
margin, or the last section when scrolled to the end. A flag stops selection and scrolling from
re-triggering each other. Rows are regrouped into cards (`Border` with `CardBackgroundFillColor`).

### D7. Cursor indicator styles
`Settings` gains `CursorIndicatorStyle` (`pill`, `tag`, `underline`; default `pill`), serialised
as a string like the other enums; old settings files without it read the default.
`CursorIndicator` replaces its bitmap with a small WPF visual built per style; the window,
placement, timing and fade stay as they are. The style is chosen in General with a preview per
choice.

### D8. Twelve sounds generated by a script
`tools/build_sounds.py` (standard library only, fixed random seed) synthesises the nine new
sounds as 44.1 kHz 16-bit mono WAV files, the same format `SwitchSoundPlayer` already requires:
bandura and cymbals by plucked-string synthesis, spoons and frame drum by filtered noise bursts,
drop, glass, sopilka by sine tones with envelopes, jaw harp by a swept band-pass on a sawtooth,
and Navpaky as two files, `navpaky-up.wav` and `navpaky-down.wav`. The three existing WAVs stay.
`SwitchSound` gains members after the existing ones, so stored values keep their meaning; the
default becomes `Navpaky`. `SwitchSoundPlayer.Play(sound, towardEnglish)` picks the variant.
Sounds are ours (ADR 0003).

### D9. Three-step welcome
`WelcomeViewModel` gains `Step` (0–2) with `Next`, `Back` and `IsLast`; the window shows one
step panel at a time and keeps the existing try-it field, Layout check and choices. Closing at
any step marks the welcome completed, as today.

### D10. Launch splash
`Feedback/LaunchSplash` is a transparent, topmost, click-through window (`ScreenFacts.MakeOverlay`,
as the cursor indicator) with `ShowActivated=false`, centred on the primary screen's work area. It
is shown from `OnStartup` after the tray icon exists, so it delays nothing. A
`DispatcherTimer`-driven script types "Navpaky" (95 ms per letter, red with a wavy underline),
flips it to the greeting from `Strings` (green), shows the Language pill (same visual as the
cursor indicator pill), settles and fades; total about 3 s (design decision). The step timing
is a pure `SplashScript` (list of timed frames) tested in `Navpaky.App.Tests`. When
`SystemParameters.ClientAreaAnimation` is off, the script is one static frame for 1.4 s. The
window closes itself and is never reused. UI thread.

### D11. Strings
New keys in `Strings.resx` and `Strings.uk.resx`; `MenuWelcome` becomes "First steps" /
"Перші кроки" without a product placeholder. Texts are ours.

## Risks / Trade-offs

- Taking focus for the quick panel moves the foreground; D5 hands it back before switching.
- Grunt Grotesk ships only Light and Bold free; headings use only those.
- 3D detail at 16 px is subtle; the legend stays in the system face to keep it crisp.
- Look and feel need a person: those checks go to `docs/manual-tests/deferred-by-hand.md`.
