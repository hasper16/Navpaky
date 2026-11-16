# Proposal

## Why

Rekey's main audience types in Ukrainian and Russian as well as English, and many of them use Windows in those languages. The original ships its interface in about 40 languages. For the MVP we need the interface in English and Ukrainian, with the same plural and wording quality as the source text. By the owner's decision the interface has no Russian: users on Russian Windows get the Ukrainian interface.

## What Changes

- All UI text (tray, menus, windows, notifications, installer prompts) moved to resources.
- Interface languages: English and Ukrainian; by default Ukrainian when the Windows display language is Ukrainian or Russian, English otherwise.
- User override of the interface language in settings (System, English, Ukrainian), applied without restart.
- Correct plural forms for Ukrainian ("1 хвилина", "2 хвилини", "5 хвилин").
- A check in CI that every language has every string.
- Scope is the interface only: Russian keyboard-layout detection, Language Profiles and correction behavior stay as they are.

## Capabilities

### New Capabilities

- `localization`: Which interface languages Rekey offers, how the language is chosen, and the completeness and grammar rules for translated text.

### Modified Capabilities

None.

## Impact

- `Rekey.App`: `.resx` resources (`Strings.resx`, `Strings.uk.resx`), a localization service with change notification for bindings.
- Settings: `UiLanguage` (`system` default).
- CI: resource completeness test.
- Touches every UI change; scheduled last so all strings exist, but earlier UI changes already put strings in `Strings.resx`.
- Depends on `add-tray-and-settings-ui`, `add-statistics-and-diagnostics`, `add-auto-update-and-installer`, `add-onboarding` and `add-telemetry`.
