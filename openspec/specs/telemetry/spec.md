# telemetry Specification

## Purpose
Defines what Navpaky may send over the network about crashes and usage, the consent required before anything is sent, and the data that must never leave the user's computer.

## Requirements

### Requirement: Nothing is sent without consent
Navpaky MUST NOT send crash reports, freeze reports, Usage Digests or the setup signal until the user has agreed. The welcome window SHALL ask about crash and freeze reports, with the choice off by default; Usage Digests and the setup signal SHALL be agreed in settings. The user SHALL be able to change either at any time in settings.

#### Scenario: Fresh install, no answer
- **WHEN** Navpaky is installed and the user closes the welcome window without turning on crash reports
- **THEN** Navpaky makes no telemetry network request

#### Scenario: Crash reports from the welcome window
- **WHEN** the user turns on crash reports in the welcome window
- **THEN** crash and freeze reports may be sent, and no Usage Digest or setup signal is sent until the user turns them on in settings

#### Scenario: Consent withdrawn
- **WHEN** the user turns diagnostics off in settings
- **THEN** no further telemetry is sent and any unsent reports are deleted

### Requirement: Typed text never leaves the computer
No network request made by Navpaky SHALL contain typed text, Words, Renderings, Protected Words, Learned Words, clipboard content, window titles, the Windows user name, file paths inside the user profile, or memory dumps. The one exception is the email address the user types into the opt-in sign-up form and explicitly submits, sent only in that sign-up request; keystroke-derived text, such as Words and Renderings, is never sent.

#### Scenario: Crash while a Word is buffered
- **WHEN** Navpaky crashes while the current Word is "secret"
- **THEN** the crash report contains no "secret" and no memory dump

### Requirement: Crash and freeze reports
With consent, Navpaky SHALL report unhandled exceptions and freezes of its UI or engine lasting longer than 2 seconds, including exception type, stack trace, Navpaky version, Windows version and the Incident types of the last 10 minutes (both periods are design decisions).

#### Scenario: Engine freeze
- **WHEN** consent is given and the engine thread does not respond for 3 seconds
- **THEN** a freeze report with the engine stack and versions is sent

### Requirement: Daily Usage Digest
With consent, Navpaky SHALL send at most one Usage Digest per day containing only counters, Navpaky and Windows versions, the UI language, the Language codes of installed Layouts, and on/off states of main settings. It SHALL carry no stable identifier of the user or device.

#### Scenario: Digest content
- **WHEN** a Usage Digest is sent
- **THEN** it contains counts such as Words analysed and Corrections, and no field that identifies the device across days

#### Scenario: Offline
- **WHEN** the digest cannot be sent today
- **THEN** Navpaky discards the unsent digest and makes no more Usage Digest attempts that day; the next day's attempt, if made, carries only the counters of the day before it

### Requirement: One-time setup signal
With consent, Navpaky SHALL send one anonymous setup signal when the welcome window closes, containing versions, UI language, number of installed Layouts and covered Languages, the main settings chosen, and how far the user got in the welcome window.

#### Scenario: Sent once
- **WHEN** the user closes the welcome window with consent and later reopens and closes it again
- **THEN** only one setup signal has been sent

### Requirement: Optional email sign-up
Navpaky SHALL offer an optional field where the user can enter an email address to hear about new versions or about problems they reported. Navpaky SHALL send the address only when the user submits it, only for the ticked purposes, and SHALL NOT link it to any report.

#### Scenario: Sign up
- **WHEN** the user enters an email, ticks "New versions" and submits
- **THEN** one request with the email and that purpose is sent
- **AND** no crash report or digest contains the email

### Requirement: Show what was sent
Settings SHALL show the content of the last 10 telemetry requests (design decision) exactly as sent.

#### Scenario: Inspect last digest
- **WHEN** the user opens "Show what was sent"
- **THEN** the last Usage Digest is shown in full

### Requirement: Secure transport
Telemetry SHALL be sent only over HTTPS with standard certificate validation to endpoints fixed in the app.

#### Scenario: Invalid certificate
- **WHEN** the telemetry endpoint presents an invalid certificate
- **THEN** nothing is sent and the report is retried later or discarded per its rules
