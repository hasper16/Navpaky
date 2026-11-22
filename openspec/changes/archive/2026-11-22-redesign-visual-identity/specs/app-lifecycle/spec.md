## ADDED Requirements

### Requirement: Launch splash
On every start, including the start at sign-in (owner decision), Navpaky SHALL show a splash centred on the primary screen for about three seconds (design decision): a small card where a caret types "Navpaky" marked as wrong, the word turns into the greeting in the UI language ("Привіт" for Ukrainian, "Hello" for English) marked as corrected, a pill with the Language code appears in the card's corner, and the card fades out.

#### Scenario: Ukrainian interface
- **WHEN** the UI language is Ukrainian and Navpaky starts
- **THEN** the splash types "Navpaky", which turns into "Привіт" with a "UK" pill, and fades out after about three seconds

### Requirement: Launch splash stays out of the way
The splash SHALL NOT take focus, SHALL let clicks pass through, SHALL have no taskbar button and SHALL NOT delay the tray icon or Auto Correction. When Windows animations are turned off, it SHALL show the greeting without motion and then disappear.

#### Scenario: Typing during the splash
- **WHEN** the user is typing in another window while the splash is shown
- **THEN** the keystrokes reach that window and the splash does not take focus

#### Scenario: Animations off
- **WHEN** Windows animation effects are off and Navpaky starts
- **THEN** the splash shows "Hello" or "Привіт" without the typing motion and disappears

## MODIFIED Requirements

### Requirement: Background start without a main window
The app SHALL start without opening a main window or a taskbar button and SHALL show a notification-area (tray) icon while running. The launch splash is not a main window; the welcome window opens on the first start only (onboarding).

#### Scenario: Normal start
- **WHEN** the user launches Navpaky after the first start
- **THEN** a Navpaky icon appears in the notification area
- **AND** no window other than the launch splash and no taskbar button are shown
