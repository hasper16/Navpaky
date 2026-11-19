---
name: Navpaky
status: final
updated: 2026-11-19
colors:
  light:
    bg: "#F2F4F9"
    surface: "#FFFFFF"
    surface-2: "#F5F7FB"
    line: "#12283A1F"
    fg: "#111726"
    fg-2: "#4A5468"
    fg-3: "#7A8498"
    accent: "#C8102E"
    accent-2: "#17171C"
  dark:
    bg: "#090F1E"
    surface: "#131B2F"
    surface-2: "#18213A"
    line: "#96AAE624"
    fg: "#E8ECF6"
    fg-2: "#D2DAF2B8"
    fg-3: "#AFBADE7A"
    accent: "#4C86F0"
    accent-2: "#FACC15"
  state:
    working: "#22A06B"
    paused: "#C98A00"
    off: "#8C96A6"
    failed: "#E11D48"
typography:
  heading:
    family: "Grunt Grotesk"
    weights: [700, 300]
    fallback: "Segoe UI Variable Display"
  body:
    family: "Segoe UI Variable Text"
    fallback: "Segoe UI"
  legend:
    family: "Segoe UI Variable Display"
    weight: 600
rounded:
  window: 10
  card: 8
  control: 6
  pill: 999
spacing:
  unit: 4
  card-padding: [12, 16]
  section-gap: 24
components:
  keycap: "3D keycap from the icon set: rim, skirt, top face, front lip"
  cursor-indicator: "pill, tag or underline; pill is the default"
  quick-panel: "340 px flyout over the taskbar"
---

# Navpaky visual identity

The interactive prototype the owner approved on 2026-11-19 is the visual reference; this file
and `EXPERIENCE.md` win where they differ from it.

## Brand & Style

A calm Windows 11 utility with a Ukrainian accent carried by colour alone: red and black in the
light theme, blue and yellow (sapphire and gold) in the dark theme. No ornament, flags or
"made in" lines. The mood takes cues from the original app's site: deep surfaces, soft
translucency, large bold headings, rounded shapes.

## Colors

Tokens in the front matter. The palette follows the Windows app theme: light uses
`{colors.light.accent}` red with black, dark uses `{colors.dark.accent}` blue with
`{colors.dark.accent-2}` gold. State colours are the same in both themes. Body text keeps at
least 4.5:1 contrast on its surface in both themes.

## Typography

- Headings (window titles inside the content, section headers, the welcome greeting, the
  quick-panel language name): Grunt Grotesk Bold; Light for secondary display lines. Both
  styles are free under the SIL Open Font License; the font files ship with the app together
  with the licence text.
- Everything else: Segoe UI Variable, the system face.
- The tray legend and cursor indicator use Segoe UI Variable Display SemiBold so 16 px stays
  crisp.
- ZAKON and NAMU were evaluated and dropped (owner decision).

## Layout & Spacing

4 px grid. Settings: one scrolling page, a 230 px section list on the left, cards on the right.
Cards group related rows; rows are 12 × 16 px padded and separated by hairlines.

## Elevation & Depth

Windows use the system Mica backdrop. The quick panel and the cursor pill are translucent
(acrylic-like) with a soft shadow. Cards are flat with a hairline border.

## Shapes

Window 10 px, card 8 px, control 6 px, chips and the cursor pill fully round.

## Components

- **App mark.** Two 3D keycaps, Latin Q on the left and Й on the right (one physical key, owner
  decision), drawn as shapes like the icon set, in a dark squircle, joined by two
  arrows that run in opposite directions (top to the right, bottom to the left). Sizes up to
  32 px drop the arrows. One mark for both themes (sapphire and gold).
- **Tray keycap.** The 3D keycap from the icon set (rim, skirt, top face, front lip) with the
  Language code as legend. Active: white face, black skirt, red lip on a light taskbar;
  sapphire face, gold rim and lip on a dark taskbar. Paused amber, off grey at 45 % legend,
  stopped after failures crimson with a dot.
- **Cursor indicator.** Three styles: pill (translucent, theme-colour dot, default), tag
  (solid accent with a tail toward the caret), underline (accent letters, no background).
- **Quick panel.** Header with the current Layout as an accent keycap button and the language
  name; toggles; segmented chips for Sensitivity and Pause; a statistics strip; footer links.
- **Launch splash.** A borderless card on `{colors.light.surface}`/`{colors.dark.surface}` with a
  shadow; monospace word (Cascadia Mono), red with a wavy underline while wrong, green once
  corrected, then normal text; the Language pill in the top-right corner.
- **Chips.** Rounded; selected = accent outline (settings) or accent fill (segmented).

## Do's and Don'ts

- Do keep the tray legend in the system face; do keep 3D detail only where size allows.
- Don't use flags, tryzub, embroidery or slogans; don't copy the original's texts or assets.
