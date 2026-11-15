# Design

## Context

The original opens a welcome window on first run with a short explanation and a try-it step, asks for macOS permissions (none are needed on Windows), warns when it runs from a disk image or a temporary copy, and notices when its app file is moved or deleted while it runs. It also delays keyboard capture until the welcome flow ends.

## Goals / Non-Goals

**Goals:**
- A user understands and trusts Rekey within one minute.
- The try-it field exercises the real pipeline, not a simulation.

**Non-Goals:**
- Privacy and diagnostics consent content (`add-telemetry` adds it to this window).
- Installer (`add-auto-update-and-installer`).

## Decisions

### D1. One window, one scrolling page
Not a multi-step wizard: content is short, and a page lets users skim. Order: keycap mark + one-sentence description → privacy statement → try-it field → choices → tray pinning tip → Done. The try-it field is a normal WPF `TextBox`; Rekey's hook sees it like any other app, so success proves the whole pipeline. Rekey's own process is never an Excluded App for this reason.

### D2. Success detection
The worker already reports every Layout Switch it makes as `SwitchFeedback`; one with cause `Correction` while the welcome window is active and the try-it box has keyboard focus makes the view-model show the confirmation (revision R1).

### D3. Hook start
Keyboard capture starts immediately on first run (Windows needs no permission), unlike the original which waits for macOS permissions.

### D4. Location checks
- Installed location = Velopack's install root (`%LOCALAPPDATA%\key-switcher`, see `add-auto-update-and-installer` D1), read from `VelopackLocator` rather than hard-coded.
- Install option: runs the bundled Velopack setup if the copy contains one, otherwise opens the project's GitHub Releases page.
- Warn when the process path is under `%TEMP%`, `%LOCALAPPDATA%\Temp`, a path containing a `.zip` segment, or the user's Downloads folder (`SHGetKnownFolderPath(FOLDERID_Downloads)`). Dev builds (Debug configuration) skip the warning.
- File disappearance: `FileSystemWatcher` on the executable's folder plus a 60 s existence check fallback.

### D5. Tray pinning tip
Windows offers no API to pin a tray icon; show text naming the path: Settings → Personalization → Taskbar → Other system tray icons → Rekey (revision R2).

## Risks / Trade-offs

- [Users dismiss the window without reading] → it can be reopened from the tray; the first Correction also shows a one-time toast "Rekey corrected a word. Press Ctrl+Z to undo."

## Revisions

- **R1. No new engine event.** The design first named a `CorrectionApplied` engine event. The worker's existing `Switched` event already carries `SwitchFeedback` with cause `Correction` after each applied Correction, and only Rekey's own window can hold the try-it box's focus, so the app checks focus instead of comparing app ids. The engine stays unchanged.
- **R2. Tray tip is text only.** An illustration of the Windows Settings page would go stale with each Windows release; the path in words stays correct and is read by screen readers.
- **R3. The one-time toast is remembered.** `Settings.CorrectionTipShown` records that the toast after the first real Correction was shown, so it appears once per user, not once per session. A Correction in the try-it field does not count. The toast is a tray notification (`TaskbarIcon.ShowNotification`).
- **R4. First start defaults.** While `WelcomeCompleted` is false, start-up turns launch at sign-in on through the settings hub, so Windows gets the choice even when the user closes the welcome window without touching it. Closing the window in any way sets `WelcomeCompleted`.
- **R5. Language settings link.** The single-Layout notice opens `ms-settings:regionlanguage` through `explorer.exe` by its full path. "One Layout" means one language: two Layouts of the same language cannot be corrected between.
