## MODIFIED Requirements

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

### Requirement: Main choices up front
The welcome window's last step SHALL let the user choose the Hotkey between pressing Shift twice and pressing Ctrl twice, SHALL show how to keep the tray icon visible, and SHALL offer crash reports, as in the approved prototype (owner decision). Launch at sign-in SHALL be on after the first start (design decision); Auto Correction, launch at sign-in and the other Hotkeys are set in Settings.

#### Scenario: Launch at sign-in default
- **WHEN** the user closes the welcome window opened at the first start without changing anything
- **THEN** launch at sign-in is on

#### Scenario: Hotkey from the welcome window
- **WHEN** the user picks Ctrl twice on the last step
- **THEN** the Hotkey is pressing Ctrl twice, as if chosen in Settings
