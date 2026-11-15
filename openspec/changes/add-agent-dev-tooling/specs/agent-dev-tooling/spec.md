# Spec Delta

## Purpose

Defines the third-party agent tools the development workflow relies on (Graphify, ponytail, agent-skills): their sources, install scope, role in the orchestrated flow, the repo rules they yield to, and how their presence is verified.

## ADDED Requirements

### Requirement: Tools come from pinned official sources
Each tool SHALL be installed only from its official source (PyPI package `graphifyy`, GitHub `DietrichGebert/ponytail`, GitHub `addyosmani/agent-skills`) at a version or commit recorded in `tools/agent-dev-tooling/versions.json`. An update SHALL change that file in the same commit.

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
- **THEN** it reports pass for Graphify, ponytail and agent-skills and exits with code 0

#### Scenario: Missing tool
- **WHEN** one tool is not installed
- **THEN** the script reports that tool as failed with a reason and exits with a non-zero code
