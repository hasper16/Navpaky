## ADDED Requirements

### Requirement: Brand palette and fonts
All Navpaky windows and panels SHALL use the brand palette of the current app theme: red and black accents in the light theme, blue and yellow (sapphire and gold) in the dark theme (owner decision). Headings SHALL use Grunt Grotesk Bold and other text SHALL use Inter (owner decision), both shipped with the app together with their SIL Open Font License texts. When a brand font cannot be loaded, the text SHALL fall back to the Windows system font.

#### Scenario: Light theme accent
- **WHEN** Windows apps use the light theme and the user opens Settings
- **THEN** the selected section marker, switches and primary buttons use the red accent

#### Scenario: Dark theme accent
- **WHEN** Windows apps use the dark theme and the user opens Settings
- **THEN** the same elements use the blue accent

#### Scenario: Licence shipped
- **WHEN** Navpaky is installed
- **THEN** the installed files include the Open Font License texts of Inter and Grunt Grotesk

### Requirement: App mark
The executable, its windows, the About section and the welcome window SHALL show the Navpaky mark: two keycaps, Latin Q and Й, which share one physical key (owner decision), joined by two arrows that point in opposite directions. Sizes of 32 px and below SHALL show the keycaps without arrows. The mark SHALL be the same in both themes.

#### Scenario: Executable icon
- **WHEN** the user looks at Navpaky in File Explorer or the Start menu
- **THEN** the icon is the Navpaky mark at the size Windows asks for

### Requirement: Quick panel
A left click on the tray icon SHALL open the quick panel next to the tray; a right click SHALL open the tray menu. The panel SHALL close on Esc, on a click outside it and when it loses focus. Changes made in it SHALL take effect immediately.

#### Scenario: Open and close
- **WHEN** the user left-clicks the tray icon and then presses Esc
- **THEN** the quick panel opens next to the tray and closes again

### Requirement: Quick panel controls
The quick panel SHALL show the current Language and state and SHALL offer: switching the Layout by clicking the Language key, Auto Correction on/off, turning Navpaky off or on in the app the user typed in before opening the panel (owner decision), Sensitivity, Pause with the tray menu's presets (the active preset resumes when clicked again), switch sound on/off, cursor indicator on/off, Settings, First steps, Quit, and a hint naming the current Hotkey.

#### Scenario: Switch Layout from the panel
- **WHEN** English (US) is active and the user clicks the Language key in the quick panel
- **THEN** the panel closes, the window the user typed in before gets focus back with the next installed Layout
- **AND** the tray icon shows the new Language

#### Scenario: Turn off in the current app
- **WHEN** the user was typing in Telegram, opens the quick panel and turns Navpaky off for Telegram
- **THEN** Telegram becomes an Excluded App
- **AND** the panel shows the switch for Telegram as off

#### Scenario: Pause from the panel
- **WHEN** the user clicks 30 min in the quick panel
- **THEN** Auto Correction pauses for 30 minutes and the panel's state line says so

### Requirement: Quick panel statistics strip
When statistics are on, the quick panel SHALL show today's Corrections, Undos and the share of Corrections kept, and clicking the strip SHALL open the Statistics window. When statistics are off, the strip SHALL be hidden.

#### Scenario: Open statistics
- **WHEN** the user clicks the statistics strip
- **THEN** the Statistics window opens

#### Scenario: Statistics off
- **WHEN** statistics are off
- **THEN** the quick panel shows no statistics strip

## MODIFIED Requirements

### Requirement: Tray icon shows state and Language
The tray icon SHALL show the current Language code of the active Layout on a three-dimensional keycap (rim, skirt, top face and front lip) and SHALL look distinct for each state: active, paused, off, and paused itself after keyboard failures. Active SHALL be red and black on a light taskbar and sapphire and gold on a dark taskbar; paused SHALL be amber, off SHALL be grey with a faded code, and paused itself SHALL be crimson with a dot (owner decision). Its tooltip SHALL state the state in words, including remaining pause time.

#### Scenario: Paused
- **WHEN** Auto Correction is paused for 30 minutes
- **THEN** the tray icon shows the paused look and the tooltip says how many minutes remain

#### Scenario: Layout changes
- **WHEN** the user switches from English (US) to Russian
- **THEN** the tray icon shows "RU"

#### Scenario: Taskbar theme
- **WHEN** the taskbar switches from light to dark while Auto Correction is active
- **THEN** the tray keycap changes from red and black to sapphire and gold

### Requirement: Tray menu
A right click on the tray icon SHALL open a menu with: Auto Correction on/off, Pause (15 minutes, 30 minutes, 1 hour, 3 hours, until restart; presets are a design decision) or Resume, an item to exclude or include the current app by name, Settings, Statistics, Check for updates, First steps, About, and Quit.

#### Scenario: Exclude current app from the tray
- **WHEN** the user is typing in Telegram and selects "Turn off in Telegram" from the tray menu
- **THEN** Telegram becomes an Excluded App
- **AND** the same item reads "Turn on in Telegram" next time

#### Scenario: Right click
- **WHEN** the user right-clicks the tray icon
- **THEN** the tray menu opens and the quick panel does not

### Requirement: Settings window
The settings window SHALL offer every user setting of Navpaky on one scrolling page, grouped into the sections General, Correction, Hotkey, Apps and words, Diagnostics and privacy, and About, each made of cards of related settings. A section list beside the page SHALL show full section names, SHALL scroll the page to a section when chosen, and SHALL highlight the section in view while the user scrolls. Changes SHALL take effect immediately without a Save button.

#### Scenario: Change Sensitivity
- **WHEN** the user selects Cautious in the Correction section
- **THEN** the next Word is evaluated with Cautious Sensitivity

#### Scenario: Reopen settings
- **WHEN** the settings window is already open and the user selects Settings again
- **THEN** the existing window comes to the front instead of a second window

#### Scenario: Jump to a section
- **WHEN** the user chooses Hotkey in the section list
- **THEN** the page scrolls so the Hotkey section is at the top

#### Scenario: Highlight while scrolling
- **WHEN** the user scrolls the page until Apps and words is at the top
- **THEN** the section list highlights Apps and words

### Requirement: Cursor indicator
When enabled (default on, design decision), after each Correction or Layout Switch made by Navpaky the app SHALL show a small mark with the new Language code next to the text caret for about one second (design decision), without taking focus. The user SHALL choose its style in Settings: pill (translucent, with a dot in the theme accent; default, owner decision), tag (solid accent with a tail toward the caret) or underline (accent letters, no background). When the caret position is unknown, the mark SHALL appear next to the mouse pointer.

#### Scenario: After a Correction
- **WHEN** a Correction switches to Russian
- **THEN** a mark reading "RU" in the chosen style appears next to the caret and disappears after about one second
- **AND** typing continues in the same window without interruption

#### Scenario: Change style
- **WHEN** the user chooses Underline in Settings and a Correction happens
- **THEN** the mark shows the Language code underlined, without a background

### Requirement: Switch sound
When enabled (default off, design decision), Navpaky SHALL play the chosen sound after each Correction and not after an Undo. The user SHALL be able to choose among twelve sounds (owner decision) and preview them: Key click, Soft tick, Chime, Bandura, Wooden spoons, Drop, Glass, Cymbals, Sopilka, Jaw harp, Frame drum and Navpaky. The default sound SHALL be Navpaky (owner decision), which rises in pitch when the new Language is not English and falls when it is English. A sound chosen before this change SHALL stay chosen.

#### Scenario: Sound on
- **WHEN** the switch sound is on and a Correction happens
- **THEN** the chosen sound plays once

#### Scenario: Navpaky sound direction
- **WHEN** the chosen sound is Navpaky and a Correction switches from English (US) to Ukrainian
- **THEN** the rising variant plays
- **AND** a later Correction back to English (US) plays the falling variant

#### Scenario: Earlier choice kept
- **WHEN** the user had chosen Chime before updating
- **THEN** Chime is still the chosen sound after the update
