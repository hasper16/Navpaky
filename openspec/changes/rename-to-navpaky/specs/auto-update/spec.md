# Spec Delta

## MODIFIED Requirements

### Requirement: Per-user installation
The installer SHALL install Navpaky for the current Windows user without administrator rights, add a Start menu entry and an entry in Windows "Installed apps", and start Navpaky when done.

#### Scenario: Standard user installs
- **WHEN** a user without administrator rights runs the installer
- **THEN** Navpaky is installed, appears in the Start menu and Installed apps, and is running

#### Scenario: Installed under the product name
- **WHEN** the installer finishes
- **THEN** the app files are under `%LOCALAPPDATA%\navpaky-app`, the Start menu and Installed apps entries read "Navpaky", settings are kept under `%APPDATA%\Navpaky` and machine-local state under `%LOCALAPPDATA%\Navpaky`

### Requirement: Automatic update checks
When automatic checks are on (default), Navpaky SHALL check for updates at start and every 24 hours (design decision). The tray menu item "Check for updates" SHALL check immediately and report the result.

#### Scenario: No update
- **WHEN** the user selects "Check for updates" and Navpaky is current
- **THEN** Navpaky says it is up to date

#### Scenario: Update available
- **WHEN** a newer version exists
- **THEN** Navpaky downloads it in the background and offers "Restart to update"

### Requirement: Only authentic updates are applied
Navpaky SHALL download update feeds and packages only over HTTPS from the project's GitHub Releases, fixed in the app, and SHALL apply a package only if its checksum matches the feed and its executables carry a valid Authenticode signature from the Navpaky publisher. Otherwise the update MUST be discarded and an Incident recorded.

#### Scenario: Tampered package
- **WHEN** a downloaded package's checksum does not match the feed
- **THEN** the package is discarded and the current version keeps running

#### Scenario: Wrong signer
- **WHEN** a package's checksum matches but its executable is unsigned or signed by another publisher
- **THEN** the package is discarded and an Incident records the signature failure

### Requirement: No downgrades
Navpaky SHALL NOT apply a package whose version is lower than or equal to the running version.

#### Scenario: Older version in feed
- **WHEN** the feed lists a version lower than the running one as latest
- **THEN** Navpaky does not install it

### Requirement: Update preserves user data
Applying an update SHALL keep settings, word lists, statistics and launch-at-sign-in state.

#### Scenario: After update
- **WHEN** Navpaky restarts into a new version
- **THEN** all settings and Protected Words are unchanged

### Requirement: Clean uninstall
Uninstalling SHALL remove the app files, the Start menu entry and the sign-in entry, and SHALL ask once whether to delete Navpaky's data (settings, word lists, statistics, logs, in both data folders); both folders are kept if the user declines and deleted only on an explicit yes. The uninstaller SHALL wait for the answer before it reports the removal as complete, for at most 50 seconds (design decision: Velopack 1.2.161 ends its uninstall hook after 60 seconds); if the wait runs out the question stays open and data is deleted only after an explicit yes. Data SHALL be deleted only once the old installation is gone, and SHALL be kept when Navpaky was installed again while the question was open.

#### Scenario: Uninstall with data removal
- **WHEN** the user uninstalls and chooses to delete data
- **THEN** neither `%APPDATA%\Navpaky` nor `%LOCALAPPDATA%\Navpaky` exists and Navpaky no longer starts at sign-in

#### Scenario: Uninstall keeping data
- **WHEN** the user uninstalls and declines to delete data
- **THEN** settings and word lists are still in `%APPDATA%\Navpaky`, statistics and logs in `%LOCALAPPDATA%\Navpaky`, and a later reinstall uses them

#### Scenario: Question before completion
- **WHEN** the user uninstalls Navpaky
- **THEN** the data question appears first, and the message that the removal is complete appears only after the user answers or 50 seconds pass

#### Scenario: Reinstalled while the question is open
- **WHEN** the user installs Navpaky again while the data question of an earlier uninstall is still open, then answers that question with delete
- **THEN** `%APPDATA%\Navpaky` and `%LOCALAPPDATA%\Navpaky` are kept for the new installation and a message says why
