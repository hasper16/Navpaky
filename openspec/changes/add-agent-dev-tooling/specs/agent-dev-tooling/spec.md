# Spec Delta

## Purpose

Defines the third-party agent tools and skills the development workflow relies on (Graphify, ponytail, agent-skills, security-audit): their sources, install scope, role in the orchestrated flow, the repo rules they yield to, and how their presence is verified.

## ADDED Requirements

### Requirement: Tools come from pinned official sources
Each tool SHALL be installed only from its official source (PyPI package `graphifyy`, GitHub `DietrichGebert/ponytail`, GitHub `addyosmani/agent-skills`) at a version or commit recorded in `tools/agent-dev-tooling/versions.json`. The already-installed `security-audit` skill SHALL be pinned there by a content hash of its folder. An update SHALL change that file in the same commit.

#### Scenario: Look-alike package is rejected
- **WHEN** an install step names a package or repository other than the official one (for example PyPI `graphify`)
- **THEN** the check script reports the tool as failed

#### Scenario: Installed version matches the pin
- **WHEN** the check script runs
- **THEN** it reports each tool's installed version or commit next to the pinned one and fails on a mismatch

### Requirement: Tools are installed globally
Each tool SHALL be installed at the user level. Skills SHALL live in the canonical store `C:\Users\Haspe\.ai-global\skills\` and reach tools through the existing junctions. No tool SHALL write skills, hooks or MCP entries into the repository, unless the owner says "local" or "project-only" for that tool.

#### Scenario: Skill visible through junction
- **WHEN** a curated skill is installed
- **THEN** its folder exists in `C:\Users\Haspe\.ai-global\skills\` and the same folder is visible under `C:\Users\Haspe\.claude\skills\`

#### Scenario: No tool files in the repository
- **WHEN** the tools are installed and `git status` runs in the repository
- **THEN** no file under `.claude/skills/`, `.agents/skills/` or `graphify-out/` is listed as untracked or modified

### Requirement: Repo rules take precedence over tool guidance
When a tool's guidance conflicts with `AGENTS.md`, the `ub-mode` skill, an OpenSpec artifact or an analyzer rule, the repo rule SHALL win. No tool SHALL be a reason to disable an analyzer rule, skip a failing test or remove a privacy guardrail.

#### Scenario: Minimal-code advice versus a failing test first
- **WHEN** ponytail advises skipping a test that `ub-mode` requires
- **THEN** the implementer writes the failing test first, as `ub-mode` requires

#### Scenario: Analyzer warning stays an error
- **WHEN** a tool suggests code that an analyzer flags
- **THEN** the implementer fixes the code instead of suppressing the rule

### Requirement: Code graph stays local
The code graph SHALL be built from code only, with no LLM backend, and SHALL NOT be committed. It SHALL exclude `.private/` and `.claude/worktrees/`. Graph query logging SHALL be off.

#### Scenario: Owner notes are not indexed
- **WHEN** the graph is built in a checkout that contains `.private/`
- **THEN** no node in the graph refers to a file under `.private/`

#### Scenario: No backend is called
- **WHEN** the graph is built or updated
- **THEN** no request goes to an LLM provider and the build output names no backend

### Requirement: Implementers orient with the code graph
At the start of a change, the implementer SHALL build or update the code graph in its worktree and SHALL prefer graph queries over broad file searches when locating code.

#### Scenario: Fresh worktree
- **WHEN** an implementer starts in a new worktree
- **THEN** it runs the graph update before reading code and the graph reflects that worktree's commit

### Requirement: Implementers self-review before reporting ready
Before reporting `ready` to the orchestrator, the implementer SHALL run the ponytail review on its branch and SHALL either fix each must-fix finding or note in the PR description why it stays.

#### Scenario: Must-fix finding
- **WHEN** the self-review reports a must-fix finding
- **THEN** the PR either contains the fix or its description states why the finding stays

### Requirement: Security review before ready for sensitive changes
Before reporting `ready`, the implementer SHALL run the `security-audit` skill in guidance mode over its branch's diff when the diff touches a security-sensitive area: keyboard capture, key injection or text replacement, typed-text handling, logs and diagnostics, telemetry, auto-update and installer, network I/O, CI workflows or dependency manifests.

#### Scenario: Sensitive change
- **WHEN** a branch changes the keyboard capture code
- **THEN** the implementer runs the guidance-mode security review before reporting `ready`

#### Scenario: Change outside sensitive areas
- **WHEN** a branch changes only OpenSpec artifacts or UI text resources
- **THEN** no security review is required and the PR body states "Security review: not required"

#### Scenario: Full audit only on request
- **WHEN** the implementer runs the security review
- **THEN** it uses guidance mode and writes no audit output directory; full audit mode runs only when the owner asks for it

### Requirement: Security findings are resolved in the PR
Each security finding SHALL be fixed in the same PR or listed in the PR body's `Security review` section with a reason for deferring it. That section SHALL name the skill mode, the commit reviewed and the sensitive areas touched. Findings SHALL carry no typed text, Words or window titles.

#### Scenario: Finding fixed
- **WHEN** the review reports a confirmed finding and the implementer fixes it
- **THEN** the PR contains the fix and the section lists the finding as fixed

#### Scenario: Finding deferred
- **WHEN** a finding is not fixed in the PR
- **THEN** the section lists it with the reason and, where follow-up work is needed, the follow-up's name

### Requirement: Orchestrator verifies the security review
Before merging a PR whose diff touches a security-sensitive area, the orchestrator SHALL confirm that the PR body has a `Security review` section naming a reviewed commit that is the PR head or an ancestor of it, and SHALL return the PR to the implementer otherwise.

#### Scenario: Section missing
- **WHEN** a PR touches the auto-update code and its body has no `Security review` section
- **THEN** the orchestrator sends the PR back to the same implementer and does not merge

#### Scenario: Head moved after the review
- **WHEN** commits after the reviewed one change files in a sensitive area
- **THEN** the orchestrator asks the implementer to review again before merging

### Requirement: Curated skills do not compete with OpenSpec
Only the curated agent-skills set SHALL be installed. Lifecycle commands that drive specs, plans, builds or releases outside OpenSpec SHALL NOT be installed. Where a curated skill overlaps an existing project skill, the existing skill SHALL be used.

#### Scenario: Lifecycle command absent
- **WHEN** an agent session lists its commands
- **THEN** no `/spec`, `/plan`, `/build` or `/ship` command from agent-skills is present

#### Scenario: Overlap resolved to the existing skill
- **WHEN** a task is bug diagnosis
- **THEN** the implementer uses `diagnosing-bugs`, not `debugging-and-error-recovery`

### Requirement: Tool presence is verified by one command
A single check script SHALL report, per tool, pass or fail with a reason, and SHALL exit non-zero when any tool fails. Its output SHALL carry only tool names, versions, paths and status codes.

#### Scenario: All tools present
- **WHEN** the check script runs on a configured machine
- **THEN** it reports pass for Graphify, ponytail, agent-skills and security-audit and exits with code 0

#### Scenario: Missing tool
- **WHEN** one tool is not installed
- **THEN** the script reports that tool as failed with a reason and exits with a non-zero code
