# Orchestrated implementation of OpenSpec changes

One **orchestrator** session drives the changes in `docs/roadmap.md` strictly in order, one at a time. For each change it starts one **implementer** session and keeps that same session for the whole life of the change: implementation, review fixes and archiving. Only the orchestrator merges into `main`.

Language: the orchestrator replies to and asks the user in Russian. Implementers think, report and reply in English, including to the user, because only the orchestrator's messages need to be in the user's language and English costs fewer tokens. Messages between sessions are in English.

## Orchestrator

The orchestrator coordinates; it does not author. It never edits files, commits or pushes. Its tools are session management (`start_session`, `SendMessage`, `list_sessions`), read-only `gh` / `git` queries, `gh pr merge`, and the desktop-check runs of step 4 from a detached worktree of the pinned commit (`git worktree add --detach`, removed after the run).

Loop, for each change in roadmap order:

1. **Start** one implementer: `start_session` with `use_worktree: true`, name `impl-<change>`, a brief in English naming the change, pointing to this document and stating that the implementer works and reports in English. Record the session id.
2. **Wait** for the implementer's report. The app wakes the orchestrator when the session ends a turn; do not poll.
3. **Check readiness** of the PR the implementer reports (all must hold):
   - every required check passes: `gh pr checks <N> --required`;
   - every review thread is resolved, and no reviewer's latest review is `CHANGES_REQUESTED` (check each reviewer, not only the PR-wide `reviewDecision`):
     ```bash
     gh api graphql --paginate -F n=<N> -f query='query($n:Int!,$endCursor:String){repository(owner:"hasper16",name:"key-switcher"){pullRequest(number:$n){mergeable headRefOid latestReviews(first:100){totalCount nodes{author{login} state}} reviewThreads(first:100,after:$endCursor){totalCount pageInfo{hasNextPage endCursor} nodes{isResolved path comments(first:1){nodes{author{login} body}}}}}}}'
     ```
     `--paginate` follows `reviewThreads` pages; if `latestReviews.totalCount` exceeds 100, page it the same way before deciding.
   - the PR is mergeable (no conflicts, branch up to date with `main`);
   - a sensitive PR carries a complete, fresh security review (see [Security review](#security-review)):
     1. **Scope**: `gh pr diff <N> --name-only` against the path table, plus the added lines of code files only (`*.cs`, `*.xaml`, `*.csproj`, `*.props`, `*.ps1`, `.github/workflows/*.yml`; Markdown and `openspec/**` are left out) in `gh pr diff <N>` scanned for `HttpClient`, `WebRequest`, `WebClient`, `Socket`, `TcpClient`, `NamedPipe`, `Process.Start`, and the client calls `SendAsync`, `PostAsync`, `GetStringAsync`, `GetStreamAsync`, `GetByteArrayAsync`. The list is not exhaustive: any added HTTP, socket, pipe or process call, including one through an existing client, makes the PR sensitive; otherwise the body says `Security review: not required`.
     2. **Content**: the `## Security review` section has all four template lines: `Skill: security-audit (guidance mode)`, a reviewed commit, `Areas:` naming every area step 1 matched, and `Findings:` (`none`, or each finding fixed or deferred with a reason).
     3. **Freshness**: `git merge-base --is-ancestor <reviewed-sha> <head>` holds, and no later commit touches a table path, adds a listed API in any code file, touches a file step 1 flagged as sensitive, or touches a helper a reviewed file relies on for a security decision (signature, path or input checks). Otherwise the review is redone.
   Anything open goes back to **the same implementer** via `SendMessage`, listing the unresolved threads or failing checks. Never fix it yourself and never hand it to another session.
4. **Desktop-check gate**: if the PR adds or changes an automated set (a `tools/Rekey.DesktopChecks` run), either its description in `docs/manual-tests/<change>.md` or its code under `tools/Rekey.DesktopChecks/`, record the current head SHA (`headRefOid`) first, then run that set on that commit on the owner's Windows desktop. The run needs the desktop untouched for a few minutes: ask the user (in Russian), naming the SHA, and start only after they confirm the desktop is free. Snapshot the input-language list before the run and check after it that the list is unchanged. Every check must pass; a failure goes back to the implementer as in step 3. A passing run covers only the recorded commit. When the head moves after it, first record the new head SHA, then use that same pinned SHA for everything that follows: if `git diff --name-only <passed-sha>..<new-sha>` lists anything outside `docs/` and `openspec/`, or changes which set or command `docs/manual-tests/<change>.md` documents, run the affected sets again on `<new-sha>`, asking the user first in the same way. Re-run step 3 for `<new-sha>` too; only then does it become the one to merge. By-hand checklists are not a gate: they are deferred to the `add-final-acceptance` change (see Implementer), so do not ask the user to run them before merge. The exception is `add-final-acceptance` itself: its by-hand pass is the change's own work (its tasks), and it starts only after every desktop-check set passes on the commit pinned in its task 1.1.
5. **Merge** the exact head you verified in step 3: `gh pr merge <N> --merge --match-head-commit <verified-sha>`. If GitHub refuses because the head moved, re-run steps 3–4 for the new head. Never `--admin`, never bypass protection.
6. **Archive**: tell the same implementer to run `/opsx:archive <change>` on branch `chore/archive-<change>`, open a PR, and report. Run steps 2–5 for that PR.
7. **Next**: only after the archive PR is merged, start the next change. The implementer session is left idle (the user archives sessions).

Escalate to the user (and pause the loop) when: a desktop-check run needs the desktop; an implementer raises a spec question; one PR needs more than 3 rounds of review fixes; CI fails for a reason the implementer cannot fix; a merge is refused by GitHub. Report progress to the user in one or two lines after each merge.

## Implementer

- Work only in your worktree, on branch `change/<change>` (archive: `chore/archive-<change>`).
- Think, report and reply in English, including end-of-turn summaries and answers to the user. Only the orchestrator writes to the user in Russian.
- Load `ub-mode`, run `/opsx:apply <change>`, follow `AGENTS.md`. Conventional Commits, no `Co-Authored-By` trailers.
- On start, and again after each rebase, run `graphify update .` in your worktree; locate code with `graphify query "<question>"`, `graphify path <a> <b>` and `graphify explain <symbol>` before broad searches. The graph is advisory: read the file before you edit it.
- Use the curated agent-skills where they fit: `doubt-driven-development` for the keyboard hook, input injection and the updater; `security-and-hardening` while writing update, telemetry and diagnostics code; `ci-cd-and-automation` for workflows. Where a skill overlaps an existing one, `AGENTS.md` names the winner.
- Before reporting `ready`, run `/ponytail-review branch`; fix each must-fix finding or say in the PR description why it stays.
- Before reporting `ready`, when the diff is sensitive, run the security review below after your last code commit and record it in the PR body.
- Push your branch and open a PR against `main`; turn on Auto-fix for it so CI failures and review comments wake you.
- Resolve **every** review thread (CodeRabbit and humans): fix the code, or reply with the reason it stays as is; then mark the thread resolved. Keep the branch up to date with `main`.
- Checks that a tool can run at a Windows desktop go into `tools/Rekey.DesktopChecks` and are described in `docs/manual-tests/<change>.md`; they gate the merge (orchestrator step 4).
- Checks that need a person (UI look and feel, sign-out, UAC, real apps, a physical keyboard): append them as a checklist to `docs/manual-tests/deferred-by-hand.md`, under a section named after your change, instead of waiting for the owner. The owner runs them all in the last roadmap change, `add-final-acceptance`.
- Tick a task once its automated evidence passes (unit tests, a desktop-check run). When part of its verification is by hand, tick it anyway and note in the task line that the by-hand part is deferred to `deferred-by-hand.md`.
- Report to the orchestrator in one line: `ready: PR #<N>` (or `blocked: <reason>`, or `question: <spec question>`).
- Never merge a PR, never start, stop or message other sessions, never start the next change.

## Security review

A diff is sensitive when it touches a path below or adds a network or process call (`HttpClient`, `WebRequest`, `WebClient`, `Socket`, `TcpClient`, `NamedPipe`, `Process.Start`, and the client calls `SendAsync`, `PostAsync`, `GetStringAsync`, `GetStreamAsync`, `GetByteArrayAsync`; the list is not exhaustive) in any code file. When unsure, treat it as sensitive.

| Area | Paths |
|---|---|
| Keyboard capture, key injection, text replacement, layouts | `src/Rekey.Win32/**`, `NativeMethods.txt` |
| Typed-text handling in the engine | `src/Rekey.Core/Engine*.cs`, `src/Rekey.Core/Detection/**`, word-learning and replacement code in `src/Rekey.Core/` |
| Logs, diagnostics, Incidents, word-analysis log | `src/Rekey.Core/Diagnostics/**`, `src/Rekey.App/Feedback/**` |
| Telemetry | telemetry code added by `add-telemetry` |
| Auto-update and installer | `src/Rekey.App/Updates/**`, installer and Velopack config |
| Network I/O | any file that adds an HTTP, socket or pipe call |
| CI and dependencies | `.github/workflows/**`, `Directory.Packages.props`, `*.csproj` package references |

The implementer loads the `security-audit` skill in guidance mode and reviews the branch's diff (`git diff origin/main...HEAD`) from source only: no audit output directory, no project code run by the skill. Full audit mode runs only when the owner asks. Focus: typed text reaching logs, exceptions, files or the network; hook callback robustness against injected or malformed input; update download integrity and signature checks; elevation and file-write locations; untrusted data in CI.

Each finding is fixed in the PR or deferred with a reason (and the follow-up's name where work remains). Findings carry file, line, class and severity, never typed text, Words or window titles. The PR body records:

```
## Security review
Skill: security-audit (guidance mode), reviewed commit <sha>
Areas: <areas from the table>
Findings: <none | each finding: severity, one line, fixed | deferred: reason>
```

For a diff outside every area the body says `Security review: not required`.
