# Design

## Context

`global.json` pins SDK 11.0.100 (`rollForward: latestFeature`) and every workflow takes the SDK from it, but each of the 14 projects names `net10.0` or `net10.0-windows` itself. `Directory.Build.props` already sets `LangVersion=latest`. The app ships self-contained (single file, ReadyToRun), so the runtime moves with the package. See proposal.md for why.

## Goals / Non-Goals

**Goals:**
- One place names the target framework.
- Every .NET 11 breaking change that reaches our code is handled, not only the ones the compiler reports.

**Non-Goals:**
- Package version bumps (Dependabot owns them) and new .NET 11 or C# 15 features.
- NativeAOT, trimming or other publish changes.

## Decisions

### D1. `DotNetTargetFramework` in `Directory.Build.props`

`Directory.Build.props` sets `DotNetTargetFramework` to `net11.0`; each project writes `<TargetFramework>$(DotNetTargetFramework)</TargetFramework>` or `$(DotNetTargetFramework)-windows`. The move to .NET 12 is then this property plus `global.json`. Alternative: set `TargetFramework` itself in the props file and override it in Windows projects; rejected because a project that forgets the override silently loses WPF/Windows APIs, and the effective value is no longer visible in the project file.

### D2. Desktop checks find the app by their own target framework

`Checks.AppPath` took the folder `bin/Debug/net10.0-windows`. The tool and the app build with the same `DotNetTargetFramework-windows`, so the tool reads its own output folder's name from `AppContext.BaseDirectory` instead of naming a version. Alternative: a generated constant from MSBuild; more machinery for the same result.

### D3. One input clock on Windows' own time base (`InputClock`)

.NET 11 changes `Environment.TickCount64` on Windows from `GetTickCount64` to `QueryUnbiasedInterruptTime`: it no longer counts time spent asleep or hibernated, and there is no switch to restore the old value. Windows still stamps input with the `GetTickCount` base, which counts sleep. Navpaky mixes the two:

| Where | Windows time | Compared with |
|---|---|---|
| `FocusTracker.EventTime` (WinEvent thread → worker) | 32-bit WinEvent `dwmsEventTime` | key times from the hook |
| `SessionInputActivity.SystemLastInputTick` (worker) | 32-bit `LASTINPUTINFO.dwTime` | `InputHook.LastInputTick` in `HookWatchdog` |

After the first sleep the 32-bit widening (`now - (uint)(now - time)`) would put every Windows time about 49 days in the past: the watchdog would never see the hook fall behind, and focus changes would always look older than the keys around them, so a Keystroke typed right after a focus move would not be dropped as uncertain.

`Navpaky.Win32.InputClock` reads `GetTickCount64` (CsWin32, `NativeMethods.txt`) and owns the 32-bit widening. Every input time uses it: the hook callbacks (hook thread; `GetTickCount64` reads shared memory, so the callback stays allocation-free), the worker's `Now`, `NoteProgress` and staleness check, the focus tracker, the input activity, and the word-list edit clock in `App` (compared with Word times by the engine). `KeepSoundAwake` keeps `Environment.TickCount64`: it only measures its own one-second gap. Alternative: `DateTime.UtcNow` as Microsoft's guide suggests for wall time; rejected because Windows' input times are not wall-clock and converting each one would need its own base.

The test checks that a 32-bit `GetTickCount` value widens to the clock's now. On a machine that never slept, `Environment.TickCount64` passes it too, so it pins the time base rather than reproducing the drift; the sleep scenario is a by-hand check (`deferred-by-hand.md`, `upgrade-to-dotnet-11`).

### D4. .NET 11 breaking changes reviewed

Source: the .NET 11 breaking-change index (learn.microsoft.com/dotnet/core/compatibility/11, updated 2026-11-17; Microsoft marks it incomplete) and the per-item articles for the two that reach our code. None is listed for WPF, Windows Forms, System.Text.Json or reflection.

| Change | Effect on Navpaky |
|---|---|
| `Environment.TickCount`/`TickCount64` exclude sleep on Windows | Breaks input timing after sleep; fixed by D3. |
| `GetCommandLineArgs()[0]` keeps the apphost invocation name | None: `App` only searches the arguments for `--check-ui`, `--update-feed` and `--update-pin`, never reads `[0]`. |
| New API obsoletions with their own diagnostic IDs | Would fail the build under warnings-as-errors; the build shows none. |
| New generic `JsonSerializerOptions.GetTypeInfo<T>()` (not a listed break) | CA2263 fails the build on the two JSON readers that cast `GetTypeInfo(typeof(T))`; they call the generic overload. |
| `Assembly.GetCallingAssembly` without stack-trace support | None: not used, and the publish keeps stack-trace support. |
| Floating-point to small-integer casts saturate; `Math.Round` correctly rounded | None found in scoring paths; the accuracy and decision-speed tests guard the numbers. |
| Minimum hardware requirements updated (JIT) | Windows 10/11 x64 machines that run the current WPF meet them. |
| Restore no longer picks a higher version from the global packages folder | None: central package management pins exact versions. |
| VSTest drops Newtonsoft.Json | None: tests run on Microsoft.Testing.Platform (`global.json`). |
| `runtimeconfig.dev.json` overrides `runtimeconfig.json` | None: the package ships no `.dev.json`. |
| WPF 11 makes `DirectWriteForwarder` and `System.Printing` mixed-mode (dotnet/wpf#11575; not in the index) | The single-file Release build crashes at start; fixed by D6. |
| Others (Unix-only, NativeAOT, MAUI, containers, tar/zip, tensors, CBOR, WMI) | Not used. |

### D5. Packages on .NET 11

No package targets `net11.0` yet; each ships assets .NET 11 can consume:

| Package | Assets used on net11.0 |
|---|---|
| CommunityToolkit.Mvvm 8.4.2 | net8.0 + source generators |
| H.NotifyIcon.Wpf 2.4.1 | net10.0-windows7.0 |
| Microsoft.Diagnostics.Runtime 4.2.751201 | net10.0 |
| Microsoft.Windows.CsWin32 0.3.371 | source generator only |
| Sentry 6.13.0 | net10.0 |
| Velopack 1.2.161 | net10.0 (the `vpk` 1.2.161 tool packs any self-contained folder) |
| WPF-UI 4.3.0 | net10.0-windows7.0 |
| xunit.v3.mtp-v2 4.0.3 | net8.0 via xunit.v3 core |
| SonarAnalyzer.CSharp, StyleCop.Analyzers | analyzers, framework-neutral |

Restore, build and tests on the CI runner (SDK 11.0.100) are the proof; the desktop sets and the update over 0.4.2 check WPF, the tray, Velopack and Sentry at run time.

### D6. Single-file publish extracts every assembly

WPF 11 builds `DirectWriteForwarder.dll` and `System.Printing.dll` as mixed-mode (C++/CLI, IJW) assemblies instead of pure IL (dotnet/wpf#11575). A mixed-mode assembly loads only from a file on disk, and the single-file bundler still treats both as managed assemblies it keeps in memory, so a self-contained single-file WPF app on .NET 11.0.0 dies in `Application`'s type initializer with `BadImageFormatException` for `DirectWriteForwarder.dll`, with or without ReadyToRun (dotnet/wpf#11842, open; reproduced with the 0.4.3 test pack from 57feed8). `IncludeNativeLibrariesForSelfExtract` does not help: it extracts only native libraries.

`Navpaky.App.csproj` sets `IncludeAllContentForSelfExtract` whenever `PublishSingleFile` is on, so every single-file publish (`release.yml`, `docs/release.md`, the manual checklist) gets it from one place: the bundle extracts all assemblies to `%TEMP%\.net\Navpaky.App\<hash>` at first start and loads them from disk. NETSDK1244 warns that this mode is legacy; following the warning brings the crash back, so the project suppresses it next to the setting. `AppContext.BaseDirectory` becomes the extraction folder; Navpaky reads its own location only through `Environment.ProcessPath`, and Velopack's runtime locates the install from the process path, so neither moves.

Alternatives: keep the two assemblies and `ijwhost.dll` out of the bundle (`ExcludeFromSingleFile`); rejected because the install is then four files, and a copy started from inside a ZIP, which Explorer extracts as the exe alone, crashes again. A multi-file publish; rejected for the same reason. Revisit when dotnet/wpf#11842 ships a fix.

CI guards it: the `Published app starts` step publishes with the Release workflow's flags, starts the executable and fails when it exits within 30 seconds.

## Risks / Trade-offs

- [The breaking-change index is incomplete] → CI build and every test run on .NET 11; the desktop sets re-run before merge; the by-hand sleep check is deferred to final acceptance.
- [Packages compiled for net10.0 run on net11.0] → forward compatibility is the .NET rule; the desktop sets exercise each package's runtime path.
- [Install size grows with the .NET 11 runtime] → the update check records the new size against 186 MB.
- [The first start extracts about 200 MB to `%TEMP%` and is slower; cleaning `%TEMP%` makes the next start extract again] → accepted until dotnet/wpf#11842 is fixed; the extraction folder belongs to the same user, so it opens no new trust boundary.
- [STS ends with .NET 10] → ADR 0005 plans the move to .NET 12 LTS.
- [The owner's machine has only SDK 10.0.404] → `global.json` has asked for 11.0.100 since PR #27; README now names the .NET 11 SDK, which the desktop runs need.

## Migration Plan

Merging ships nothing; the next release tag packs the .NET 11 runtime and updates installed copies through Velopack as usual. Rollback is a revert of this change before that tag.
