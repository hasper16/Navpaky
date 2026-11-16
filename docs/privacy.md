# What Rekey sends

Rekey sees every keystroke, so nothing leaves the computer unless the user agreed, and what leaves is limited to the fields below (spec `telemetry`). Typed text, Words, Renderings, Protected and Learned Words, clipboard content, window titles, the Windows user name, file paths inside the user profile and memory dumps are never sent.

Consent lives in `DiagnosticsConsent`: both parts are off by default. The welcome window's one checkbox sets both; Settings → Diagnostics and privacy sets them separately. Turning a part off stops it at once and drops what was not sent yet. The last 10 requests are kept in `sent.jsonl` in the data folder and shown under "Show what was sent" exactly as sent; the email sign-up is never kept.

The endpoints are fixed in `src/Rekey.App/Telemetry/TelemetryEndpoints.cs`, HTTPS only with standard certificate validation. Until the backend exists they are unset and Rekey sends nothing.

## Crash and freeze reports

Consent: `CrashReports`. Sent through Sentry for unhandled exceptions and for freezes of the UI or engine thread longer than 2 seconds. Each report is rebuilt from this list before it is sent (`ReportAllowlist`):

| Field | Content |
|---|---|
| Exception type | .NET type name, or `Rekey.Freeze.Ui` / `Rekey.Freeze.Engine` for a freeze |
| Mechanism | how it was caught (type, handled) |
| Stack frames | module (assembly name), function, line, whether it is Rekey's code; no local variables, no file paths |
| Release | Rekey version |
| OS | "Windows" and its version |
| `incidents` tag | Incident types of the last 10 minutes, such as `InputFrozen` |
| Level, event id, time | set by Sentry |

Exception messages, user, request, extra data, breadcrumbs, device and server names, loaded modules and attachments are dropped. Freeze stacks are read from an in-memory snapshot of the process; the snapshot is never written or sent. "Show what was sent" lists a report when it is handed to Sentry, in the form above; Sentry keeps no offline copy, so a report that fails to arrive is dropped but still listed.

## Usage Digest

Consent: `UsageDigest`. At most once a day, an hour or so after Rekey starts, covering the day before. A digest that could not be sent is not retried; the next day sends that day's counters.

| Field | Content |
|---|---|
| `schemaVersion` | 1 |
| `requestId` | random UUID of this request only, for server de-duplication; it identifies nothing across days |
| `day` | the local date the counters cover |
| `environment` | see below |
| `settings` | see below |
| `counters` | `analysed`, `autoCorrections`, `manualCorrections`, `undos`, `correctDecisions`, `deferDecisions`, `keepDecisions`, `protectedDecisions`: counts of that day |

`environment`: `appVersion` (Rekey version), `osVersion` (Windows version), `uiLanguage` (UI culture name such as en-US), `layoutLanguages` (two-letter codes of the installed Layouts' languages).

`settings` (on/off): `autoCorrection`, `launchAtSignIn`, `cursorIndicator`, `switchSound`, `statistics`, `autoCheckForUpdates`.

## Setup signal

Consent: `UsageDigest`. Once, when the welcome window closes.

| Field | Content |
|---|---|
| `schemaVersion`, `requestId`, `environment`, `settings` | as in the Usage Digest |
| `layoutCount` | number of installed Layouts |
| `coveredLanguageCount` | number of their languages Rekey has a profile for |
| `progress` | `triedCorrection` (a Correction happened in the try-it field), `closedWithDone` (closed with Done rather than the window's X) |

## Email sign-up

Sent only when the user presses Sign up, on its own request and never linked to a report. Rekey forgets the address once the server accepted it.

| Field | Content |
|---|---|
| `schemaVersion` | 1 |
| `email` | the address the user entered |
| `newVersions` | the user wants news of new versions |
| `reportedProblems` | the user wants news about problems they reported |

## Review checklist for changes to telemetry

- A new field goes into a record in `src/Rekey.Core/Telemetry/Schemas` and into this document; the schema and document tests fail until both agree.
- No field name or value may carry text the user typed or saw: the schema test rejects names like text, word, title, path, user, name, window.
- No identifier may outlive one request.
- A new crash-report field is added to `ReportAllowlist` on purpose, never by passing Sentry's event through.
- The change gets a `security-audit` review (docs/agents/orchestration.md).
