## MODIFIED Requirements

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
