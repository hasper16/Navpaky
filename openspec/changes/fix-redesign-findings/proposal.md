# Proposal

## Why

The owner's visual check of the 0.4.4 test pack (main 2ea8778) found that the redesign does not show as approved: the app's theme lags behind the Windows theme and looks inverted, the accent is a pale blue instead of red (light) and blue (dark), text is Segoe UI instead of Inter, and the font licences are not next to the installed app. The owner also wants the quick panel to stay open when the Language key is clicked.

## What Changes

- Follow the Windows light/dark setting while Navpaky runs, also with no window open; apply the brand accent with fixed shades so the dark theme keeps a saturated blue.
- Make Inter the font of every Navpaky window, menu and tooltip (Grunt Grotesk stays for headings), and check the fonts in the published single-file build in CI (`--check-fonts`).
- **Quick panel**: the Language key switches the Layout of the window the user typed in and keeps the panel open; the panel header and the tray show the new Language. While the panel is in front, the tray keeps showing the app's Language, not the panel's.
- Ship `OFL-Inter.txt` and `OFL-GruntGrotesk.txt` in a `Fonts` folder next to `Navpaky.App.exe`; About names no fonts.
- Sound names in the picker are names only ("Navpaky" / "Навпаки").

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `tray-and-settings`: Quick panel controls (Language key keeps the panel open), Brand palette and fonts (fonts on every window, licence files), Accessible and theme-aware UI (follows a theme change while running).

## Impact

- `src/Navpaky.App`: App startup (theme following, fonts, `--check-fonts`), `Views/BrandTheme`, new `Views/BrandFonts`, `Tray/QuickPanel`, About page, `Navpaky.App.csproj` (licence files excluded from the single-file bundle).
- `src/Navpaky.Win32`: `InputWorker` gains a request that switches a given window's Layout.
- `.github/workflows/ci.yml`: the published-app step runs `--check-fonts`.
- Depends on: `redesign-visual-identity` (merged).
