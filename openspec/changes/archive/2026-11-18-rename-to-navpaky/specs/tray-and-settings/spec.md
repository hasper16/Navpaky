# Spec Delta

## MODIFIED Requirements

### Requirement: Settings window
The settings window SHALL offer every user setting of Navpaky grouped into General, Correction, Hotkey, Apps and words, Diagnostics and privacy, and About. Changes SHALL take effect immediately without a Save button.

#### Scenario: Change Sensitivity
- **WHEN** the user selects Cautious in the Correction section
- **THEN** the next Word is evaluated with Cautious Sensitivity

#### Scenario: Reopen settings
- **WHEN** the settings window is already open and the user selects Settings again
- **THEN** the existing window comes to the front instead of a second window

### Requirement: Excluded Apps editor
The Apps and words section SHALL let the user add Excluded Apps from running applications or by choosing an executable, and remove them. An Excluded App chosen as an executable SHALL also match a running packaged (Store) app whose executable has the same file name, ignoring case, so that a launcher stub and the Store app it starts count as one (design decision of add-final-acceptance). Programs that are not packaged still match by their full path only.

#### Scenario: Add from running apps
- **WHEN** the user picks Windows Terminal from the running apps list
- **THEN** Windows Terminal is listed as an Excluded App with its name and icon

#### Scenario: Program chosen through a launcher stub
- **WHEN** the user chooses `C:\Windows\notepad.exe` and Windows 11 runs Notepad from its Store package folder
- **THEN** Navpaky does not correct Words typed in Notepad

### Requirement: Cursor indicator
When enabled (default on, design decision), after each Correction or Layout Switch made by Navpaky the app SHALL show a small badge with the new Language code next to the text caret for about one second (design decision), without taking focus. When the caret position is unknown, the badge SHALL appear next to the mouse pointer.

#### Scenario: After a Correction
- **WHEN** a Correction switches to Russian
- **THEN** a badge reading "RU" appears next to the caret and disappears after about one second
- **AND** typing continues in the same window without interruption

### Requirement: Switch sound
When enabled (default off, design decision), Navpaky SHALL play the chosen sound after each Correction and not after an Undo. The user SHALL be able to choose among at least three sounds (design decision) and preview them.

#### Scenario: Sound on
- **WHEN** the switch sound is on and a Correction happens
- **THEN** the chosen sound plays once

### Requirement: Launch at sign-in
The user SHALL be able to turn launch at sign-in on or off. When on, Navpaky SHALL start automatically after the user signs in to Windows.

#### Scenario: Sign in
- **WHEN** launch at sign-in is on and the user signs in
- **THEN** Navpaky is running and its tray icon is visible

#### Scenario: Removed in Task Manager
- **WHEN** the user disables Navpaky under Startup apps in Task Manager
- **THEN** the settings window shows launch at sign-in as off
