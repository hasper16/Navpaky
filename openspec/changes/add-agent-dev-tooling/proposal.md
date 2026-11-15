# Proposal

## Why

Implementer sessions spend most of their tokens re-reading the codebase, and the only reviewers before CodeRabbit are the analyzers. The owner wants three third-party agent tools wired into the orchestrated flow so that sessions find code faster, write less of it and check it harder, with no extra steps for the owner.

## What Changes

- **Graphify** (Graphify-Labs/graphify): a local code knowledge graph. Implementers query it (`graphify query`, `path`, `explain`) before grepping. Code-only, no LLM backend, graph output ignored by git.
- **ponytail** (DietrichGebert/ponytail): an always-on "write only what the task needs" prompt at level `lite`, plus `/ponytail-review` as a pre-report self-review. Repo rules (`AGENTS.md`, `ub-mode`, analyzers) win on conflict.
- **agent-skills** (addyosmani/agent-skills): a curated set of seven skills that fill gaps in our skill set. The lifecycle commands (`/spec`, `/plan`, `/build`, `/ship`) are not installed because OpenSpec owns that flow.
- All three are installed globally, following the owner's "global by default" rule: skills land in the canonical store `C:\Users\Haspe\.ai-global\skills\` and reach every tool through the existing junctions.
- The repo gets only what the flow needs: `.gitignore` and `.graphifyignore` entries, a check script under `tools/agent-dev-tooling/`, and the new steps in `docs/agents/orchestration.md` and `AGENTS.md`.
- Product code and product behavior do not change.

## Capabilities

### New Capabilities

- `agent-dev-tooling`: Which third-party agent tools the development workflow uses, where they come from, how they are scoped, what each step of the orchestrated flow uses them for, which repo rules they must yield to, and how their presence is verified.

### Modified Capabilities

None.

## Impact

- Owner's machine (global): `uv` tool `graphifyy`; Claude Code user-scope plugin `ponytail`; seven skill folders in `C:\Users\Haspe\.ai-global\skills\`; user environment variables `GRAPHIFY_BACKEND`, `GRAPHIFY_QUERY_LOG_DISABLE`, `PONYTAIL_DEFAULT_MODE`.
- Repo: `.gitignore`, new `.graphifyignore`, new `tools/agent-dev-tooling/` (`versions.json`, `Test-AgentDevTooling.ps1` and its case-table tests), `docs/agents/orchestration.md`, `AGENTS.md`.
- No `src/` or test project changes; no NuGet dependencies; CI workflows unchanged.
- Independent of product changes 12–15 (see `docs/roadmap.md`).
