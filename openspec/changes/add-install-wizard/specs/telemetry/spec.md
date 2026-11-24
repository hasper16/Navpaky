## MODIFIED Requirements

### Requirement: Nothing is sent without consent
Navpaky MUST NOT send crash reports, freeze reports, Usage Digests or the setup signal until the user has agreed. The welcome window SHALL ask once, with one switch that is off by default: "Send anonymous usage counters and crash reports". That switch covers crash and freeze reports and Usage Digests together (owner decision). The setup signal SHALL be agreed in settings. Settings SHALL keep crash reports and Usage Digests as separate switches, and the user SHALL be able to change any of them at any time.

#### Scenario: Fresh install, no answer
- **WHEN** Navpaky is installed and the user closes the welcome window without turning the switch on
- **THEN** Navpaky makes no telemetry network request

#### Scenario: Crash reports from the welcome window
- **WHEN** the user turns on "Send anonymous usage counters and crash reports" in the welcome window
- **THEN** crash and freeze reports and Usage Digests may be sent
- **AND** no setup signal is sent until the user turns it on in settings
- **AND** Settings shows both crash reports and Usage Digests as on

#### Scenario: Consent withdrawn
- **WHEN** the user turns diagnostics off in settings
- **THEN** no further telemetry is sent and any unsent reports are deleted
