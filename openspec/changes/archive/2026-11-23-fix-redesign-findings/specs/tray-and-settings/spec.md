## MODIFIED Requirements

### Requirement: Brand palette and fonts
All Navpaky windows, menus and panels SHALL use the brand palette of the current app theme: red and black accents in the light theme, blue and yellow (sapphire and gold) in the dark theme (owner decision), in saturated shades in both themes. Headings SHALL use Grunt Grotesk Bold and all other text SHALL use Inter (owner decision). Both fonts SHALL ship with their SIL Open Font License texts as files in a `Fonts` folder next to the app's executable (owner decision: About carries no licence line). When a brand font cannot be loaded, the text SHALL fall back to the Windows system font.

#### Scenario: Light theme accent
- **WHEN** Windows apps use the light theme and the user opens Settings
- **THEN** the selected section marker, switches and primary buttons use the red accent

#### Scenario: Dark theme accent
- **WHEN** Windows apps use the dark theme and the user opens Settings
- **THEN** the same elements use a saturated blue accent, not a pale one

#### Scenario: Body font
- **WHEN** the user opens Settings, the welcome window or the quick panel
- **THEN** their text is set in Inter and their headings in Grunt Grotesk

#### Scenario: Licence shipped
- **WHEN** Navpaky is installed
- **THEN** `OFL-Inter.txt` and `OFL-GruntGrotesk.txt` are files in the `Fonts` folder next to the installed `Navpaky.App.exe`

### Requirement: Accessible and theme-aware UI
All windows SHALL follow the Windows light/dark theme, also when the theme changes while Navpaky runs with no window open, SHALL be fully usable with the keyboard alone, and SHALL expose names to screen readers for every control.

#### Scenario: Dark mode
- **WHEN** Windows uses dark mode
- **THEN** the settings window and the tray menu use dark colors

#### Scenario: Theme changes while running
- **WHEN** Navpaky started with Windows in dark mode and the user then switches Windows to light mode
- **THEN** the next window or panel Navpaky opens uses light colors and the red accent

#### Scenario: Keyboard only
- **WHEN** the user navigates the settings window with Tab and arrow keys only
- **THEN** every control can be reached and changed, and focus is always visible

### Requirement: Quick panel controls
The quick panel SHALL show the Language of the app the user typed in before opening the panel and the state, and SHALL offer: switching that app's Layout by clicking the Language key while the panel stays open (owner decision), Auto Correction on/off, turning Navpaky off or on in that app (owner decision), Sensitivity, Pause with the tray menu's presets (the active preset resumes when clicked again), switch sound on/off, cursor indicator on/off, Settings, First steps, Quit, and a hint naming the current Hotkey. Settings, First steps and Quit SHALL each carry the same icon as in the tray menu (owner decision). While the panel is in front, the tray icon SHALL keep showing the app's Language.

#### Scenario: Switch Layout from the panel
- **WHEN** the user typed in Notepad with English (US), opens the quick panel and clicks the Language key
- **THEN** the panel stays open and its Language key and the tray icon show the next installed Language
- **AND** typing in Notepad after closing the panel uses that Layout

#### Scenario: Turn off in the current app
- **WHEN** the user was typing in Telegram, opens the quick panel and turns Navpaky off for Telegram
- **THEN** Telegram becomes an Excluded App
- **AND** the panel shows the switch for Telegram as off

#### Scenario: Pause from the panel
- **WHEN** the user clicks 30 min in the quick panel
- **THEN** Auto Correction pauses for 30 minutes and the panel's state line says so

### Requirement: Tray menu
A right click on the tray icon SHALL open a menu with: Auto Correction on/off, Pause (15 minutes, 30 minutes, 1 hour, 3 hours, until restart; presets are a design decision) or Resume, an item to exclude or include the current app by name, Settings, Statistics, Check for updates, First steps, About, and Quit. Every item SHALL carry a monochrome icon from the icon set the Settings sections use, in the text colour (owner decision).

#### Scenario: Exclude current app from the tray
- **WHEN** the user is typing in Telegram and selects "Turn off in Telegram" from the tray menu
- **THEN** Telegram becomes an Excluded App
- **AND** the same item reads "Turn on in Telegram" next time

#### Scenario: Right click
- **WHEN** the user right-clicks the tray icon
- **THEN** the tray menu opens and the quick panel does not

#### Scenario: Icons on every item
- **WHEN** the user right-clicks the tray icon
- **THEN** every item of the menu and of its Pause submenu shows an icon

