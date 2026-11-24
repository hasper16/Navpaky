## ADDED Requirements

### Requirement: Branded install wizard
Navpaky SHALL be installed through its own wizard, `NavpakySetup.exe`, drawn in the Navpaky look in the light or dark theme Windows uses. Its pages are Language, Licence, Welcome back (only when kept data is found), Folder, Options, Installing and Done, in that order, with a step counter. The wizard SHALL run without administrator rights and SHALL NOT ask for elevation.

#### Scenario: Navigation
- **WHEN** the user is on any page before Installing
- **THEN** it offers Back (except the first page) and Next, Options offers Install instead of Next, and Done offers Finish

#### Scenario: New install
- **WHEN** a user with no Navpaky data runs `NavpakySetup.exe`
- **THEN** the wizard shows Language, Licence, Folder, Options, Installing and Done, in that order, with no Welcome back page

#### Scenario: Standard user
- **WHEN** a user without administrator rights runs the wizard and finishes it
- **THEN** no UAC prompt appears and Navpaky is installed for that user

### Requirement: Wizard language
The Language page SHALL offer "Same as Windows", English and Українська. The choice SHALL switch the wizard's texts at once, without restarting it, and SHALL become the app's interface language. "Same as Windows" SHALL be selected by default.

#### Scenario: Switch to English
- **WHEN** the wizard starts on a Ukrainian Windows and the user picks English on the first page
- **THEN** every text of the wizard is in English from then on
- **AND** after the install Navpaky's interface language is English

#### Scenario: Keep the default
- **WHEN** the user leaves "Same as Windows" selected
- **THEN** Navpaky's interface language setting after the install is "Same as Windows"

### Requirement: Licence acceptance
The Licence page SHALL show the licence text in a scrollable box and an "I accept the licence" check box, off by default. Next SHALL be disabled until the box is checked.

#### Scenario: Not accepted
- **WHEN** the user has not checked "I accept the licence"
- **THEN** Next on the Licence page is disabled

### Requirement: Install folder for the current user
The Folder page SHALL propose a folder and let the user pick another one through a folder picker. It proposes the folder kept settings remember from an earlier, since uninstalled install, and otherwise `%LOCALAPPDATA%\Programs\Navpaky` (design decision). The page SHALL say that Navpaky installs for the current account only, needs no administrator rights and updates quietly, and SHALL show the space needed and the space free on the chosen drive.

#### Scenario: Default folder
- **WHEN** a user with no earlier install reaches the Folder page
- **THEN** the proposed folder is `%LOCALAPPDATA%\Programs\Navpaky`

#### Scenario: Remembered folder after uninstall
- **WHEN** an earlier install in `D:\Apps\Navpaky` was uninstalled with its data kept, and the user runs the wizard
- **THEN** the proposed folder is `D:\Apps\Navpaky` and the user can pick another one

#### Scenario: Not enough space
- **WHEN** the chosen drive has less free space than the install needs
- **THEN** Next is disabled and the page says how much space is needed

### Requirement: Install into a Navpaky folder
When Navpaky is not installed for the user, the wizard SHALL install only into a folder named `Navpaky`. When the user picks a folder with any other name, the wizard SHALL use a `Navpaky` subfolder of it and show the resulting path. An existing install's folder SHALL be used exactly as it is, whatever its name.

#### Scenario: Folder with another name
- **WHEN** the user picks `D:\Tools`
- **THEN** the wizard shows and uses `D:\Tools\Navpaky`

### Requirement: Refuse unsafe folders
The wizard SHALL refuse a target folder that exists, is not empty and does not hold a Navpaky install. Velopack's silent setup deletes the existing contents of its target (observed in the 2026-11-24 live run). The wizard SHALL also refuse a folder the user cannot write to, and a folder that is, lies inside, or contains either of Navpaky's data folders (`%APPDATA%\Navpaky`, `%LOCALAPPDATA%\Navpaky`).

#### Scenario: Non-empty folder
- **WHEN** the resulting folder exists and holds files that are not a Navpaky install
- **THEN** Next is disabled and the page says the folder is not empty
- **AND** none of those files are changed

#### Scenario: Folder overlaps the data folder
- **WHEN** the user picks `%LOCALAPPDATA%\Navpaky\Tools`
- **THEN** Next is disabled and the page says the folder is Navpaky's data folder

#### Scenario: Folder not writable
- **WHEN** the user picks a folder under `C:\Program Files`
- **THEN** Next is disabled and the page says the folder needs administrator rights

### Requirement: Reuse the existing install folder
When Navpaky is already installed for the user, the wizard SHALL install into that same folder. Installing into a second folder leaves the first copy orphaned and takes over its Installed apps entry (observed in the 2026-11-24 live run).

#### Scenario: Existing install
- **WHEN** Navpaky is installed for the user in `%LOCALAPPDATA%\navpaky-app` and the user runs the wizard
- **THEN** the Folder page shows that folder, the folder cannot be changed, and the page says why

### Requirement: Install options
The Options page SHALL offer:
- "Start Navpaky when I sign in to Windows", on by default;
- "Create a desktop shortcut", off by default (owner decision). The hint says Navpaky is always in the Start menu and the tray.

After "Use previous settings", both SHALL be preset from the kept data and show what they were before.

#### Scenario: Defaults
- **WHEN** a new user reaches the Options page
- **THEN** launch at sign-in is on and the desktop shortcut is off

#### Scenario: Shortcut chosen
- **WHEN** the user turns the desktop shortcut on and finishes the install
- **THEN** the user's desktop has a Navpaky shortcut that starts Navpaky

#### Scenario: Sign-in applied without starting
- **WHEN** the user keeps launch at sign-in on and unchecks "Start Navpaky now" on the Done page
- **THEN** Navpaky starts at the user's next sign-in

### Requirement: Choices reach the app
The wizard's choices SHALL take effect even when Navpaky is not started after the install:
- the interface language;
- launch at sign-in;
- the desktop shortcut;
- keep or start fresh.

They SHALL be applied when the install completes, before the wizard reports Done. The choices contain no typed text.

#### Scenario: Not started after install
- **WHEN** the user unchecks "Start Navpaky now" and finishes
- **THEN** the chosen interface language, launch at sign-in and desktop shortcut are already in effect

### Requirement: Welcome back
When the wizard finds Navpaky data kept from an earlier install, it SHALL show a Welcome back page after the Licence. The page offers:
- "Use previous settings", selected by default, listing what is kept (hotkey, learned words, apps, statistics);
- "Start fresh", which deletes all kept data, as on a first install.

#### Scenario: Kept data found
- **WHEN** an earlier uninstall kept the data and the user runs the wizard
- **THEN** the Welcome back page appears with "Use previous settings" selected

#### Scenario: Use previous settings
- **WHEN** the user keeps "Use previous settings" and finishes the install
- **THEN** settings, word lists and statistics are those of the earlier install
- **AND** Navpaky's first start shows no Welcome window

### Requirement: Start fresh after a confirmation
Choosing "Start fresh" and pressing Next SHALL first ask for confirmation. The confirmation names what is deleted (settings, learned and protected words, the app list, statistics, logs) and says it cannot be undone. It offers Cancel, which returns to the choice, and "Delete and continue".

Navpaky's first start after Start fresh SHALL show the Welcome window.

#### Scenario: Cancel
- **WHEN** the user presses Cancel on the confirmation
- **THEN** the Welcome back page shows the choice again and nothing is deleted

#### Scenario: Confirmed
- **WHEN** the user confirms Start fresh and the install succeeds
- **THEN** no settings, word lists, statistics or logs of the earlier install remain
- **AND** Navpaky's first start opens the Welcome window at step 1

### Requirement: Start fresh deletes only after a complete install
After Start fresh, the kept data SHALL be deleted only after the whole install has completed, including its Installed apps entry and the application of the wizard's choices. When the install fails at any point, the kept data SHALL be as it was before.

#### Scenario: Install fails after confirmation
- **WHEN** the user confirms Start fresh and the install fails
- **THEN** the kept data is still there

### Requirement: Install progress and errors
The Installing page SHALL show progress while the files are copied, and SHALL NOT allow going back. The install SHALL count as successful only when the setup succeeded and every choice was applied. When the install fails, the wizard SHALL say so in the chosen language, keep the earlier state, and offer to retry or close. Velopack's own windows SHALL NOT appear during the install.

#### Scenario: Setup fails
- **WHEN** the embedded setup ends with an error
- **THEN** the wizard shows that the install failed and offers Retry and Close
- **AND** the earlier install, if any, still works

### Requirement: Retry only after the setup stopped
The wizard SHALL offer Retry only once the failed setup is known to have stopped. Otherwise it SHALL offer only Close and say that a setup may still be running, so two setups never write the same folder.

#### Scenario: Setup does not finish
- **WHEN** the embedded setup is still running after the time limit
- **THEN** the wizard stops it and offers Retry only after it has stopped
- **AND** no second setup starts while the first one may still be running

#### Scenario: Choices not applied
- **WHEN** the setup reports success but the choices were not all applied
- **THEN** the wizard reports the install as failed and offers Retry and Close

### Requirement: Done page
The Done page SHALL say that Navpaky is installed and where to find it (tray and Start menu), and SHALL offer "Start Navpaky now", on by default. Finish SHALL close the wizard and start Navpaky when that box is checked.

#### Scenario: Finish
- **WHEN** the user presses Finish with "Start Navpaky now" checked
- **THEN** Navpaky is running and the wizard has closed

### Requirement: One signed download
Each release SHALL offer `NavpakySetup.exe` as the single installer download, with Velopack's setup inside it. The wizard and the setup inside it SHALL carry a valid Authenticode signature from the Navpaky publisher. A release whose wizard is unsigned or signed by another publisher MUST NOT be published.

#### Scenario: Release assets
- **WHEN** a version tag is released
- **THEN** the GitHub release lists `NavpakySetup.exe` next to the update packages
- **AND** `NavpakySetup.exe` and the setup it runs are signed by the Navpaky publisher
