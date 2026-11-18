# Spec Delta

## MODIFIED Requirements

### Requirement: Clean uninstall
Uninstalling SHALL remove the app files, the Start menu entry and the sign-in entry, and SHALL ask whether to delete Rekey's data (settings, word lists, statistics, logs); data is kept if the user declines. The uninstaller SHALL wait for the answer before it reports the removal as complete, for at most 50 seconds (design decision: Velopack 1.2.161 ends its uninstall hook after 60 seconds); if the wait runs out the question stays open and data is deleted only after an explicit yes. Data SHALL be deleted only once the old installation is gone, and SHALL be kept when Rekey was installed again while the question was open.

#### Scenario: Uninstall with data removal
- **WHEN** the user uninstalls and chooses to delete data
- **THEN** `%LOCALAPPDATA%\Rekey` no longer exists and Rekey no longer starts at sign-in

#### Scenario: Uninstall keeping data
- **WHEN** the user uninstalls and declines to delete data
- **THEN** settings and word lists are still in `%LOCALAPPDATA%\Rekey` and a later reinstall uses them

#### Scenario: Question before completion
- **WHEN** the user uninstalls Rekey
- **THEN** the data question appears first, and the message that the removal is complete appears only after the user answers or 50 seconds pass

#### Scenario: Reinstalled while the question is open
- **WHEN** the user installs Rekey again while the data question of an earlier uninstall is still open, then answers that question with delete
- **THEN** `%LOCALAPPDATA%\Rekey` is kept for the new installation and a message says why
