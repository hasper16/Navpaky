# Spec Delta

## MODIFIED Requirements

### Requirement: Excluded Apps editor
The Apps and words section SHALL let the user add Excluded Apps from running applications or by choosing an executable, and remove them. An Excluded App chosen as an executable SHALL also match a running app whose executable has the same file name, ignoring case, so that a launcher stub and the app it starts count as one (design decision of add-final-acceptance; two different programs with the same file name are both excluded).

#### Scenario: Add from running apps
- **WHEN** the user picks Windows Terminal from the running apps list
- **THEN** Windows Terminal is listed as an Excluded App with its name and icon

#### Scenario: Program chosen through a launcher stub
- **WHEN** the user chooses `C:\Windows\notepad.exe` and Windows 11 runs Notepad from its Store package folder
- **THEN** Rekey does not correct Words typed in Notepad
