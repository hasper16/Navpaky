# Spec Delta

## MODIFIED Requirements

### Requirement: Interface languages
Navpaky's interface SHALL be available in English and Ukrainian, covering the tray icon tooltip and menu, all windows, notifications and uninstall prompts. The interface SHALL NOT offer Russian; Russian layout detection and correction are not affected by this requirement.

#### Scenario: Ukrainian interface
- **WHEN** the interface language is Ukrainian
- **THEN** the tray menu, settings, statistics and welcome windows show Ukrainian text

### Requirement: Language follows Windows by default
By default the interface language SHALL be Ukrainian when the Windows display language is Ukrainian or Russian, and SHALL be English otherwise.

#### Scenario: Ukrainian Windows
- **WHEN** Windows display language is Ukrainian and the user has not chosen a language
- **THEN** Navpaky's interface is Ukrainian

#### Scenario: Russian Windows
- **WHEN** Windows display language is Russian and the user has not chosen a language
- **THEN** Navpaky's interface is Ukrainian

#### Scenario: German Windows
- **WHEN** Windows display language is German and the user has not chosen a language
- **THEN** Navpaky's interface is English

### Requirement: User override without restart
The user SHALL be able to choose the interface language in settings from System, English and Ukrainian, and the change SHALL apply to all open windows and the tray menu without restarting Navpaky.

#### Scenario: Switch to English
- **WHEN** the interface is Ukrainian and the user selects English in settings
- **THEN** the settings window and tray menu show English immediately
