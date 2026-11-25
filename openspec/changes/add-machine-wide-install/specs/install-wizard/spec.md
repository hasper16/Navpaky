## MODIFIED Requirements

### Requirement: Branded install wizard
Navpaky SHALL be installed through its own wizard, `NavpakySetup.exe`, drawn in the Navpaky look in the light or dark theme Windows uses. Its pages are Language, Licence, Welcome back (only when kept data is found), Folder, Options, Installing and Done, in that order, with a step counter. The wizard SHALL start without administrator rights. It SHALL ask for elevation only when the user chose "For all users", and then once, when the install starts.

#### Scenario: Navigation
- **WHEN** the user is on any page before Installing
- **THEN** it offers Back (except the first page) and Next, Options offers Install instead of Next, and Done offers Finish

#### Scenario: New install
- **WHEN** a user with no Navpaky data runs `NavpakySetup.exe`
- **THEN** the wizard shows Language, Licence, Folder, Options, Installing and Done, in that order, with no Welcome back page

#### Scenario: Standard user
- **WHEN** a user without administrator rights runs the wizard and finishes it with "Just for me"
- **THEN** no UAC prompt appears and Navpaky is installed for that user

## ADDED Requirements

### Requirement: Install mode
The Folder page SHALL offer two modes: "Just for me", selected by default, and "For all users". "Just for me" keeps the per-user folder rules. "For all users" SHALL say that:
- Windows asks for an administrator's approval when the install starts, and an account that isn't an administrator needs an administrator's password;
- every update asks for that approval again;
- Navpaky goes into `%ProgramFiles%\Navpaky` for every account on the PC, and each user keeps their own settings.

#### Scenario: Default mode
- **WHEN** a user with no Navpaky install reaches the Folder page
- **THEN** "Just for me" is selected and the proposed folder is `%LOCALAPPDATA%\Programs\Navpaky`

#### Scenario: All users chosen
- **WHEN** the user selects "For all users"
- **THEN** the page shows the administrator warning and the folder `%ProgramFiles%\Navpaky`, the folder cannot be changed, and no UAC prompt has appeared yet

#### Scenario: Back to just me
- **WHEN** the user selects "For all users" and then "Just for me"
- **THEN** the page shows the per-user folder again, without the warning

### Requirement: Folder rules for all users
For all users, the target SHALL be `%ProgramFiles%\Navpaky`. The wizard SHALL still refuse it when it is not empty and holds no Navpaky install, and SHALL show the space needed and free, as for "Just for me". It SHALL NOT refuse it for being unwritable to the user: the setup runs with administrator rights.

#### Scenario: Program Files folder
- **WHEN** the user selects "For all users" and `%ProgramFiles%\Navpaky` does not exist
- **THEN** Next is enabled when the drive has the space the install needs

#### Scenario: Foreign files in the Program Files folder
- **WHEN** `%ProgramFiles%\Navpaky` holds files that are not a Navpaky install
- **THEN** Next is disabled and the page says the folder is not empty

### Requirement: An existing install keeps its mode
When Navpaky is installed for the current user, "For all users" SHALL be unavailable and the page SHALL say that Navpaky is already installed for this account. When Navpaky is installed for all users, "For all users" SHALL be selected and "Just for me" unavailable, and the page SHALL say that Navpaky is installed for all users. Two installs of Navpaky on one account would both start and both update.

#### Scenario: Per-user install exists
- **WHEN** Navpaky is installed for the user in `%LOCALAPPDATA%\Programs\Navpaky` and the user runs the wizard
- **THEN** "Just for me" is selected, "For all users" is disabled, and the page says why

#### Scenario: All-users install exists
- **WHEN** Navpaky is installed for all users and any user runs the wizard
- **THEN** "For all users" is selected, "Just for me" is disabled, and the folder is `%ProgramFiles%\Navpaky`

### Requirement: Elevation for an all-users install
For all users, pressing Install SHALL raise one UAC prompt for the Navpaky wizard. Everything that needs administrator rights SHALL run in that elevated wizard, with no window of its own. The elevated part SHALL NOT run, write or delete files at paths the unelevated user chose or can change. When the prompt is declined or times out, the wizard SHALL report the install as failed, change nothing, and offer Retry and Close. The user's own choices (interface language, sign-in, desktop shortcut, keep or start fresh) SHALL apply to the user who ran the wizard, also when an administrator approved with their own password.

#### Scenario: Approved
- **WHEN** an administrator selects "For all users", presses Install and approves the UAC prompt
- **THEN** Navpaky is installed in `%ProgramFiles%\Navpaky` and the wizard shows Done

#### Scenario: Declined
- **WHEN** the user presses Install for all users and declines the UAC prompt
- **THEN** the wizard says the install failed and offers Retry and Close
- **AND** nothing is installed and kept data is unchanged

#### Scenario: Standard user with an administrator's password
- **WHEN** a standard user installs for all users and an administrator types their password at the prompt
- **THEN** Navpaky is installed for all users, and the standard user's interface language, sign-in and desktop shortcut follow the wizard's choices
