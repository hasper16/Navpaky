# Spec Delta

## ADDED Requirements

### Requirement: Warn before a reinstall deletes extra files
When the target folder already holds a Navpaky install, the Folder page SHALL list the files and folders at that folder's top level that are not part of the install, and say that the reinstall deletes them. Velopack's setup replaces the whole folder (observed in the 2026-11-24 live run). The list SHALL show at most 8 entries, then how many more there are (design decision).

#### Scenario: Extra files in the existing install folder
- **WHEN** Navpaky is installed for the user and its folder also holds a file `notes.txt` and a folder `backup` the user put there
- **THEN** the Folder page names `notes.txt` and `backup` and says they will be deleted

#### Scenario: Many extra entries
- **WHEN** the folder holds 12 entries that are not part of the install
- **THEN** the page names 8 of them and says there are 4 more

#### Scenario: Only the install
- **WHEN** the folder holds only the Navpaky install
- **THEN** the Folder page shows no warning and no confirmation

### Requirement: Confirm before deleting extra files
When the Folder page lists entries a reinstall would delete, it SHALL offer a check box to delete them and reinstall, off by default, and Next SHALL stay disabled until it is checked. Until the install starts, nothing in the folder SHALL change: going back, leaving the box unchecked or closing the wizard keeps every entry.

#### Scenario: Not confirmed
- **WHEN** the page lists extra entries and the box is not checked
- **THEN** Next is disabled

#### Scenario: Confirmed
- **WHEN** the user checks the box
- **THEN** Next is enabled and the install proceeds into that folder

#### Scenario: Closed without confirming
- **WHEN** the user closes the wizard on the Folder page without checking the box
- **THEN** every listed entry is still in the folder
