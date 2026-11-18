# Spec Delta

## MODIFIED Requirements

### Requirement: Protected Words are never auto-corrected
Navpaky SHALL NOT auto-correct a Word whose Rendering in the current Layout matches a Protected Word, ignoring case. A deferred run containing a Protected Word SHALL NOT be corrected.

#### Scenario: Protected brand name
- **WHEN** "ntcn" is a Protected Word and the user types "ntcn" followed by Space in English (US)
- **THEN** the text stays "ntcn "

#### Scenario: Run with a Protected Word
- **WHEN** a deferred run would correct "yt ntcn" and "ntcn" is a Protected Word
- **THEN** neither Word is corrected

### Requirement: Protection learned from repeated Undos
When the user undoes a Correction, Navpaky SHALL add the Word to the Protected Words, as the original does after one Undo. Words shorter than 3 letters SHALL NOT be protected automatically (the original skips short words; the length is a design decision).

#### Scenario: Undo protects the Word
- **WHEN** the user undoes the Correction of "ntcn"
- **THEN** "ntcn" becomes a Protected Word
- **AND** the next "ntcn" followed by Space is not corrected

#### Scenario: Short word
- **WHEN** the user undoes the Correction of "yt"
- **THEN** "yt" does not become a Protected Word

### Requirement: Learned Words from Manual Correction
When the user manually corrects a Word whose new Rendering is not a word of the target Language, Navpaky SHALL add that Rendering as a Learned Word of that Language. A Learned Word SHALL count as a common word of its Language in later Decisions.

#### Scenario: New slang word
- **WHEN** the user manually corrects "rhby;" to "кринж" and "кринж" is not in the Russian Language Profile
- **THEN** "кринж" becomes a Learned Word of Russian
- **AND** the next time "rhby;" is typed in English (US), Auto Correction corrects it to "кринж"

### Requirement: Undoing a learned correction removes the Learned Word
When the user undoes a Correction whose target Rendering is a Learned Word, Navpaky SHALL remove that Learned Word.

#### Scenario: Learned by mistake
- **WHEN** "кринж" is a Learned Word and the user undoes a Correction to "кринж"
- **THEN** "кринж" is no longer a Learned Word

### Requirement: Lists persist and stay bounded
Protected Words and Learned Words SHALL persist across restarts. Each list SHALL hold up to 10,000 entries (design decision); when full, the least recently used automatic entry is dropped first and user-added entries are never dropped.

#### Scenario: Restart
- **WHEN** Navpaky restarts
- **THEN** all Protected Words and Learned Words are still present
