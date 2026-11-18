# Settings window: keyboard, focus, Narrator and theme

Task 3.5 of `add-tray-and-settings-ui`. Deferred to the final acceptance pass (owner decision 2026-11-14). Use a Debug build of `src/Navpaky.App`, open Settings from the tray, and go through every section: General, Correction, Hotkey, Apps and words, Diagnostics and privacy, About. Record each result in the last column (pass, or what failed).

| Item | Light mode | Dark mode |
|---|---|---|
| The section list takes focus with Tab and changes section with the arrow keys | | |
| In every section, Tab and Shift+Tab reach every control in reading order; nothing traps focus | | |
| Space or Enter changes each switch, check box, radio button and button; arrow keys move within the radio groups and the sound and language lists | | |
| The Hotkey recorder leaves with Tab or Escape; a recorded combination appears in the box | | |
| Focus is always visible: a focus ring on the control that has it | | |
| Narrator reads the section names, every switch, check box and radio button with its label and state, the sound and language lists by their labels, the "Remove …" buttons with the item they remove, and the window's Minimize, Maximize and Close buttons | | |
| The window, the tray menu and the cursor indicator use dark colors when Windows uses dark mode, and light ones in light mode; switching the Windows mode while the window is open repaints it | | |
| Text and controls stay readable at 150 % and 200 % display scale | | |
