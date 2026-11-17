# Spec Delta

## MODIFIED Requirements

### Requirement: Single instance per user session
Only one Rekey process SHALL run per Windows user session. A second launch MUST exit without side effects and leave the running instance untouched. When the second launch carries command-line arguments, which the running instance does not receive, it SHALL tell the user that Rekey is already running before it exits (design decision of add-final-acceptance).

#### Scenario: Second launch
- **WHEN** Rekey is already running and the user launches it again
- **THEN** the second process exits within 2 seconds (design decision)
- **AND** only one tray icon remains

#### Scenario: Second launch with arguments
- **WHEN** Rekey is already running and the user launches it again with arguments
- **THEN** a message says Rekey is already running and must be quit first for the arguments to apply
- **AND** the second process exits once the message is closed and only one tray icon remains

#### Scenario: Another user session
- **WHEN** Rekey runs in user session A and is launched in user session B
- **THEN** both instances run independently
