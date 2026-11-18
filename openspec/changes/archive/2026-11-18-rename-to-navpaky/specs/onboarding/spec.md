# Spec Delta

## MODIFIED Requirements

### Requirement: Welcome window on first run
On the first start for a Windows user, Navpaky SHALL open a welcome window that explains in plain words what Navpaky does, states that typed text never leaves the computer, and shows how to undo a Correction.

#### Scenario: First start
- **WHEN** Navpaky starts for the first time for this user
- **THEN** the welcome window opens

#### Scenario: Later starts
- **WHEN** the user completed or closed the welcome window before and Navpaky starts again
- **THEN** no welcome window opens

### Requirement: Try it
The welcome window SHALL contain a text field where the user can type a word in the wrong Layout and see Navpaky correct it, with a short instruction naming their installed Layouts.

#### Scenario: Successful try
- **WHEN** English (US) and Russian are installed, Auto Correction is on, and the user types "ghbdtn" followed by Space in the try-it field with English (US) active
- **THEN** the field shows "привет "
- **AND** the welcome window confirms that Navpaky works

#### Scenario: Only one Layout installed
- **WHEN** the user has only one Layout installed
- **THEN** the welcome window explains that Navpaky needs at least two Layouts and links to Windows language settings

### Requirement: Reopen from tray
The user SHALL be able to reopen the welcome window from the tray menu at any time, through an item whose name does not read like the Settings item (owner review of the Ukrainian interface in add-final-acceptance).

#### Scenario: Reopen
- **WHEN** the user selects "Getting started with Navpaky" in the tray menu
- **THEN** the welcome window opens with current settings

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
