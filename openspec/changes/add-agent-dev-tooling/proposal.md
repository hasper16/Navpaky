# Proposal

## Why

Implementer sessions spend most of their tokens re-reading the codebase, and the only reviewers before CodeRabbit are the analyzers. Rekey sees every keystroke, yet no step of the flow requires a security review of the code that handles them; the installed `security-audit` skill has been used once and no repo doc mentions it. The owner wants three third-party agent tools and that skill wired into the orchestrated flow so that sessions find code faster, write less of it and check it harder, with no extra steps for the owner.

## What Changes

- **Graphify** (Graphify-Labs/graphify): a local code knowledge graph. Implementers query it (`graphify query`, `path`, `explain`) before grepping. Code-only, no LLM backend, graph output ignored by git.
- **ponytail** (DietrichGebert/ponytail): an always-on "write only what the task needs" prompt at level `lite`, plus `/ponytail-review` as a pre-report self-review. Repo rules (`AGENTS.md`, `ub-mode`, analyzers) win on conflict.
- **agent-skills** (addyosmani/agent-skills): a curated set of seven skills that fill gaps in our skill set. The lifecycle commands (`/spec`, `/plan`, `/build`, `/ship`) are not installed because OpenSpec owns that flow.
- **security-audit** (the skill already installed in the canonical store, from cloudflare/security-audit-skill): becomes a required guidance-mode security review by the implementer before `ready` for any change that touches a security-sensitive area (keyboard capture, key injection, logs and diagnostics, telemetry, auto-update and installer, typed text, network I/O, CI and dependencies). Findings are fixed in the PR or deferred with a reason in the PR body; the orchestrator checks the PR body before merging. The skill is documented and verified, not reinstalled.
- All three new tools are installed globally, following the owner's "global by default" rule: skills land in the canonical store `C:\Users\Haspe\.ai-global\skills\` and reach every tool through the existing junctions.
- The repo gets only what the flow needs: `.gitignore` and `.graphifyignore` entries, a check script under `tools/agent-dev-tooling/`, and the new steps in `docs/agents/orchestration.md` and `AGENTS.md`.
- Product code and product behavior do not change.

## Capabilities

### New Capabilities

- `agent-dev-tooling`: Which third-party agent tools and skills the development workflow uses (Graphify, ponytail, curated agent-skills, security-audit), where they come from, how they are scoped, what each step of the orchestrated flow uses them for, which repo rules they must yield to, and how their presence is verified.

### Modified Capabilities

None.

## Impact

- Owner's machine (global): `uv` tool `graphifyy`; Claude Code user-scope plugin `ponytail`; seven skill folders in `C:\Users\Haspe\.ai-global\skills\`; user environment variables `GRAPHIFY_BACKEND`, `GRAPHIFY_QUERY_LOG_DISABLE`, `GRAPHIFY_NO_TIPS`, `PONYTAIL_DEFAULT_MODE`.
- Repo: `.gitignore`, new `.graphifyignore`, new `tools/agent-dev-tooling/` (`versions.json`, `Test-AgentDevTooling.ps1` and its case-table tests), `docs/agents/orchestration.md`, `AGENTS.md`.
- `security-audit`: no install change; its content hash is pinned in `versions.json` and checked by the script.
- Orchestrator readiness check: one more item, the PR body's security review section for changes in sensitive areas.
- No `src/` or test project changes; no NuGet dependencies; CI workflows unchanged.
- Independent of product changes 12–15 (see `docs/roadmap.md`).
