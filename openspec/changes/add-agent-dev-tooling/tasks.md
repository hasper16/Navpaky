# Tasks

All installs run on the owner's machine at user scope (design D1–D3). No task edits `src/`, `tests/` or `tools/Rekey.DesktopChecks/`.

## 1. Pins and check script

- [ ] 1.1 Look up the current releases (Graphify on PyPI `graphifyy`, ponytail plugin release, agent-skills `main` commit) and write `tools/agent-dev-tooling/versions.json` with those pins and the seven curated skill names; verify the file parses with `ConvertFrom-Json` and lists three tools and seven skills
- [ ] 1.2 Write `tools/agent-dev-tooling/Test-AgentDevTooling.Tests.ps1` with failing cases for each check in design D5 (version match and mismatch, missing skill folder, missing `SOURCE`, lifecycle command present, tool files in `git status`, aggregation to exit code 1); verify it fails because the checks do not exist yet
- [ ] 1.3 Implement `tools/agent-dev-tooling/Test-AgentDevTooling.ps1` with the smallest code that passes 1.2, then refactor; verify `pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.Tests.ps1` passes and the script's output holds only names, versions, paths and status codes

## 2. Graphify

- [ ] 2.1 Install `uv tool install "graphifyy==<pin>"` and run `graphify install --platform agents`; verify `graphify --version` equals the pin and `graphify\SKILL.md` exists in both `C:\Users\Haspe\.ai-global\skills\` and `C:\Users\Haspe\.claude\skills\`
- [ ] 2.2 Set user environment variables `GRAPHIFY_BACKEND=none` and `GRAPHIFY_QUERY_LOG_DISABLE=1`; verify in a new shell that both are set
- [ ] 2.3 Add `graphify-out/` to `.gitignore` and create `.graphifyignore` (`.private/`, `.claude/worktrees/`, `artifacts/`, `bin/`, `obj/`); verify `graphify update .` in this worktree completes without naming an LLM backend, `graph.json` has no path under `.private/` or `.claude/worktrees/`, and `git status --porcelain` lists nothing under `graphify-out/`
- [ ] 2.4 Verify `graphify query "where is the keyboard hook callback"` returns a node in `Rekey.Win32` and exits 0

## 3. ponytail

- [ ] 3.1 Read the ponytail plugin manifest and hook scripts at the pinned release and confirm they ship no binaries and make no network calls outside the band's local files; record the result in the PR description
- [ ] 3.2 Run `claude plugin marketplace add DietrichGebert/ponytail` and `claude plugin install ponytail@ponytail --scope user`; set `PONYTAIL_DEFAULT_MODE=lite`; verify `claude plugin list` shows ponytail at the pin and a new session reports level `lite`
- [ ] 3.3 Verify `/ponytail-review branch` runs on this change's branch and returns a review

## 4. agent-skills

- [ ] 4.1 Clone `addyosmani/agent-skills` at the pinned commit into a temporary folder; copy the seven curated `skills/<name>/` folders into `C:\Users\Haspe\.ai-global\skills\`, each with the repo-level `references/` files it cites and a `SOURCE` file holding repo and commit; verify each folder is visible under `C:\Users\Haspe\.claude\skills\` and no `/spec`, `/plan`, `/build` or `/ship` command from agent-skills exists in `C:\Users\Haspe\.claude\commands\`
- [ ] 4.2 Verify the new skills appear in a new Claude Code session's skill list and do not replace any existing skill of the same name (none of the seven names exists in the store before 4.1)

## 5. Flow and rules

- [ ] 5.1 Add to `AGENTS.md` the precedence line (repo rules, `ub-mode`, OpenSpec artifacts and analyzers win over ponytail and agent-skills), the overlap list from design D3, and "no `shortcut:` comments; deferred work goes to `tasks.md` or a follow-up"; verify `AGENTS.md` stays under its current section structure and the `writing-for-agents` skill's checks pass
- [ ] 5.2 Add the three implementer steps from design D4 to `docs/agents/orchestration.md`; verify each step names the command it runs and the orchestrator section is unchanged
- [ ] 5.3 Run `pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.ps1` on the owner's machine; verify every line is `PASS` and the exit code is 0

## Workflow follow-up

- Archive the change after the PR is merged (`/opsx:archive add-agent-dev-tooling` on `chore/archive-add-agent-dev-tooling`).
- The orchestrator starts using the new implementer steps from the next implementer it starts.
