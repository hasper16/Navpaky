# app-lifecycle Specification

## Purpose
Defines how Navpaky runs as a per-user background process on Windows: start-up without a main window, single-instance guarantee, per-user settings storage, and clean shutdown.

## Requirements

### Requirement: Background start without a main window
The app SHALL start without opening a main window or a taskbar button and SHALL show a notification-area (tray) icon while running. The launch splash is not a main window; the welcome window opens automatically on the first start only, and the user can reopen it from the tray menu or the quick panel (onboarding).

#### Scenario: Normal start
- **WHEN** the user launches Navpaky after the first start
- **THEN** a Navpaky icon appears in the notification area
- **AND** no window other than the launch splash and no taskbar button are shown

### Requirement: Quit from the tray
The tray icon menu SHALL contain a Quit item that ends the process.

#### Scenario: User quits
- **WHEN** the user selects Quit in the tray menu
- **THEN** the tray icon disappears and the Navpaky process exits with code 0

### Requirement: Single instance per user session
Only one Navpaky process SHALL run per Windows user session. A second launch MUST exit without side effects and leave the running instance untouched. When the second launch carries command-line arguments, which the running instance does not receive, it SHALL tell the user that Navpaky is already running before it exits (design decision of add-final-acceptance).

#### Scenario: Second launch
- **WHEN** Navpaky is already running and the user launches it again
- **THEN** the second process exits within 2 seconds (design decision)
- **AND** only one tray icon remains

#### Scenario: Second launch with arguments
- **WHEN** Navpaky is already running and the user launches it again with arguments
- **THEN** a message says Navpaky is already running and must be quit first for the arguments to apply
- **AND** the second process exits once the message is closed and only one tray icon remains

#### Scenario: Another user session
- **WHEN** Navpaky runs in user session A and is launched in user session B
- **THEN** both instances run independently

### Requirement: Per-user settings storage
The app SHALL persist settings per Windows user in a file under `%APPDATA%\Navpaky`, write it atomically, and keep settings across restarts. Settings and word lists SHALL live in `%APPDATA%\Navpaky` (the roaming profile, so they may follow the user to another PC); statistics, the word-analysis log, Incidents, restart state, telemetry records and check files SHALL live in `%LOCALAPPDATA%\Navpaky` (the local profile, so logs and statistics that can hold typed words never roam). Both folders SHALL lie outside the install root (design decision of rename-to-navpaky).

#### Scenario: Settings and local state in separate folders
- **WHEN** Navpaky has saved its settings, word lists and statistics
- **THEN** `settings.json` and `words.json` are in `%APPDATA%\Navpaky` and `stats.json` is in `%LOCALAPPDATA%\Navpaky`

#### Scenario: Settings survive restart
- **WHEN** a setting is changed and Navpaky is restarted
- **THEN** the changed value is in effect after restart

#### Scenario: Missing settings file
- **WHEN** Navpaky starts and no settings file exists
- **THEN** Navpaky starts with default settings and creates the file

### Requirement: Recovery from a corrupt settings file
If the settings file cannot be parsed, the app MUST start with default settings and preserve the unreadable file as a backup instead of deleting it.

#### Scenario: Corrupt settings file
- **WHEN** Navpaky starts and the settings file cannot be parsed
- **THEN** Navpaky starts with default settings
- **AND** the original file is kept next to it with a `.bak` suffix

### Requirement: Clean shutdown
On Quit, Windows logoff, or shutdown, the app SHALL release every system hook and resource it installed and flush pending settings before exiting.

#### Scenario: Logoff while running
- **WHEN** the user signs out of Windows while Navpaky is running
- **THEN** pending settings are written to disk before the process exits

### Requirement: Launch splash
On every start that becomes the running instance, including the start at sign-in (owner decision), Navpaky SHALL show a splash; a second launch that hands over to the running instance and exits shows none. The splash is centred on the primary screen for about three seconds (design decision): a small card where a caret types "Navpaky" marked as wrong, the word turns into the greeting in the UI language ("Привіт" for Ukrainian, "Hello" for English) marked as corrected, a pill with the Language code appears in the card's corner, and the card fades out.

#### Scenario: Ukrainian interface
- **WHEN** the UI language is Ukrainian and Navpaky starts
- **THEN** the splash types "Navpaky", which turns into "Привіт" with a "UK" pill, and fades out after about three seconds

#### Scenario: Second launch
- **WHEN** Navpaky is running and the user starts it again
- **THEN** the second process exits as the single-instance requirement says and shows no splash

### Requirement: Launch splash stays out of the way
The splash SHALL NOT take focus, SHALL let clicks pass through, SHALL have no taskbar button and SHALL NOT delay the tray icon or Auto Correction. When Windows animations are turned off, it SHALL show the greeting without motion and then disappear.

#### Scenario: Typing during the splash
- **WHEN** the user is typing in another window while the splash is shown
- **THEN** the keystrokes reach that window and the splash does not take focus

#### Scenario: Animations off
- **WHEN** Windows animation effects are off and Navpaky starts
- **THEN** the splash shows "Hello" or "Привіт" without the typing motion and disappears
