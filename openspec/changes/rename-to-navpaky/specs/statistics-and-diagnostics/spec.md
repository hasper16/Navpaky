# Spec Delta

## MODIFIED Requirements

### Requirement: Usage counters
When statistics are on (default), Navpaky SHALL count Words analysed, Auto Corrections, Manual Corrections, Undos, Decisions by reason, and replacement duration, per day, for the last 30 days (design decision). Counters SHALL contain no words or text.

#### Scenario: A Correction is counted
- **WHEN** an Auto Correction happens
- **THEN** today's Auto Correction count increases by one

#### Scenario: Statistics off
- **WHEN** the user turns statistics off
- **THEN** no counters are updated and stored counters are deleted

### Requirement: Incident log
Navpaky SHALL keep a local log of Incidents with time, type and non-text details, limited to the last 500 entries (design decision).

#### Scenario: Hook reinstalled
- **WHEN** Navpaky reattaches to the keyboard after Windows dropped it
- **THEN** an Incident with type "keyboard reattached" and the time is added

#### Scenario: Log limit
- **WHEN** the 501st Incident is recorded
- **THEN** the oldest Incident is removed

### Requirement: Word-analysis log is opt-in and local
Navpaky SHALL keep a word-analysis log only when the user turns it on (default off). The log SHALL record each analysed Word, its Renderings and the Decision, SHALL keep at most the last 7 days (design decision), SHALL be readable only by the current Windows user, and MUST NOT be sent anywhere by Navpaky.

#### Scenario: Default
- **WHEN** Navpaky is freshly installed
- **THEN** no word-analysis log file exists

#### Scenario: Retention
- **WHEN** the log is on and an entry is older than 7 days
- **THEN** the entry is deleted

#### Scenario: Turned off
- **WHEN** the user turns the word-analysis log off
- **THEN** the existing log file is deleted

### Requirement: Diagnostics export with preview
The user SHALL be able to export diagnostics to the clipboard or a file. Before export, Navpaky SHALL show the exact content. The export SHALL include versions, Windows version, installed Layouts, settings, counters and Incidents, and SHALL exclude typed text, Protected Words, Learned Words and window titles unless the user explicitly adds the word-analysis log.

#### Scenario: Default export
- **WHEN** the user exports diagnostics without changing options
- **THEN** the exported text contains no typed words, Protected Words, Learned Words or window titles

#### Scenario: Copy to clipboard
- **WHEN** the user chooses Copy in the export dialog
- **THEN** the previewed text is placed on the clipboard; this explicit action is the only way Navpaky writes the clipboard

#### Scenario: Include analysis log
- **WHEN** the user ticks "Include word-analysis log" and confirms the preview
- **THEN** the export includes the log entries shown in the preview

### Requirement: Personal paths are masked
Exported diagnostics SHALL replace the user's profile path and Windows user name with placeholders.

#### Scenario: Path in an Incident
- **WHEN** an Incident mentions `C:\Users\anna\AppData\Local\Navpaky`
- **THEN** the export shows `%USERPROFILE%\AppData\Local\Navpaky`
