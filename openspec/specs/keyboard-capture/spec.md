# keyboard-capture Specification

## Purpose
Defines how Navpaky observes the user's typing across all applications, where Words begin and end, which input it must never record, and how it stays attached to the keyboard when Windows interferes.

## Requirements

### Requirement: Observe typing without disturbing it
While running, Navpaky SHALL observe every Keystroke in the user's session and SHALL pass each one on to the focused application unchanged, unless another requirement explicitly holds it.

#### Scenario: Normal typing
- **WHEN** the user types "hello" in any application
- **THEN** the application receives "hello" exactly as typed
- **AND** Navpaky's current Word is "hello"

### Requirement: Keys that build a Word
Keys that produce a character in any active Layout (letters, digits, punctuation) SHALL extend the current Word, except Space, Enter and Tab. Backspace SHALL remove its last Keystroke.

#### Scenario: Backspace
- **WHEN** the user types "ghbdtnn" and presses Backspace once
- **THEN** the current Word is "ghbdtn"

#### Scenario: Punctuation inside a Word
- **WHEN** the user types "ghbdtn!" in English (US)
- **THEN** the current Word is "ghbdtn!"

### Requirement: Keys and events that end a Word
Space, Enter (on the main keyboard or the numeric keypad) and Tab SHALL end the Word whatever character they produce, as in the original; only a Trigger Key makes Navpaky evaluate it (spec `auto-correction`). Arrow keys, Home, End, Page Up, Page Down, Delete, Escape, any key combined with Ctrl, Alt or Win, a mouse click, or a focus change SHALL end it without evaluation, except as stated under "Suggestion list focus keeps the Word".

#### Scenario: Space that is not a Trigger Key
- **WHEN** Space is not a Trigger Key and the user types "ghb", Space, then "dtn"
- **THEN** the current Word is "dtn" and "ghb" was not evaluated

#### Scenario: Caret moved
- **WHEN** the user types "ghb", presses Left Arrow, then types "dtn"
- **THEN** the current Word is "dtn"

#### Scenario: Mouse click
- **WHEN** the user types "ghb" and clicks elsewhere in the text
- **THEN** the current Word is empty

### Requirement: Typing gap ends the Word
A pause of more than 2 seconds between Keystrokes SHALL end the current Word and drop any Deferred Words.

#### Scenario: Pause mid-word
- **WHEN** the user types "ghb", waits 3 seconds, then types "dtn"
- **THEN** the current Word is "dtn" and no Deferred Word remains

### Requirement: Password Fields are never recorded
Navpaky MUST NOT record Keystrokes, keep a Word, or make any Correction while the focused control is a Password Field, except for the known limit stated under "Each Keystroke is confirmed outside a Password Field" (a page script moving focus there and back within milliseconds).

#### Scenario: Typing a password
- **WHEN** the user types in a password box
- **THEN** Navpaky keeps no Keystrokes from that typing and makes no Correction

### Requirement: Each Keystroke is confirmed outside a Password Field
Navpaky SHALL keep a Keystroke only after confirming that the focused control is not a Password Field while nothing typed or clicked after that Keystroke is waiting to be processed (design decision), so a focus move inside one window (Tab, Shift+Tab, a click, the page itself) is covered before Windows reports it. When Navpaky cannot confirm this, it SHALL keep nothing more of that Word, which then gets no Correction.

#### Scenario: Focus moves to a password box inside the same window
- **WHEN** the user presses Tab from a user-name box into a password box on the same web page and types at once
- **THEN** Navpaky keeps none of the Keystrokes typed in the password box, even before Windows reports the focus change

#### Scenario: Navpaky falls behind the typing
- **WHEN** more input is already waiting while Navpaky processes a Keystroke
- **THEN** Navpaky keeps nothing more of that Word and makes no Correction of it

#### Scenario: Known limit — a script moves focus there and back at once
- **WHEN** a page script moves focus into a Password Field and back to the same control within milliseconds of a Keystroke, before Windows reports either move
- **THEN** that Keystroke may reach the Word until the next focus notice clears it (accepted by the owner: Navpaky sees focus only after the Keystroke)

### Requirement: Own input is ignored
Input that Navpaky injects itself SHALL NOT be recorded as user typing.

#### Scenario: After a Correction
- **WHEN** Navpaky retypes a Word as part of a Correction
- **THEN** the retyped characters do not appear in the current Word or in statistics as user typing

### Requirement: Keyboard observation recovers automatically
While the keyboard watchdog is on (default, as in the original), when Windows stops delivering Keystrokes to Navpaky while the user is typing, Navpaky SHALL detect it within 2 seconds (design decision) of the next user input, reattach, and record an Incident. The user SHALL be able to turn the keyboard watchdog off; then Navpaky does not check for a dropped hook.

#### Scenario: Hook dropped by Windows
- **WHEN** Windows stops delivering Keystrokes to Navpaky and the user keeps typing
- **THEN** Navpaky reattaches within 2 seconds of detecting missing input
- **AND** an Incident records the drop and the recovery

#### Scenario: Secure desktop
- **WHEN** a UAC prompt, the lock screen or the Ctrl+Alt+Del screen takes the input and the user types or clicks there
- **THEN** Navpaky does not treat that input as a dropped hook and records no Incident

#### Scenario: Watchdog turned off
- **WHEN** the keyboard watchdog is off and Windows stops delivering Keystrokes to Navpaky
- **THEN** Navpaky does not reattach on its own and records no Incident

### Requirement: Self-pause after repeated failures
If keyboard observation fails twice within 10 minutes, Navpaky SHALL pause Auto Correction, tell the user through the tray, and retry automatically after 10 minutes (both periods are design decisions; the original shows the same self-pause message). The user SHALL be able to resume manually at any time.

#### Scenario: Two failures in a row
- **WHEN** keyboard observation fails twice within 10 minutes
- **THEN** Auto Correction is paused and the tray shows that it paused itself
- **AND** Navpaky retries after 10 minutes

#### Scenario: Manual resume
- **WHEN** Auto Correction paused itself and the user chooses Resume in the tray
- **THEN** Navpaky reattaches immediately and Auto Correction resumes

### Requirement: Restart after a repeated freeze
When Navpaky's own processing of input stops responding for more than 5 seconds a second time within one hour (design decisions), Navpaky SHALL record an Incident and, while "Restart after a repeated freeze" is on (default, as in the original), restart itself, at most once per 10 minutes (as in the original).

#### Scenario: Second freeze
- **WHEN** input processing froze once and freezes again for more than 5 seconds 20 minutes later
- **THEN** Navpaky restarts, its tray icon is back, and an Incident records the restart

#### Scenario: Restart limit
- **WHEN** Navpaky restarted itself 4 minutes ago and input processing freezes twice more
- **THEN** Navpaky does not restart again before 10 minutes have passed since the last restart

#### Scenario: Restart turned off
- **WHEN** "Restart after a repeated freeze" is off and input processing freezes a second time within one hour
- **THEN** Navpaky records an Incident and keeps running without restarting

### Requirement: Elevated windows
When the foreground window belongs to a process with higher privileges than Navpaky, Navpaky SHALL make no Correction there and SHALL NOT treat the missing input as a failure.

#### Scenario: Administrator console
- **WHEN** the user types in a console running as administrator while Navpaky runs as a normal user
- **THEN** Navpaky makes no Correction and records no Incident

### Requirement: Suggestion list focus keeps the Word
While the user types the Keystrokes of a Word into a text control, a focus move inside the same window to an element that takes no text and is not a menu bar, menu, menu item, button or Password Field (such as a row of an autocomplete suggestion list) SHALL NOT end the Word, and the text control stays the Word's control (owner decision). Such a move that follows any other key or a click SHALL end the Word.

#### Scenario: Suggestion list opens while typing
- **WHEN** Space is a Trigger Key, the user types "ghbdtn" into an editor, its suggestion list takes focus after "g" while keys still reach the editor, and the user presses Space
- **THEN** the Word "ghbdtn" is evaluated, and focus coming back to the editor is not a focus change

#### Scenario: Navigation key, then a list
- **WHEN** the user types "ghb", presses Tab, and focus moves to a list row in the same window
- **THEN** the current Word is empty, and the keys typed next belong to the list row, not the editor

#### Scenario: Menu or another text control
- **WHEN** the user types "ghb" and focus moves to a menu item or to another text control in the same window
- **THEN** the current Word is empty

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
