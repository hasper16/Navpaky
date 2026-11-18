# Spec Delta

## MODIFIED Requirements

### Requirement: Bundled Language Profiles
The app SHALL ship a Language Profile for English, Russian and Ukrainian and load them at start-up without network access.

#### Scenario: Start offline
- **WHEN** Navpaky starts on a machine with no network connection
- **THEN** the English, Russian and Ukrainian Language Profiles are loaded

### Requirement: Active Languages follow installed Layouts
A Language SHALL be active only while at least one of its Layouts is installed for the current user. Words SHALL only be evaluated between active Languages.

#### Scenario: Ukrainian layout not installed
- **WHEN** the user has only English (US) and Russian Layouts installed
- **THEN** English and Russian are active and Ukrainian is inactive

#### Scenario: Layout added while running
- **WHEN** the user installs a Ukrainian Layout while Navpaky is running
- **THEN** Ukrainian becomes active without restarting Navpaky

### Requirement: Layouts without a Language Profile are left alone
A Layout not listed by any Language Profile SHALL NOT be a source or target of a Correction.

#### Scenario: Unsupported layout
- **WHEN** the active Layout is German and German has no Language Profile
- **THEN** Navpaky makes no Correction from or to the German Layout

### Requirement: Invalid profile does not stop the app
If a Language Profile cannot be read or fails validation, the app MUST record an Incident, skip that Language, and continue with the remaining Languages.

#### Scenario: Corrupt profile
- **WHEN** the user has English, Russian and Ukrainian Layouts installed
- **AND** the Ukrainian Language Profile is corrupt
- **THEN** Navpaky starts with English and Russian active
- **AND** an Incident names the Ukrainian Language Profile
