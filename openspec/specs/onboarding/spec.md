# onboarding Specification

## Purpose

Defines the first-run experience: explaining what Navpaky does and how it treats typed text, letting the user see a Correction work, and warning when Navpaky runs from a place where it will not keep working.

## Requirements

### Requirement: Welcome window on first run
On the first start for a Windows user, Navpaky SHALL open a welcome window that leads through three steps with Back, Next and Done: what Navpaky does (with a before/after example and how to undo a Correction), try it (with the statement that what is typed there is sent nowhere), and your choices. Closing the window at any step SHALL count as completing it.

#### Scenario: First start
- **WHEN** Navpaky starts for the first time for this user
- **THEN** the welcome window opens at step 1 of 3

#### Scenario: Later starts
- **WHEN** the user completed or closed the welcome window before and Navpaky starts again
- **THEN** no welcome window opens

#### Scenario: Step through
- **WHEN** the user presses Next twice
- **THEN** the window shows step 3 of 3 with a Done button

### Requirement: Try it
The welcome window SHALL contain a text field where the user can type a word in the wrong Layout and see Navpaky correct it, with a short instruction naming their installed Layouts.

#### Scenario: Successful try
- **WHEN** English (US) and Russian are installed, Auto Correction is on, and the user types "ghbdtn" followed by Space in the try-it field with English (US) active
- **THEN** the field shows "привет "
- **AND** the welcome window confirms that Navpaky works

#### Scenario: Only one Layout installed
- **WHEN** the user has only one Layout installed
- **THEN** the welcome window explains that Navpaky needs at least two Layouts and links to Windows language settings

### Requirement: Main choices up front
The welcome window's last step SHALL let the user choose the Hotkey from the same options as Settings, drawn the same way (owner decision), SHALL show how to keep the tray icon visible, and SHALL offer crash reports, as in the approved prototype (owner decision). Launch at sign-in SHALL be on after the first start (design decision); Auto Correction, launch at sign-in and the other Hotkeys are set in Settings.

#### Scenario: Launch at sign-in default
- **WHEN** the user closes the welcome window opened at the first start without changing anything
- **THEN** launch at sign-in is on

#### Scenario: Hotkey from the welcome window
- **WHEN** the user picks Ctrl twice, or any other option Settings offers, on the last step
- **THEN** the Hotkey is that option, as if chosen in Settings

### Requirement: Reopen from tray
The user SHALL be able to reopen the welcome window from the tray menu and the quick panel at any time, through an item named "First steps" ("Перші кроки" in Ukrainian; owner decision), which does not read like the Settings item.

#### Scenario: Reopen
- **WHEN** the user selects "First steps" in the tray menu
- **THEN** the welcome window opens at step 1 with current settings

### Requirement: Warn about temporary locations
When Navpaky runs from a temporary folder, a ZIP preview, or the Downloads folder rather than its installed location, it SHALL warn that it will stop working when that copy is removed and SHALL offer to install it properly.

#### Scenario: Run from a ZIP preview
- **WHEN** the user starts Navpaky directly from inside a ZIP file opened in Explorer
- **THEN** Navpaky shows the warning with an Install option

### Requirement: Notice when the app files disappear
When the app's own executable is moved or deleted while Navpaky runs, Navpaky SHALL tell the user that it will stop after the next restart and how to reinstall.

#### Scenario: Cleanup tool removed the app
- **WHEN** the Navpaky executable is deleted while Navpaky runs
- **THEN** Navpaky shows a notice within one minute (design decision) explaining how to reinstall
