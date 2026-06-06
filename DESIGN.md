# DESIGN.md — Pully art taste memory

The Game Art agent reads this before generating and updates it as the style locks. Goal: visual consistency across sprites, UI, and FX — and across runs.

## Art style
- **Style:** <set from run param ART_STYLE — flat-vector | pixel | …>   (L4: agent decides, then record here with a one-line rationale)
- **Mood:** <e.g. clean, playful, high-contrast arcade>
- **Pixels-per-unit / resolution target:** <e.g. 100 PPU; pixel art → point filter>

## Palette (lock these hex values)
| Role | Hex | Used for |
|------|-----|----------|
| Background | #______ | play-field |
| Accent / UI | #______ | buttons, HUD |
| Circle-Green target | #______ | single-tap |
| Circle-Red target | #______ | long-press trap |
| Square-Blue target | #______ | double-tap |
| Triangle-Yellow target | #______ | swipe-tap |
| Star-Purple target | #______ | two-finger |

## Readability rules (these are scored — see spec/ACCEPTANCE.md)
- Each shape distinguishable by **silhouette AND color** — never color alone (red/green colorblind safe).
- Consistent stroke/weight across all targets.
- HUD legible over the busiest play-field state.

## Asset inventory (check off as produced)
- [ ] 5 target sprites (one per shape/color in spec/RULESET.md)
- [ ] Hit-pop FX, miss-flash FX
- [ ] UI: Play / Retry / Menu buttons, HUD frame
- [ ] Packed atlas (no import warnings)

## Decisions log (style)
- <date> — <what was chosen and why>
