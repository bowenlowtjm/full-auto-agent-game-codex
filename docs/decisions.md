# decisions.md — ADR-lite

One short entry per non-obvious decision (architecture, ruleset tuning, art direction, build/signing). Required at L3/L4 (the human isn't there to ask).

## Template
```
### <date> — <decision title>
- Context: <what forced a choice>
- Options: <a / b / c>
- Decision: <chosen> by <role/rung>
- Rationale: <why>
- Reversible? <yes/no; how>
```

## Decisions
### 2026-06-06 — Local bootstrap-first approach before art pass
- Context: Repo was empty except git metadata; needed a runnable baseline quickly from AGENT-BRIEF while preserving deterministic ruleset behavior.
- Options: (a) full prefab/UI art pipeline first, (b) bootstrap runtime primitives + minimal GUI first, then art/juice.
- Decision: (b) chosen by agent (L3-style no human-in-loop for open implementation details).
- Rationale: Fastest path to a playable, testable core loop and compile verification; aligns with fail-honest progress and can be replaced incrementally.
- Reversible? Yes; replace runtime primitives/OnGUI with prefab+atlas/UI Toolkit later without changing ruleset contracts.
