# Spec Delta

## MODIFIED Requirements

### Requirement: Hotkey choices
The user SHALL be able to choose the Hotkey from: double tap of Shift (default), double tap of Ctrl, double tap of Alt, Ctrl+Space, Win+Space, or a custom combination of modifiers and one key. Ctrl+Space and Win+Space are the Windows counterparts of the original's ⌃Space, ⌘Space and ⌥⌘Space options.

#### Scenario: Default Hotkey
- **WHEN** Navpaky is freshly installed
- **THEN** the Hotkey is a double tap of Shift

### Requirement: Correct the last Word
When the Hotkey is performed after a Word (with at most one Space or Tab typed after it), Navpaky SHALL correct that Word to its best Rendering in another active Layout and switch to that Layout. A Word followed by Enter is not corrected, because the line may already have been submitted (as for Undo).

#### Scenario: Doubtful word
- **WHEN** the user types "yt" followed by Space in English (US) and performs the Hotkey
- **THEN** the text becomes "не " and the Layout is Russian

#### Scenario: More than two Layouts
- **WHEN** English, Russian and Ukrainian are active and the user performs the Hotkey after "ghbdsn" typed in English (US)
- **THEN** the Word becomes its best-scoring Rendering, "привіт" in Ukrainian

#### Scenario: Word followed by Enter
- **WHEN** the user types "yt" followed by Enter in English (US) and performs the Hotkey
- **THEN** no text changes and the Layout becomes Russian

#### Scenario: Pause before the Hotkey
- **WHEN** the user types "ghbdtn", waits 5 seconds and performs the Hotkey
- **THEN** the text becomes "привет" and the Layout is Russian

### Requirement: Correct the selected text
When text is selected and the focused control exposes the selection, the Hotkey SHALL replace the selection with its Rendering in the best other active Layout, character by character, and switch to that Layout. A selection with a line break or longer than 500 characters (design decision) SHALL NOT be replaced; the Hotkey then performs only a Layout Switch.

#### Scenario: Selected sentence
- **WHEN** the user selects "ghbdtn vbh" and performs the Hotkey
- **THEN** the selection is replaced with "привет мир"

#### Scenario: Selection not readable
- **WHEN** text is selected but the control does not expose the selection
- **THEN** Navpaky performs only a Layout Switch and leaves the text unchanged

#### Scenario: Selection with a line break
- **WHEN** the user selects two lines of text and performs the Hotkey
- **THEN** Navpaky performs only a Layout Switch and leaves the text unchanged

### Requirement: Hotkey after a Correction reverts it
When the Hotkey is performed after a Correction under the same conditions as an Undo with Ctrl+Z (within the Undo Window, before any other key, not after an Enter-triggered Correction), Navpaky SHALL revert that Correction as an Undo. Otherwise the Hotkey acts as described above.

#### Scenario: Hotkey reverts
- **WHEN** Auto Correction changed "ghbdtn " to "привет " and the user performs the Hotkey
- **THEN** the text reads "ghbdtn " and the Layout is English (US)
- **AND** an Undo is recorded

#### Scenario: After an Enter-triggered Correction
- **WHEN** a Correction was triggered by Enter and the user performs the Hotkey within 5 seconds
- **THEN** the Correction is not reverted

### Requirement: Conflict warning
When the chosen Hotkey matches the Windows Layout Switch shortcut or a hotkey another application has registered, Navpaky SHALL say which action it is taking over and offer to pick a different Hotkey. Navpaky SHALL NOT change Windows settings itself (as in the original, which asks the user to change the system shortcut).

#### Scenario: Same as Windows shortcut
- **WHEN** the user picks Win+Space as a custom Hotkey
- **THEN** Navpaky explains that the Hotkey will replace the Windows Layout Switch shortcut and keeps the previous Hotkey until the user confirms or picks another

#### Scenario: Taken by another app
- **WHEN** the user picks a custom Hotkey that another running application has registered
- **THEN** Navpaky shows that the combination is already in use and keeps the previous Hotkey until the user confirms or picks another
