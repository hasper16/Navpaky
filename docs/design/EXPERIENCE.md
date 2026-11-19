---
name: Navpaky
status: final
updated: 2026-11-19
---

# Navpaky experience

Visual tokens live in `DESIGN.md`; this file owns structure and behaviour.

## Foundation

Windows 11 desktop tray app, WPF with WPF UI. Surfaces: tray icon, tray menu (right click),
quick panel (left click), Settings window, Welcome window, cursor indicator, Statistics window.
UI languages English and Ukrainian.

## Information Architecture

- **Launch splash** (every start): a caret types "Navpaky" marked wrong, it turns into
  "Привіт"/"Hello" per UI language with a Language pill, then fades (about 3 s, no focus).

- **Tray icon**: state and current Language.
- **Quick panel** (left click): current Layout (click switches it), Auto Correction on/off,
  Sensitivity, Pause (15 min, 30 min, 1 h, 3 h, until Navpaky restarts), today's statistics
  strip (opens Statistics), switch sound on/off, cursor indicator on/off, Settings…,
  First steps, Quit, and a hint naming the Hotkey.
- **Tray menu** (right click): unchanged items; the welcome item is named "First steps"
  ("Перші кроки").
- **Settings**: one page with sections General, Correction, Hotkey, Apps and words,
  Diagnostics and privacy, About. The section list scrolls the page and highlights the section
  in view. General gains the cursor indicator style and the larger sound list.
- **Welcome**: three steps: what it does (with a live before/after example and Undo), try it,
  your choices (Auto Correction, Hotkey, launch at sign-in, keeping the tray icon visible,
  diagnostics). Back, Next, Done; closing at any step counts as completed.

## Voice and Tone

Plain, short, second person. Buttons say what happens. No slogans.

## Component Patterns

- Quick panel closes on Esc, on click outside and when it loses focus; it never steals focus
  from the app the user types in until clicked.
- Segmented chips select one value; the Pause chip that is active shows the pause; clicking it
  again resumes.
- Sound list: each entry previews on demand. Default sound: "Navpaky", which rises in pitch
  when the new Language is not English and falls when it is English.

## State Patterns

Working, paused (with time left), off, stopped after keyboard failures: same four states in the
tray icon, the quick-panel status line and the tooltip.

## Interaction Primitives

Left click tray = quick panel; right click = menu; Esc closes panels; Tab/arrow keys reach every
control.

## Accessibility Floor

Every control has an automation name; focus is always visible; colour is never the only signal
(state text accompanies state colour); motion respects the Windows animation setting.

## Key Flows

**Oksana, translator, switches between Ukrainian and English all day.** 1) She types a word in
the wrong Layout; Navpaky fixes it and the pill shows "UK" next to the caret with the rising
"Navpaky" sound. 2) Before a password field she left-clicks the tray keycap, picks Pause 15 min.
3) **Climax**: the quick panel's strip says 37 corrections today; she clicks it to see the
week. 4) Later she opens Settings, scrolls through one page, switches the indicator to the
underline style.
