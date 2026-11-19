## ADDED Requirements

### Requirement: Input timing survives sleep
Navpaky SHALL measure the times of Keystrokes, clicks, focus changes and the session's last input on one clock that counts the same way as the times Windows reports for them, including the time the PC spent asleep or hibernated (design decision: the .NET 11 runtime clock leaves sleep out, Windows' input times do not).

#### Scenario: Hook watchdog after wake
- **WHEN** the PC wakes from sleep, the keyboard watchdog is on, and Windows stops delivering Keystrokes to Navpaky while the user keeps typing
- **THEN** Navpaky detects the drop and reattaches as it does before any sleep

#### Scenario: No false drop after wake
- **WHEN** the PC wakes from sleep and the hook keeps delivering Keystrokes
- **THEN** Navpaky does not report a dropped hook and records no Incident

#### Scenario: Focus change after wake
- **WHEN** the PC wakes from sleep and focus moves to another window just before a Keystroke
- **THEN** Navpaky handles them as it does before any sleep: a focus change Windows reported before the Keystroke counts at its own time, and a Keystroke whose focus change is reported later is not kept, since its focus is uncertain
