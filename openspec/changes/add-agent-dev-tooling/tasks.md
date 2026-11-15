# Tasks

All installs run on the owner's machine at user scope (design D1–D3). No task edits `src/`, `tests/` or `tools/Rekey.DesktopChecks/`.

## 1. Pins and check script

- [x] 1.1 Look up the current releases (Graphify on PyPI `graphifyy`, ponytail plugin release, agent-skills `main` commit) and write `tools/agent-dev-tooling/versions.json` with those pins and the seven curated skill names; verify the file parses with `ConvertFrom-Json` and lists three tools and seven skills
- [x] 1.2 Write `tools/agent-dev-tooling/Test-AgentDevTooling.Tests.ps1` with failing cases for each check in design D5 (version match and mismatch, `GRAPHIFY_BACKEND` and `GRAPHIFY_QUERY_LOG_DISABLE` missing or present with a wrong value such as `openai` or `0`, `PONYTAIL_DEFAULT_MODE` not `lite`, marketplace source without the pinned tag, missing skill folder, missing `SOURCE`, security-audit folder hash match and mismatch, lifecycle command present, tool files in `git status`, aggregation to exit code 1); verify it fails because the checks do not exist yet
- [x] 1.3 Implement `tools/agent-dev-tooling/Test-AgentDevTooling.ps1` with the smallest code that passes 1.2, then refactor; verify `pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.Tests.ps1` passes and the script's output holds only names, versions, paths and status codes

## 2. Graphify

- [ ] 2.1 Install `uv tool install "graphifyy==<pin>"` and run `graphify install --platform agents`; verify `graphify --version` equals the pin and `graphify\SKILL.md` exists in both `C:\Users\Haspe\.ai-global\skills\` and `C:\Users\Haspe\.claude\skills\`
- [ ] 2.2 Set user environment variables `GRAPHIFY_BACKEND=none`, `GRAPHIFY_QUERY_LOG_DISABLE=1` and `GRAPHIFY_NO_TIPS=1`; verify in a new shell that they equal `none`, `1` and `1`
- [ ] 2.3 Add `graphify-out/` to `.gitignore` and create `.graphifyignore` (`.private/`, `.claude/worktrees/`, `artifacts/`, `bin/`, `obj/`); verify `graphify update .` in this worktree completes without naming an LLM backend, `graph.json` has no path under `.private/` or `.claude/worktrees/`, and `git status --porcelain` lists nothing under `graphify-out/`
- [ ] 2.4 Verify `graphify query "where is the keyboard hook callback"` returns a node in `Rekey.Win32` and exits 0

## 3. ponytail

- [ ] 3.1 Read the ponytail plugin manifest and hook scripts at the pinned release and confirm they ship no binaries and make no network calls outside the band's local files; record the result in the PR description
- [ ] 3.2 Run `claude plugin marketplace add DietrichGebert/ponytail#<pinned tag>` and `claude plugin install ponytail@ponytail --scope user`; set `PONYTAIL_DEFAULT_MODE=lite`; verify `claude plugin marketplace list` shows the source at the pinned tag, `claude plugin list` shows ponytail at the pin and a new session reports level `lite`
- [ ] 3.3 Verify `/ponytail-review branch` runs on this change's branch and returns a review

## 4. agent-skills

- [ ] 4.1 Clone `addyosmani/agent-skills` at the pinned commit into a temporary folder; copy the seven curated `skills/<name>/` folders into `C:\Users\Haspe\.ai-global\skills\`, each with the repo-level `references/` files it cites and a `SOURCE` file holding repo and commit; verify each folder is visible under `C:\Users\Haspe\.claude\skills\` and no `/spec`, `/plan`, `/build` or `/ship` command from agent-skills exists in `C:\Users\Haspe\.claude\commands\`
- [ ] 4.2 Verify the new skills appear in a new Claude Code session's skill list and do not replace any existing skill of the same name (none of the seven names exists in the store before 4.1)

## 5. security-audit

- [ ] 5.1 Confirm the installed `C:\Users\Haspe\.ai-global\skills\security-audit\` is the cloudflare/security-audit-skill content (compare every file in the installed folder with that repository's `main`, by path and content; each file missing upstream or differing from it is reconciled with upstream or explicitly rejected by the owner before the hash is recorded; no hash is pinned while a difference is open) without reinstalling; compute the folder hash per design D7 and add it to `versions.json`; verify the check script reports `PASS` for security-audit and `FAIL` after a one-byte change to a copy of the folder
- [ ] 5.2 Run a guidance-mode review on the diff of a past sensitive PR (for example the `add-auto-update-and-installer` merge) as a dry run; verify it writes no output directory, its findings follow the privacy guardrails, and the result fits the PR-body template in design D7

## 6. Flow and rules

- [ ] 6.1 Add to `AGENTS.md` the security review rule (guidance mode before `ready` for sensitive changes, findings never carry typed text), the precedence line (repo rules, `ub-mode`, OpenSpec artifacts and analyzers win over ponytail and agent-skills), the overlap list from design D3, and "no `shortcut:` comments; deferred work goes to `tasks.md` or a follow-up"; verify `AGENTS.md` stays under its current section structure and the `writing-for-agents` skill's checks pass
- [ ] 6.2 Add the four implementer steps from design D4, the sensitive-path table and the PR-body template from D7 to `docs/agents/orchestration.md`; verify each step names the command it runs
- [ ] 6.3 Add the security review item to the orchestrator's step 3 readiness check in `docs/agents/orchestration.md` (diff paths against the table and added lines against the API list, section present with mode, areas covering every match and findings, reviewed commit is an ancestor of the head, API scan over code files only, re-review when a later commit touches a table path, adds a listed API anywhere, or touches a file the earlier diff flagged as sensitive); verify by applying it by hand to PR #32 (sensitive) and to this change's PR (not sensitive, although its prose names `HttpClient`) and recording both outcomes in the PR description
- [ ] 6.4 Run `pwsh -File tools/agent-dev-tooling/Test-AgentDevTooling.ps1` on the owner's machine; verify every line is `PASS` and the exit code is 0

## Workflow follow-up

- Archive the change after the PR is merged (`/opsx:archive add-agent-dev-tooling` on `chore/archive-add-agent-dev-tooling`).
- The orchestrator starts using the new implementer steps from the next implementer it starts.
