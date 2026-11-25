# onboarding Specification

## Purpose

Defines the first-run experience: explaining what Navpaky does and how it treats typed text, letting the user see a Correction work, and warning when Navpaky runs from a place where it will not keep working.

## Requirements

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
