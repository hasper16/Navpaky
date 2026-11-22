# Desktop checks: redesign-visual-identity

The redesign changes the welcome window (three steps), the tray (3D keycap, left click opens the quick panel), the cursor indicator (styles), the switch sounds and the settings window (one page), and adds the launch splash. The changed sets below gate the merge; everything that needs eyes is in the `redesign-visual-identity` section of [deferred-by-hand.md](deferred-by-hand.md).

Needs the English (US) and Russian Layouts installed and a Debug build. Close every running Navpaky and input probe, then run from the repository root. Do not touch the mouse or keyboard while a set runs.

## Automated: `tools/Navpaky.DesktopChecks`

- [ ] `onboarding`: `dotnet build && dotnet run --project tools/Navpaky.DesktopChecks -- onboarding` (the set now presses Next to reach the try-it field and Done).
- [ ] `tray-and-settings`: `dotnet run --project tools/Navpaky.DesktopChecks -- tray-and-settings` (cursor indicator, one-page settings, switch sound).
- [ ] Default set: `dotnet run --project tools/Navpaky.DesktopChecks` (the launch splash shows at every start and must not take focus from the checks).
- [ ] The input-language list is the same before and after the runs.
