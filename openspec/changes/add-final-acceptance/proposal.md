# Proposal

## Why

Until change 7, every change waited before merge for the owner to run its by-hand checklist at a Windows desktop. That made the owner a gate on every merge and slowed the roadmap. The owner decided (2026-11-14) to defer all by-hand checks to one pass at the end: implementers collect them in `docs/manual-tests/deferred-by-hand.md`, one section per change, and this change is that pass, together with the fixes for whatever fails.

## What Changes

- The owner runs every unchecked item in `docs/manual-tests/deferred-by-hand.md` on one commit of `main` that contains all other roadmap changes, at a Windows desktop.
- Each failure is recorded as a finding under its item (steps, expected, observed; no typed text beyond the invented Words of the checklist) and fixed in this change: a code fix with a failing test first, or a spec revision through `/opsx:update` when the spec itself was wrong.
- Items that pass are ticked in `deferred-by-hand.md`; the by-hand tasks the earlier changes left unticked are ticked in their archived `tasks.md` with a pointer to the run.
- No new behavior is planned. The fixes that change required behavior carry delta specs (see Modified Capabilities).

## Capabilities

### New Capabilities

None.

### Modified Capabilities

Added after the owner's pass (task 1.3) through `/opsx:update`:

- `text-replacement`: keys typed between the parts of a multi-part replacement land after it (owner decision).
- `tray-and-settings`: an Excluded App chosen as a file also matches a running Store app with the same file name (the Windows 11 Notepad stub).
- `auto-update`: the uninstaller waits for the data question before it reports completion, and keeps the data when Rekey was installed again meanwhile.
- `app-lifecycle`: a second start with arguments tells the user Rekey is already running.
- `onboarding`: the tray item that reopens the welcome window is named "Getting started" ("Знайомство з Rekey"), apart from Settings.

## Impact

- Docs: `docs/manual-tests/deferred-by-hand.md` (ticks, findings, the commit each run was on); archived `tasks.md` of earlier changes (ticks of deferred by-hand tasks).
- Code: unknown until the pass; any module may receive fixes.
- Depends on every other change in `docs/roadmap.md` (1–14); it is the last entry and the release gate.
