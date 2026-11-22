# Design

## Context

Findings of the owner's check of the 0.4.4 pack, traced in WPF UI 4.3.0's source:
- WPF UI follows a Windows theme change only through `SystemThemeWatcher` hooks on watched, open windows; a tray app usually has none, so the app keeps its start-time theme.
- `ApplicationAccentColorManager.Apply(color, Dark)` lightens and desaturates the accent for the dark theme, so the brand blue turns pale.
- WPF UI defines `ContentControlThemeFontFamily` but no control or window reads it; windows take the system message font (Segoe UI).
- The single-file publish bundles content files, so the OFL texts never reach the install folder.

## Decisions

### D1. Follow the theme app-wide
`Views/ThemeFollower` subscribes to `SystemEvents.UserPreferenceChanged` (raised for General/Color/VisualStyle changes, including the apps light/dark switch) and, on the UI dispatcher, applies the theme for Windows' apps-mode setting (`AppsUseLightTheme`, or high contrast) with the brand accent and redraws the tray. WPF UI reads the desktop theme file first, which can disagree with apps mode, so the follower also corrects a theme WPF UI applies itself when a watched window opens. `ThemeFor(highContrast, appsUseLightTheme)` is the tested selector. UI thread.

### D2. Explicit accent shades
`BrandTheme.Apply` uses `ApplicationAccentColorManager.Apply(system, primary, secondary, tertiary)` with fixed shades: light red #C8102E / #B00E28 / #8F0B20 / #6E0818; dark blue #4C86F0 / #6B9CF7 / #8AB2FA / #A9C7FC. Text-on-accent stays WPF UI's choice.

### D3. Fonts on every window
`Views/BrandFonts.Apply()` overrides the default of `TextElement.FontFamilyProperty` for `Window`, `ContextMenu` and `ToolTip` with the Inter pack URI (Segoe UI Variable fallback in the same `FontFamily`), once at start. Controls inherit it; `HeadingText` keeps Grunt Grotesk. `BrandFonts.Check()` loads a glyph typeface of each brand font; `Navpaky.App.exe --check-fonts` runs only that check and exits 0 or 1, before Velopack and the single-instance guard. CI's published-app step runs it on the single-file build.

### D4. Language key keeps the panel open
`InputWorker.SwitchWindowToNextLayout(window)` posts a message; on the worker thread it reads the Layout of the window's thread (`GetKeyboardLayout`), picks `LayoutCycle.Next`, posts `WM_INPUTLANGCHANGEREQUEST` to that window (as `ForegroundLayout` does for the focus target) and raises `WindowLayoutRequested(LayoutId)`. The app shows that Language in the tray and the open panel. While a Navpaky window is in front, `LayoutChanged` from the foreground poll no longer updates the tray, so it keeps the app's Language. Risk: an app that ignores a posted request while in the background keeps its Layout; the next foreground poll shows the real one when the user returns. By-hand check.

### D5. Licence files and About
The two `OFL-*.txt` items get `ExcludeFromSingleFile="true"`, so the publish folder and the install carry them next to the exe. About gains one line naming Inter and Grunt Grotesk under the SIL Open Font License 1.1, with the file names.

## Risks / Trade-offs

- `SystemEvents` keeps a hidden window; it is released on exit.
- Posting a Layout request to a background window is honoured by ordinary Win32 and WPF apps; consoles and some UWP apps may ignore it (D4).
