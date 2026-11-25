# Roadmap

Implementation order of the OpenSpec changes in `openspec/changes/`. Apply them in this order with `/opsx:apply <change>`; each one lists its dependencies in `proposal.md`.

| # | Change | Capability | Depends on |
|---|---|---|---|
| 1 | `bootstrap-solution` | app-lifecycle | — |
| 2 | `add-language-profiles` | language-profiles | 1 |
| 3 | `add-language-scorer` | wrong-layout-detection | 2 |
| 4 | `add-keyboard-capture-and-layouts` | keyboard-capture, layout-management | 1, 3 |
| 5 | `add-text-replacement` | text-replacement | 4 |
| 6 | `add-auto-correction` | auto-correction | 3, 4, 5 |
| 7 | `add-manual-correction` | manual-correction | 5 |
| 8 | `add-word-learning` | word-learning | 3, 5, 7 |
| 9 | `add-tray-and-settings-ui` | tray-and-settings | 6, 7, 8 |
| 10 | `add-statistics-and-diagnostics` | statistics-and-diagnostics | 6, 9 |
| 11 | `add-auto-update-and-installer` | auto-update | 1, 9 |
| 12 | `add-onboarding` | onboarding | 9, 11 |
| 13 | `add-telemetry` | telemetry | 10, 12 |
| 14 | `add-ui-localization` | localization | 9–13 |
| 15 | `add-final-acceptance` | — (by-hand acceptance and fixes) | 1–14 |

Changes 2–3 (pure logic) and 4 (Windows interop) can be built in parallel after 1. Changes 10 and 11 can run in parallel after 9. Onboarding comes after the installer because its "install properly" offer and its installed-location check need the Velopack install root.

Tooling changes sit outside the numbered order: they change how agents work on the repo, not the product, so the orchestrator may run them alongside the product changes.

| # | Change | Capability | Depends on |
|---|---|---|---|
| T1 | `add-agent-dev-tooling` | agent-dev-tooling (Graphify, ponytail, curated agent-skills, security-audit review step) | — |

Platform changes move the runtime or SDK. They run between product changes, when no other change is open, because they touch every project.

| # | Change | Capability | Depends on |
|---|---|---|---|
| P1 | `upgrade-to-dotnet-11` | keyboard-capture (input times survive sleep) | 1–15 |

Design changes reshape the look and feel after the product is complete.

| # | Change | Capability | Depends on |
|---|---|---|---|
| D1 | `redesign-visual-identity` | tray-and-settings, onboarding, app-lifecycle (new mark, palette, quick panel, one-page settings, splash) | 1–15, P1 |
| D2 | `fix-redesign-findings` | tray-and-settings (theme following, accent, fonts, licences, quick panel Language key) | D1 |

Installer changes replace Velopack's one-click setup with Navpaky's own wizard (owner decision, 2026-11-24; approved prototype v3.1). They run in order, one at a time.

| # | Change | Capability | Depends on |
|---|---|---|---|
| I1 | `add-install-wizard` | install-wizard (new), auto-update, onboarding, telemetry: branded per-user wizard (language, licence, folder, shortcut, sign-in, Welcome back / Start fresh), hand-off of choices to the install hook, slimmer Welcome with one telemetry consent, single signed `NavpakySetup.exe` | D2 |
| I2 | `add-machine-wide-install` | install-wizard, auto-update: "All users" into Program Files, with the 2026-11-24 live-run workarounds (HKLM entry and elevating uninstall, shared Start-menu link, per-user autostart through Active Setup, no apply-on-start, closing Navpaky in every session before an update, cleanup of the per-admin Installed apps entry) | I1 |

Initiative after I2, **analysis pending** (owner, 2026-11-24): **Portable and use without admin rights**. The goal: a user can run Navpaky on a PC they don't administer, such as a corporate PC, without asking IT. It has no OpenSpec change yet; changes are proposed once the owner settles the open choice (portable data in the profile or next to the exe).

| # | Item | Scope | Depends on |
|---|---|---|---|
| V1 | Portable zip | Run without installing (Velopack's portable package): data in the profile or next to the exe (open), no autostart or uninstall entry by default, update behaviour, "remove data from this PC" | I1 |
| V2 | Folder build | Publish a self-contained folder instead of a single file, so nothing is extracted to `%TEMP%` at start | — |
| V3 | Code signing | A real code-signing certificate for every executable, including the wizard | — |
| V4 | Blocked by policy | A clear "blocked by your organization's policy, contact IT" message where Navpaky can detect it, and an IT page explaining how to allow Navpaky by publisher | V3 |

Out of scope: any workaround that bypasses application control (AppLocker/WDAC), removable-media blocking or EDR/antivirus, and IT deployment tooling (Intune/SCCM/GPO, ADMX policies, silent IT installs).

T1 can run in parallel with changes 12–15: it touches no file under `src/`, `tests/` or `tools/Navpaky.DesktopChecks/`. Its global installs reach only sessions started after them, so implementers already at work keep their tools until their change ends.

By-hand checks are not a merge gate for changes 1–14: each change appends them to `docs/manual-tests/deferred-by-hand.md`, and change 15 is the owner's single pass over that file plus the fixes for what fails (owner decision, 2026-11-14).

Sources: the owner's reverse-engineering notes (kept outside the repository), `GLOSSARY.md` (terms), `docs/adr/` (decisions).
