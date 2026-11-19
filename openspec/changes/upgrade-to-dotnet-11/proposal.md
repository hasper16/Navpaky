# Proposal

## Why

`global.json` already pins the .NET 11 SDK (PR #27), but every project still targets .NET 10. The owner decided to move the app itself to .NET 11 before the design overhaul, so the overhaul builds on the runtime it ships with. .NET 11 is an STS release supported until 2028-11-14, the same end date as .NET 10 LTS, so the move costs no support time; the next LTS, .NET 12, follows in November 2027.

## What Changes

- Every project targets `net11.0` / `net11.0-windows`, set once in `Directory.Build.props` so the next move is one line.
- Paths that name the target framework folder (`bin/Debug/net10.0-windows`) follow it: the desktop checks find the app's build folder from their own, and the manual-test docs name `net11.0-windows`.
- .NET 11 changes `Environment.TickCount64` on Windows to leave out time the PC spent asleep, while Windows still stamps keys, WinEvents and the last-input time with a clock that counts sleep. Navpaky measures all input times on the Windows clock, so the hook watchdog and focus tracking keep working after the PC wakes.
- WPF 11 makes two of its assemblies mixed-mode, which a single-file bundle cannot load, so the single-file Release build extracts its assemblies at first start, and CI starts the published app to catch such crashes.
- Docs follow: `AGENTS.md`, `README.md` (prerequisite is the .NET 11 SDK), ADR 0001, `openspec/config.yaml`; a new ADR records the move to an STS release and the plan to move to .NET 12 LTS; the roadmap lists this change.
- Desktop checks and a Velopack update over the installed 0.4.2 go into `docs/manual-tests/upgrade-to-dotnet-11.md` for the orchestrator to run.

## Capabilities

### New Capabilities

None.

### Modified Capabilities

- `keyboard-capture`: input times stay comparable with the times Windows reports after the PC sleeps or hibernates.

## Impact

- All `*.csproj` (target framework moves to `Directory.Build.props`), `Directory.Build.props`.
- `src/Navpaky.Win32` (hook, focus tracker, input worker, input activity), `src/Navpaky.App/App.xaml.cs`, `NativeMethods.txt`: one system clock for input times. Security-sensitive area (keyboard capture), so the PR carries a security review.
- `tools/Navpaky.DesktopChecks` (app path), `docs/manual-tests/*`, `docs/adr/*`, `docs/roadmap.md`, `README.md`, `AGENTS.md`, `openspec/config.yaml`.
- `src/Navpaky.App/Navpaky.App.csproj` (single-file extraction), `.github/workflows/ci.yml` (start the published app).
- No package versions change; workflows already take the SDK from `global.json`.
- Users get the .NET 11 runtime inside the self-contained package; Windows 10/11 x64 stays the supported platform.
