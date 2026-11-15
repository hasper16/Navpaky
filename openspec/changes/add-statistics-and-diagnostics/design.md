# Design

## Context

The original has a statistics window (words analysed, switches, reverted switches, other undos, a decision breakdown, replacement durations), a statistics log, an incident log, a list of recent word analyses, and a diagnostics export to the clipboard or a file. In its pre-release build the word-analysis log is on by default; we make it opt-in because it is effectively a record of typed text.

## Goals / Non-Goals

**Goals:**
- Useful numbers with zero text in default storage.
- A diagnostics export users can read before sharing.

**Non-Goals:**
- Sending anything over the network (`add-telemetry`).

## Decisions

### D1. Trust levels of local data
| Data | Contains text? | Default | Leaves the machine? |
|---|---|---|---|
| Counters (`stats.json`) | No | On | Only as Usage Digest, if user opts in |
| Incidents (`incidents.jsonl`) | No (types, codes, app exe names) | On | Only in a user-confirmed export |
| Word-analysis log (`analysis.log`) | **Yes** | Off | Only if the user includes it in an export |
| Settings, word lists | Protected/Learned Words | — | Never in exports by default |

### D2. Counters
Engine emits `RecordStat` actions; an aggregator on the worker keeps today's bucket in memory and flushes every 60 s and on exit. 30 daily buckets; durations stored as a fixed histogram (1 ms buckets up to 200 ms) for median without keeping samples.

### D3. Incident log
JSON lines, append-only, rotated at 500 entries. Fields: `time`, `type`, `details` (enum-coded values, exe name, error codes). No window titles (they often contain document names, emails or chat partners).

### D4. Word-analysis log protection
- File created with a DACL granting only the current user SID and SYSTEM (`FileSecurity`), inheritance disabled.
- Daily file rotation; files older than 7 days deleted on start and at midnight.
- Never written for Password Fields (no Keystrokes exist there anyway) or elevated windows.

### D5. Export
Builder produces plain text sections; masking replaces `%USERPROFILE%`, user name, and machine name. The preview is the exact string that will be copied/saved. "Save to file" defaults to Desktop with a timestamped name.

## Risks / Trade-offs

- [Users enable the analysis log and forget it] → settings page shows its size and a "Turn off and delete" button; retention is 7 days.
- [Clipboard export of diagnostics is visible to clipboard-history tools] → export defaults to "Save to file"; clipboard is a secondary button. (This is the only place Rekey writes the clipboard, and only on explicit user action.)

## Revisions during implementation

- **R1. Decision breakdown.** "Decisions by reason" counts each evaluated Word once by how it ended: corrected, deferred, kept, or kept because it is a Protected Word. These are the reasons the engine has; the detector's internal scores are not counted.
- **R2. Replacement duration.** The engine measures it from the key that asked for the Correction (the Trigger Key or the Hotkey) to the end of the replacement, from the timestamps it already receives; cancelled Corrections are not counted.
- **R3. Ownership.** The worker owns the counters (`StatsRecorder`) and the word-analysis log (`AnalysisLog`), so each has one writer. The UI thread saves the latest counters snapshot. Incidents arrive from the worker, the freeze guard and start-up, so one background task (`QueuedIncidentLog`) writes them.
- **R4. Log files.** The word-analysis log is one file per local day, `analysis\analysis-YYYY-MM-DD.log`; a day is deleted once it is 7 days old, on start and when the date changes. Lines hold the time, the Layout and the Word, the Renderings in the other installed Layouts and the Decision.
- **R5. Turning the log off.** Turning the switch off deletes the log at once, so the separate "Turn off and delete" button and the size display are not needed.
- **R6. Export entry points.** The tray menu opens the statistics window; the Diagnostics and privacy page has the statistics and export buttons. Export reads its facts once when the dialog opens, so the preview does not change while the user reads it.
