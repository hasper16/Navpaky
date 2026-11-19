# Tasks

## 1. Target framework (design D1, D2)

- [x] 1.1 Add `DotNetTargetFramework` = `net11.0` to `Directory.Build.props`; every `*.csproj` takes `$(DotNetTargetFramework)` or `$(DotNetTargetFramework)-windows`; the JSON readers use .NET 11's generic `GetTypeInfo<T>()` (CA2263). Verify: `git grep -n "net10" -- '*.csproj' '*.props' '*.cs' 'docs/manual-tests'` is empty and `dotnet build Navpaky.slnx -c Release -warnaserror` passes on SDK 11.0.100.
- [x] 1.2 `Checks.AppPath` takes the framework folder from the tool's own output folder; manual-test docs name `net11.0-windows`. Verify: the build of `tools/Navpaky.DesktopChecks` passes; the path is exercised by the desktop sets in 4.1.

## 2. Input clock (design D3)

- [x] 2.1 Test first: `InputClockTests` widens a 32-bit `GetTickCount` value to the clock's now. Then add `InputClock` (`GetTickCount64`, `Widen`) and route the hook, worker, focus tracker, input activity and the word-list edit clock through it. Verify: `dotnet test tests/Navpaky.Win32.Tests` passes, and no `Environment.TickCount64` remains in `src/Navpaky.Win32`; the sleep-and-wake part is deferred to `deferred-by-hand.md` (`upgrade-to-dotnet-11`).

## 3. Docs

- [x] 3.1 `AGENTS.md`, `README.md` (prerequisite .NET 11 SDK), ADR 0001, `openspec/config.yaml`; new ADR 0005 (STS now, .NET 12 LTS next); `docs/roadmap.md` lists the change. Verify: `git grep -n "\.NET 10" -- ':!openspec/changes/archive'` lists only ADR 0005, the `Settings` note on source-generated JSON since .NET 10, and this change.
- [x] 3.2 Record the breaking-change review (D4) and package check (D5) in this design. Verify: every package in `Directory.Packages.props` has a row in D5.

## 4. Single-file start (design D6)

- [x] 4.1 `Navpaky.App.csproj` sets `IncludeAllContentForSelfExtract` (and suppresses NETSDK1244) when `PublishSingleFile` is on; CI step `Published app starts` publishes with the Release workflow's flags and fails when the app exits within 30 s. Verify: the 0.4.3 pack from 57feed8 crashed with `BadImageFormatException` for `DirectWriteForwarder.dll`, the publish with the fix keeps running, and the CI step passes on the PR head.

## 5. Verification

- [ ] 5.1 All tests green on CI under .NET 11, including the accuracy check and the decision-speed test, and `docs/manual-tests/upgrade-to-dotnet-11.md` lists the three desktop sets and the Velopack update over 0.4.2 as an unticked checklist for the orchestrator. Verify: CI `build` job green on the PR head.
