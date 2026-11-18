# Spec Delta

## MODIFIED Requirements

### Requirement: Key order is preserved
The Trigger Key that caused a Correction, and any keys the user presses while the Correction is being applied, SHALL reach the application after the corrected text and in the order they were pressed. This holds for a replacement typed in several parts too: keys pressed between the parts are held until the last part is typed (owner decision in add-final-acceptance).

#### Scenario: Space after correction
- **WHEN** the user types "ghbdtn" followed by Space
- **THEN** the text reads "привет " with the space after the corrected word

#### Scenario: Enter in a chat box
- **WHEN** the user types "ghbdtn" followed by Enter in a chat box where Enter sends the message
- **THEN** the sent message reads "привет"

#### Scenario: Fast typing during a Correction
- **WHEN** the user keeps typing "rfr" while a Correction is being applied
- **THEN** "rfr" appears after the corrected text, in order

#### Scenario: Typing during a replacement in several parts
- **WHEN** a replacement that contains spaces is typed in parts and the user presses "f" while it pauses between two parts
- **THEN** "f" appears after the whole replacement, not inside it
