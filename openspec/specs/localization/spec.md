# localization Specification

## Purpose
Defines the interface languages Rekey offers, how the interface language is chosen, and the rules that keep translated text complete and grammatically correct.

## Requirements

### Requirement: Interface languages
Rekey's interface SHALL be available in English and Ukrainian, covering the tray icon tooltip and menu, all windows, notifications and uninstall prompts. The interface SHALL NOT offer Russian; Russian layout detection and correction are not affected by this requirement.

#### Scenario: Ukrainian interface
- **WHEN** the interface language is Ukrainian
- **THEN** the tray menu, settings, statistics and welcome windows show Ukrainian text

### Requirement: Language follows Windows by default
By default the interface language SHALL be Ukrainian when the Windows display language is Ukrainian or Russian, and SHALL be English otherwise.

#### Scenario: Ukrainian Windows
- **WHEN** Windows display language is Ukrainian and the user has not chosen a language
- **THEN** Rekey's interface is Ukrainian

#### Scenario: Russian Windows
- **WHEN** Windows display language is Russian and the user has not chosen a language
- **THEN** Rekey's interface is Ukrainian

#### Scenario: German Windows
- **WHEN** Windows display language is German and the user has not chosen a language
- **THEN** Rekey's interface is English

### Requirement: User override without restart
The user SHALL be able to choose the interface language in settings from System, English and Ukrainian, and the change SHALL apply to all open windows and the tray menu without restarting Rekey.

#### Scenario: Switch to English
- **WHEN** the interface is Ukrainian and the user selects English in settings
- **THEN** the settings window and tray menu show English immediately

### Requirement: Correct plural forms
Text with numbers SHALL use the plural rules of the interface language.

#### Scenario: Ukrainian minutes
- **WHEN** the interface is Ukrainian and a pause has 1, 2 or 5 minutes left
- **THEN** the tooltip says "1 хвилина", "2 хвилини" and "5 хвилин" respectively

### Requirement: Complete translations
Every interface string SHALL exist in both languages; a release build MUST fail when any string is missing.

#### Scenario: Missing string
- **WHEN** a new English string is added without a Ukrainian version
- **THEN** the CI build fails and names the missing key
