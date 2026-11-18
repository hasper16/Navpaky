# app-lifecycle Specification

## Purpose
Defines how Navpaky runs as a per-user background process on Windows: start-up without a main window, single-instance guarantee, per-user settings storage, and clean shutdown.

## Requirements

### Requirement: Background start without a main window
The app SHALL start without opening a main window or a taskbar button and SHALL show a notification-area (tray) icon while running.

#### Scenario: Normal start
- **WHEN** the user launches Navpaky
- **THEN** a Navpaky icon appears in the notification area
- **AND** no window and no taskbar button are shown

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
