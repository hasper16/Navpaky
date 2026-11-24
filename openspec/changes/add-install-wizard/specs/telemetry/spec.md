## MODIFIED Requirements

### Requirement: Nothing is sent without consent
Navpaky MUST NOT send crash reports, freeze reports, Usage Digests or the setup signal until the user has agreed. The welcome window SHALL ask once, with one switch that is off by default: "Send anonymous usage counters and crash reports". That switch covers crash and freeze reports, Usage Digests and the setup signal together (owner decision; the setup signal has always followed the Usage Digest consent). Settings SHALL keep crash reports and Usage Digests as separate switches, and the user SHALL be able to change either at any time.

#### Scenario: Fresh install, no answer
- **WHEN** Navpaky is installed and the user closes the welcome window without turning the switch on
- **THEN** Navpaky makes no telemetry network request

#### Scenario: Crash reports from the welcome window
- **WHEN** the user turns on "Send anonymous usage counters and crash reports" in the welcome window
- **THEN** crash and freeze reports, Usage Digests and the setup signal may be sent
- **AND** Settings shows both crash reports and Usage Digests as on

#### Scenario: Consent withdrawn
- **WHEN** the user turns diagnostics off in settings
- **THEN** no further telemetry is sent and any unsent reports are deleted
