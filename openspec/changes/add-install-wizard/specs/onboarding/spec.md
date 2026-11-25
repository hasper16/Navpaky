## MODIFIED Requirements

### Requirement: Welcome window on first run
On the first start for a Windows user, Navpaky SHALL open a welcome window that leads through three steps with Back, Next and Done:
1. What Navpaky does, with a before/after example and how to undo a Correction.
2. Try it, with the statement that what is typed there is sent nowhere.
3. Your choices.

Closing the window at any step SHALL count as completing it. The window SHALL NOT open after an install that chose "Use previous settings" in the install wizard. It SHALL open after a first install and after "Start fresh".

#### Scenario: First start
- **WHEN** Navpaky starts for the first time for this user
- **THEN** the welcome window opens at step 1 of 3

#### Scenario: Later starts
- **WHEN** the user completed or closed the welcome window before and Navpaky starts again
- **THEN** no welcome window opens

#### Scenario: Step through
- **WHEN** the user presses Next twice
- **THEN** the window shows step 3 of 3 with a Done button

#### Scenario: Small screen
- **WHEN** the screen is 1024×768 and the welcome window shows its tallest step, step 3 after an install without the wizard
- **THEN** the whole window, its Done button included, lies inside the screen's work area

#### Scenario: Reinstall keeping settings
- **WHEN** the user reinstalled with "Use previous settings" and Navpaky starts for the first time after that install
- **THEN** no welcome window opens

#### Scenario: Reinstall starting fresh
- **WHEN** the user reinstalled with "Start fresh" and Navpaky starts for the first time after that install
- **THEN** the welcome window opens at step 1 of 3

### Requirement: Main choices up front
The welcome window's last step SHALL do three things:
- let the user choose the Hotkey from the same options as Settings, drawn the same way (owner decision);
- offer one switch, "Send anonymous usage counters and crash reports", off by default, which replaces the crash-report switch (owner decision);
- show how to keep the tray icon visible.

The interface language, launch at sign-in and the desktop shortcut are chosen in the install wizard. When Navpaky was installed without the wizard, the welcome window SHALL ask them itself, so none goes unasked:
- the interface language on step 1;
- launch at sign-in, on by default (design decision), and the desktop shortcut, off by default, on step 3.

This covers the plain Velopack setup, for example.

#### Scenario: Installed with the wizard
- **WHEN** Navpaky was installed with the wizard and the welcome window opens
- **THEN** step 3 shows the Hotkey, the one telemetry switch and the tray tip, and no language, sign-in or shortcut choice

#### Scenario: Installed without the wizard
- **WHEN** Navpaky was installed with the plain Velopack setup and the welcome window opens
- **THEN** step 1 offers the interface language, and step 3 also offers launch at sign-in (on) and the desktop shortcut (off)

#### Scenario: Launch at sign-in default
- **WHEN** Navpaky was installed without the wizard and the user closes the welcome window without changing anything
- **THEN** launch at sign-in is on

#### Scenario: Hotkey from the welcome window
- **WHEN** the user picks Ctrl twice, or any other option Settings offers, on the last step
- **THEN** the Hotkey is that option, as if chosen in Settings
