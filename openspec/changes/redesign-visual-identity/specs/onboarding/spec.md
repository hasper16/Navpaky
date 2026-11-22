## MODIFIED Requirements

### Requirement: Welcome window on first run
On the first start for a Windows user, Navpaky SHALL open a welcome window that leads through three steps with Back, Next and Done: what Navpaky does (with a before/after example, the statement that typed text never leaves the computer, and how to undo a Correction), try it, and your choices. Closing the window at any step SHALL count as completing it.

#### Scenario: First start
- **WHEN** Navpaky starts for the first time for this user
- **THEN** the welcome window opens at step 1 of 3

#### Scenario: Later starts
- **WHEN** the user completed or closed the welcome window before and Navpaky starts again
- **THEN** no welcome window opens

#### Scenario: Step through
- **WHEN** the user presses Next twice
- **THEN** the window shows step 3 of 3 with a Done button

### Requirement: Reopen from tray
The user SHALL be able to reopen the welcome window from the tray menu and the quick panel at any time, through an item named "First steps" ("Перші кроки" in Ukrainian; owner decision), which does not read like the Settings item.

#### Scenario: Reopen
- **WHEN** the user selects "First steps" in the tray menu
- **THEN** the welcome window opens at step 1 with current settings
