# Design

## Context

See `proposal.md` for the motivation. Constraints that shape the approach:

- The owner's global rule: tools are installed at the user level by default. Skills live in the canonical store `C:\Users\Haspe\.ai-global\skills\`, which `C:\Users\Haspe\.claude\skills`, `C:\Users\Haspe\.agents\skills` and the other tool folders reach through directory junctions. MCP servers are registered in `C:\Users\Haspe\.ai-global\mcp\mcp_config.json` and merged into each tool's config. Project-only applies only when the owner says "local" or "project-only".
- The orchestrated flow (`docs/agents/orchestration.md`): one implementer per change, each in its own worktree under `.claude/worktrees/`, reporting `ready: PR #N` to an orchestrator that merges.
- Our rules: `AGENTS.md` privacy guardrails, analyzers as the reviewers (warnings as errors), `ub-mode` on every code edit, Conventional Commits, `core.hooksPath = .githooks`.
- Only Claude Code sessions run the orchestrated flow today.

Owner decisions (2026-11-15): ponytail global at level `lite`; agent-skills as a curated set; Graphify graph kept local and git-ignored, no Graphify git hook, no LLM backend.

## Goals / Non-Goals

**Goals:**
- Each tool installed once per machine, reproducible from pinned versions.
- Each tool has one named job in the orchestrated flow, and none of them adds a step for the owner.
- One script tells whether the machine is configured.

**Non-Goals:**
- Graphify Cloud, Graphify MCP server, Graphify `--strict` hook, Graphify git hooks.
- ponytail adapters for agents other than Claude Code (Cursor, Copilot, Gemini): added when one of them joins the flow.
- agent-skills personas, hooks and lifecycle commands.
- Running any of these tools in CI. CI stays build, CodeQL, GitGuardian, CodeRabbit and Snyk.

## Decisions

### D1. Graphify: global CLI and skill, local code-only graph

- Install: `uv tool install "graphifyy==<pin>"` (the official package is `graphifyy` with two y's), then `graphify install --platform agents`. That writes the skill to `C:\Users\Haspe\.agents\skills\graphify\`, which is the junction into the canonical store, so Claude Code, Gemini/Antigravity, Cursor and Copilot all see the same copy.
- No MCP server. The CLI answers the same questions (`query`, `path`, `explain`) and needs no long-running process or config merge into seven tool configs. Alternative considered: `graphifyy[mcp]` registered in `mcp_config.json`; rejected because nothing in the flow needs it and it adds a sync surface.
- Privacy: user environment variables `GRAPHIFY_BACKEND=none` (code-only graph, no LLM call for docs) and `GRAPHIFY_QUERY_LOG_DISABLE=1` (queries are not logged to `~/.cache`). Graphify parses code locally with tree-sitter; typed text never exists in the repo, so the graph holds only symbols of our own code. Our docs are not sent anywhere by Graphify; agents still read them directly.
- Repo files: `graphify-out/` in `.gitignore`; `.graphifyignore` lists `.private/`, `.claude/worktrees/`, `artifacts/`, `bin/`, `obj/`. `.private/` is already git-ignored and Graphify honors `.gitignore`, so the explicit entry is a second guard. Excluding `.claude/worktrees/` keeps the main checkout from indexing every implementer's copy.
- No `graphify hook install`: our hooks run from `.githooks` via `core.hooksPath`, and a second hook manager would collide. Freshness comes from the flow instead (D4).
- No always-on PreToolUse nudge in this change: `graphify claude install` writes into the project (`.claude/settings.json`, `CLAUDE.md`), which is project-only. The orchestration step in D4 gives the same effect without it. If a later Graphify release installs that hook at user scope, it can be added by updating this decision.

### D2. ponytail: global Claude Code plugin at level `lite`

- Install: `claude plugin marketplace add DietrichGebert/ponytail` and `claude plugin install ponytail@ponytail --scope user`. Plugins are Claude Code's global mechanism; the plugin brings the hooks, commands and band that a bare `SKILL.md` copy would lose. It is not copied into `.ai-global\skills\` because Claude Code would then load the prompt twice.
- Default level: user environment variable `PONYTAIL_DEFAULT_MODE=lite`. `lite` gives the reuse-first ladder without the stronger cutting of `full`/`ultra`, which is where it would collide with `ub-mode`'s "make it right" step and our XML-doc rule.
- Precedence: `AGENTS.md` gets one line saying repo rules, `ub-mode`, OpenSpec artifacts and analyzers win over ponytail. ponytail's own rule (never cut validation, error handling, security, accessibility) agrees with ours; the conflicts we expect are about tests and comments, both settled in our favor.
- `shortcut:` comments: not used in this repo. Our comment rule allows only "why" comments, and deferred work belongs in `tasks.md` or a follow-up. `AGENTS.md` states this so ponytail follows it.
- Use in the flow: `/ponytail-review branch` before `ready` (D4). `/ponytail-audit` is for the owner or the orchestrator on demand, not a gate.
- The band and the board keep data in `~/.config/ponytail/band/` on the owner's machine; they hold session costs and token counts, never typed text. They stay on (owner can hide with `/ponytail band`).

### D3. agent-skills: curated set copied into the canonical store

- Set (owner-approved): `security-and-hardening`, `debugging-and-error-recovery`, `doubt-driven-development`, `ci-cd-and-automation`, `documentation-and-adrs`, `source-driven-development`, `code-simplification`.
- Install: clone `addyosmani/agent-skills` at the pinned commit into a temporary folder, copy each `skills/<name>/` into `C:\Users\Haspe\.ai-global\skills\<name>\`, and copy the repo-level `references/` checklists a skill cites into that skill's own `references/` (per-skill installs otherwise lose them). Alternative considered: `npx skills add ... --skill <name>`; rejected because it does not carry `references/` and its target folders vary by agent. Alternative: the Claude Code plugin; rejected because it brings `/spec`, `/plan`, `/build`, `/ship` and session hooks that compete with OpenSpec, and it bypasses the canonical store.
- Overlaps and who wins:

  | agent-skills | Existing skill that wins | Where the new skill still adds value |
  |---|---|---|
  | `debugging-and-error-recovery` | `diagnosing-bugs` | stop-the-line rule, guard step after a fix |
  | `documentation-and-adrs` | `domain-modeling` (ADRs, glossary) | API and inline-doc guidance; XML docs still follow `docs/conventions/xml-docs.md` |
  | `security-and-hardening` | `security-audit` (full audits) | per-change hardening while coding update, telemetry and diagnostics code |
  | `code-simplification` | `ub-mode` refactor step | Chesterton's Fence before deleting code |
  | `source-driven-development` | global `ctx7` rule | citing sources for framework decisions |

  `doubt-driven-development` and `ci-cd-and-automation` have no overlap. `doubt-driven-development` is used for the keyboard hook, input injection and the updater, where a wrong confident answer is costly. Its optional cross-model escalation stays off unless the owner asks.
- Some references are JavaScript-oriented; agents apply the principles and ignore stack-specific examples. Our test style stays xUnit as in the repo.

### D4. Orchestrated flow changes

`docs/agents/orchestration.md`, implementer section, gains three lines:

1. On start, in the worktree: `graphify update .` (first run builds the graph), then use `graphify query` / `path` / `explain` to locate code before broad searches.
2. Use the curated skills where D3 says they apply.
3. Before reporting `ready`: `/ponytail-review branch`; fix must-fix findings or explain them in the PR description.

The orchestrator does nothing new: it still merges on required checks and resolved threads. The ponytail review is not a merge gate, so a tool failure never blocks a merge.

### D5. Pins and the check script

- `tools/agent-dev-tooling/versions.json` holds the Graphify version, the ponytail plugin version and the agent-skills commit, plus the list of curated skills.
- `tools/agent-dev-tooling/Test-AgentDevTooling.ps1` (PowerShell 7) checks, per tool: Graphify version equals the pin and `graphify --help` exits 0; the two environment variables are set; the Graphify skill folder exists in the canonical store and through `C:\Users\Haspe\.claude\skills`; `claude plugin list` shows ponytail at the pin and `PONYTAIL_DEFAULT_MODE` is `lite`; each curated skill folder exists in the canonical store and carries a `SOURCE` file with the pinned commit; no lifecycle command from agent-skills exists in `C:\Users\Haspe\.claude\commands`; `git status --porcelain` lists nothing under `graphify-out/`, `.claude/skills/` or `.agents/skills/`. Output is one line per check (`PASS`/`FAIL`, check name, reason) and exit code 1 on any failure. It prints paths and versions only.
- The checks are functions that take their inputs (command output, paths) as parameters. A case-table test script next to it, `Test-AgentDevTooling.Tests.ps1`, feeds them fake inputs and asserts pass/fail and the exit code, in the style of `.claude/hooks/Test-BlockDangerousGit.ps1` (no Pester dependency).
- The script is not wired into CI: CI runners do not have the owner's global tools.

### D6. Parallel work

This change touches no file under `src/`, `tests/` or `tools/Rekey.DesktopChecks/`, so it can run alongside product changes 12–15. The shared files are `AGENTS.md`, `docs/agents/orchestration.md` and `.gitignore`; edits there are small, and the implementer rebases if a product change merged first. Global installs reach only sessions started after them, so an implementer already working keeps its current tools until its change ends; the orchestrator does not restart running sessions.

## Risks / Trade-offs

- [A tool update changes its behavior or install layout] → versions are pinned; updates are explicit and re-run the check script.
- [Supply chain: a tool runs code on the owner's machine (uv package, plugin hooks)] → official sources only, pinned versions, ponytail ships no binaries; the hooks are reviewed once at install time (task 2.1).
- [ponytail and `ub-mode` give different advice] → level `lite` and the precedence line in `AGENTS.md`; if conflicts persist the owner can set `PONYTAIL_DEFAULT_MODE=off` without uninstalling.
- [Graph is stale after a rebase] → the implementer re-runs `graphify update .` after rebasing; the graph is advisory and never the only source for an edit.
- [Duplicate skills confuse skill selection] → the overlap table in D3 is copied into `AGENTS.md` as a short precedence list.
- [More context per session from extra skills and the always-on prompt] → only seven skills, `lite` level, and graph queries replace file reads.

## Migration Plan

1. Install the tools and set the environment variables (tasks 1–3).
2. Land the repo files and the doc changes; run the check script.
3. The next implementer started by the orchestrator uses the new steps.

Rollback: `graphify uninstall --purge` and `uv tool uninstall graphifyy`; `claude plugin uninstall ponytail@ponytail`; delete the seven skill folders from the canonical store; remove the environment variables; revert the repo commit. Nothing in the product depends on these tools.
