## ADDED Requirements

### Requirement: All-users installation
An all-users install SHALL put Navpaky in `%ProgramFiles%\Navpaky` with:
- one Installed apps entry for the whole PC, which reads "Navpaky" and shows the installed version;
- no Installed apps entry in the profile of the administrator who approved the install;
- one Start-menu shortcut shared by every user, and none per user.

Each user's settings and data SHALL stay in that user's own `%APPDATA%\Navpaky` and `%LOCALAPPDATA%\Navpaky`.

#### Scenario: Entries after an all-users install
- **WHEN** an administrator installs Navpaky for all users
- **THEN** Installed apps lists Navpaky once, for every user, with the installed version
- **AND** every user's Start menu shows one Navpaky shortcut that starts Navpaky

#### Scenario: Second user's data
- **WHEN** a second user signs in after an all-users install and changes a setting
- **THEN** the change is saved in that user's own `%APPDATA%\Navpaky` only

### Requirement: Sign-in launch for every user
When the all-users install was made with launch at sign-in on, every user's first sign-in after the install SHALL turn on that user's own launch at sign-in, once. A user, standard or not, SHALL be able to turn it off in Navpaky's settings or in Windows startup apps without an administrator, and an update SHALL NOT turn it on again. The user who ran the wizard SHALL get the choice made in the wizard. With launch at sign-in off in the wizard, no user's launch at sign-in is turned on.

#### Scenario: Second user signs in
- **WHEN** a user who has never signed in since the all-users install signs in
- **THEN** Navpaky starts and shows its Welcome window

#### Scenario: Standard user turns it off
- **WHEN** a standard user turns off "Start Navpaky when I sign in to Windows"
- **THEN** no administrator prompt appears, and Navpaky does not start at that user's next sign-in, also after an update

#### Scenario: Off in the wizard
- **WHEN** the all-users install was made with launch at sign-in off and a second user signs in
- **THEN** Navpaky does not start

### Requirement: Updates of an all-users install
An all-users install SHALL apply an update only when a user chooses "Restart to update". A downloaded update SHALL NOT be applied, nor an approval asked for, when Navpaky starts or a user signs in. Applying an update SHALL first close Navpaky in every session, after it saved that user's settings, word lists and statistics, so a second user's running Navpaky does not make the update fail. After the update, the Installed apps entry SHALL show the new version, and the approving administrator's profile SHALL hold no Installed apps entry of its own. When the approval is declined or times out, Navpaky SHALL keep running the current version and keep offering the update.

#### Scenario: Pending update at start
- **WHEN** an update was downloaded but not applied, and Navpaky starts at sign-in
- **THEN** no UAC or credential prompt appears and Navpaky runs the current version

#### Scenario: Another user runs Navpaky
- **WHEN** an administrator chooses "Restart to update" and approves while another signed-in user has Navpaky running
- **THEN** the other user's Navpaky closes after saving their data, the update is applied, and the administrator's Navpaky restarts in the new version

#### Scenario: Entry after an update
- **WHEN** an update of an all-users install is applied
- **THEN** Installed apps lists Navpaky once, with the new version

#### Scenario: Approval declined
- **WHEN** the user chooses "Restart to update" and declines the prompt
- **THEN** Navpaky runs the current version again and still offers "Restart to update"

### Requirement: Uninstall of an all-users install
The Installed apps entry of an all-users install SHALL start an uninstall that asks for an administrator's approval through a prompt that names Navpaky. The uninstall SHALL remove the app files, the Installed apps entry, the shared Start-menu shortcut and the sign-in setup for new users, and SHALL close Navpaky in every session first. It SHALL ask the data question for the account that approved the uninstall. Other users' data and sign-in entries SHALL stay. A sign-in entry left in another user's profile points to a removed program, and Windows skips it.

#### Scenario: Uninstall from Installed apps
- **WHEN** an administrator uninstalls Navpaky from Installed apps and approves the prompt
- **THEN** `%ProgramFiles%\Navpaky`, the Installed apps entry and the shared Start-menu shortcut are gone
- **AND** a user who signs in for the first time afterwards gets no Navpaky sign-in entry

#### Scenario: Prompt declined
- **WHEN** the uninstall prompt is declined
- **THEN** Navpaky stays installed and listed, and no file is removed
