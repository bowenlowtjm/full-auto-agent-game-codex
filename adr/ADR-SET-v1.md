# ADR: Pully v1 Architecture Decisions

This ADR set records the highest-impact architecture choices made while taking the project from scaffold to a playable, autonomous-buildable Unity mobile game prototype.

**Key decision:** We chose a code-first runtime bootstrap architecture (instead of inspector/prefab-heavy authoring) so an autonomous agent can produce a fully runnable game loop without manual Unity Editor wiring. This rejected the more conventional prefab+serialized-reference workflow because that workflow has lower runtime flexibility and better artist tooling, but it requires human editor actions that block autonomous execution. The trade-off is explicit: faster autonomous delivery and deterministic setup at the cost of less designer-friendly scene authoring and more code responsibility for composition.

Canonical term used in this document: **Ruleset Contract** = the gameplay mapping and tuning values carried by `RulesetDefinition` (`shape + color -> gesture`, rewards, combo, lives, timer, spawn thresholds, seed).

## ADR001: Use runtime bootstrap composition instead of inspector-wired scene dependencies

### Status

Accepted

### Context

Pully is built under autonomous-run constraints: game setup must be reproducible from repository state and callable via batchmode. Manual drag-and-drop scene wiring is not reliably available in this workflow. The architecture therefore needed a deterministic initialization path that does not depend on inspector-populated references.

The project has one game runtime assembly (`Assets/_Game/Scripts/Pully.Game.asmdef`) and scene generation/editing helpers in `Assets/Editor/`. We needed a setup model that keeps scene startup stable while allowing repeated agent edits and batch compile checks.

### Considered Options

| Option | Pros | Cons | Impact |
|--------|------|------|--------|
| Inspector/prefab-driven wiring | Familiar Unity workflow; better for artist/designer iteration; fewer bootstrap lines in code | Requires manual editor actions to maintain references; brittle for autonomous branch generation and reset/scaffold flows | Would require stable prefab assets + serialized fields across `GameManager`, `TargetSpawner`, `GestureRecognizer`, menu/game-over controllers, and scene files (`Assets/_Game/Scenes/*.unity`) with manual linking each run. `Assets/Editor/GameSetup.cs` would need to instantiate and wire prefab references rather than only creating root objects. |
| **Runtime bootstrap composition (chosen)** | Deterministic startup from code; works with batch-mode-first agent workflow; no hidden scene state required | More initialization logic in code; harder for non-technical content creators to tweak in inspector | `Assets/_Game/Scripts/GameBootstrap.cs` creates `GameSystems`, adds `GameManager`, `TargetSpawner`, `GestureRecognizer`, and calls `Initialize(...)` methods + `GameManager.BindSpawner(...)`. `Assets/Editor/GameSetup.cs` builds scenes with minimal root objects only. Scene dependency graph is explicit in code, not serialized references. |

### Decision

We compose core runtime dependencies in `GameBootstrap.Awake()`:

- Ensure main camera exists
- Create runtime ruleset (if not assigned)
- Create runtime systems and bind dependencies

Code path (actual methods):
- `GameManager.Initialize(RulesetDefinition rs)`
- `TargetSpawner.Initialize(RulesetDefinition rs, Camera cam, Transform container)`
- `GameManager.BindSpawner(TargetSpawner targetSpawner)`
- `GestureRecognizer.Initialize(RulesetDefinition rs, TargetSpawner ts, GameManager gm)`

This ensures a fresh clone with scene setup can run without inspector intervention.

### Consequences

- Future scene/system additions must expose explicit initialization APIs (constructor-like `Initialize` pattern), not assume serialized references exist.
- Runtime composition is now a core integration seam; regressions here have high blast radius and should be covered by smoke/integration checks.

## ADR002: Define gameplay through a single Ruleset Contract with deterministic spawning

### Status

Accepted

### Context

The core game identity depends on strict gesture mapping and predictable scoring behavior. We needed a single source of truth for gameplay parameters and a deterministic spawn model to support replayability, balancing, and testability.

Following ADR001, runtime systems can be created from code, so gameplay configuration also needed a code-constructible default path for autonomous runs where no manually authored ScriptableObject asset is guaranteed.

### Considered Options

| Option | Pros | Cons | Impact |
|--------|------|------|--------|
| Hardcode rules in each subsystem (`GameManager`, `GestureRecognizer`, `TargetSpawner`) | Fast to write initially; no extra config object | High drift risk across systems; tuning requires code edits in multiple files; weak testability | Would spread mapping/tuning across multiple methods in `GameManager.cs`, `GestureRecognizer.cs`, `TargetSpawner.cs`, and tests. Any mapping change would touch several files and call sites. |
| External JSON-only rules file | Designer-friendly data updates outside compile cycle | Extra file I/O/parsing path and runtime error surface; duplicate type definitions or conversion logic needed | Would add parser/validation layer and bootstrap file-loading flow in `GameBootstrap.cs`; tests would need fixtures and parse validation in addition to gameplay tests. |
| **Single ScriptableObject Ruleset Contract + code default factory (chosen)** | Centralized schema and tuning; Unity-native type safety; deterministic defaults available in code | Requires maintaining both schema (`RulesetDefinition`) and factory defaults (`RulesetFactory`) in sync | `Assets/_Game/Scripts/RulesetDefinition.cs` defines contract; `RulesetFactory.CreateDefault()` populates exact mappings and thresholds; `TargetSpawner` uses seeded `System.Random`; `GameManager` and `GestureRecognizer` consume same contract fields. Tests reference shared contract types in `Assets/Tests/EditMode/ScoreCalculatorTests.cs`. |

### Decision

We use `RulesetDefinition` as the canonical Ruleset Contract and `RulesetFactory.CreateDefault()` as the autonomous-safe default provider.

Determinism details:
- Seed source: `RulesetDefinition.seed`
- RNG implementation: `new System.Random(ruleset.seed)` in `TargetSpawner.Initialize(...)`
- Spawn timing interpolation: `Mathf.Lerp(spawnIntervalStart, spawnIntervalEnd, t)`

Contract fields include (actual names):
- `rules` (`List<TargetRule>`)
- `comboStep`, `comboCap`
- `lives`, `roundSeconds`, `targetLifetime`
- `spawnIntervalStart`, `spawnIntervalEnd`, `maxConcurrentTargets`
- `doubleTapWindow`, `longPressDuration`, `swipeMinDistance`
- `seed`

### Consequences

- Future balance patches should prefer data-only changes against the Ruleset Contract, reducing logic churn risk.
- Any future authored Ruleset asset pipeline must preserve parity with `RulesetFactory` or explicitly replace it to avoid hidden behavior divergence.

## ADR003: Keep full mobile gesture vocabulary and add editor modifier emulation

### Status

Accepted

### Context

The game spec requires 5 gestures, but editor testing is mouse/keyboard-driven. A naive implementation makes desktop verification painful and causes false negatives (players think scoring is broken when gesture mismatch occurs).

Following ADR002, gesture thresholds and expectations are part of the Ruleset Contract. We needed an editor interaction model that preserves spec fidelity while staying testable on desktop.

### Considered Options

| Option | Pros | Cons | Impact |
|--------|------|------|--------|
| Reduce desktop mode to tap-only gameplay | Simplifies editor controls and onboarding | Breaks parity with target mobile design; invalidates high-value rules/gestures and scoring behavior | Would require alternate rule path in `GestureRecognizer.cs` and likely conditional logic in `GameManager.cs`/UI copy; tests for non-tap gestures become meaningless. |
| Separate desktop ruleset and mobile ruleset | Keeps all gestures on mobile while simplifying editor | Two balancing surfaces and drift risk; debugging behavior mismatch becomes harder | Would require dual ruleset selection in `GameBootstrap.cs`, duplicate tuning in `RulesetFactory.cs`, and conditional UX copy in menu/HUD. |
| **Single gesture system + editor modifier mapping (chosen)** | Preserves gameplay parity; enables fast local testing of all gestures; minimizes rules drift | Desktop controls are less intuitive than touch-native gestures and need explicit UI hints | `Assets/_Game/Scripts/GestureRecognizer.cs` maps editor input: click=SingleTap, Ctrl=DoubleTap, Shift=LongPress, Alt=SwipeTap, Cmd=TwoFingerTap; touch path still uses `EnhancedTouchSupport`. `GameManager.OnGUI()` and `MenuController.OnGUI()` include control hints. |

### Decision

We preserved one gesture vocabulary across platforms and implemented editor emulation in `GestureRecognizer.HandleMouse()` using `Keyboard.current` modifiers.

Core mapping (actual logic):
- `if (shift) -> LongPress`
- `else if (alt) -> SwipeTap`
- `else if (cmd) -> TwoFingerTap`
- `else if (ctrl || doubleTapWindow check) -> DoubleTap`
- `else -> SingleTap`

Touch path remains in `HandleTouches()` with `EnhancedTouchSupport.Enable()` and timing/distance thresholds from Ruleset Contract.

### Consequences

- Any future control tutorial/accessibility feature must account for both touch-native semantics and editor emulation semantics.
- Gesture additions now require updating both touch recognition and modifier mapping to keep parity.

## ADR004: Standardize autonomous compile validation via Unity CLI refresh + watcher artifacts

### Status

Accepted

### Context

Unity often compiles on focus regain in GUI workflows, which is unreliable for autonomous iteration. We needed a repeatable, machine-readable compile-check mechanism after file edits, including clear handling for project lock errors when another Unity instance is open.

No schema/database migration is involved in this ADR; migration mechanics are not applicable.

### Considered Options

| Option | Pros | Cons | Impact |
|--------|------|------|--------|
| Manual compile checks by opening Unity and visually reading console | Zero extra tooling | Not automatable; hard to gate commits; no machine-readable status | Would keep compile verification outside repo scripts; no `TestResults/auto-compile-*.{json,log}` artifacts; higher risk of pushing unnoticed compile breaks. |
| In-editor HTTP/MCP trigger service | Supports GUI-open workflows and lower startup overhead | Extra always-on editor extension complexity and lifecycle management | Would require additional editor server script, request handling, and security/process management not currently present in `Assets/Editor/`. |
| **CLI refresh entrypoint + repo watcher script (chosen)** | Deterministic and scriptable; emits parseable status/history; integrates with batch workflow | Requires spawning Unity process; project-lock handling needed if GUI instance already open | `Assets/Editor/AutoRefresher.cs` provides `AutoRefresher.RefreshAssets()`. `scripts/unity_auto_compile_watch.py` runs `Unity -executeMethod AutoRefresher.RefreshAssets -batchmode -quit -logFile ...`, parses errors, writes `TestResults/auto-compile-status.json` and `TestResults/auto-compile-history.log`, and supports `--close-unity-if-open` using `pkill -f Unity`. |

### Decision

We adopted a two-part validation mechanism:
1. Unity editor method entrypoint: `AutoRefresher.RefreshAssets()`.
2. Python watcher/runner that triggers compile and persists machine-readable output.

Artifacts and paths (actual):
- Status: `TestResults/auto-compile-status.json`
- History: `TestResults/auto-compile-history.log`
- Raw Unity log: `/tmp/unity_auto_compile.log`

Error patterns explicitly include:
- `error CS####`
- `Scripts have compiler errors`
- `another Unity instance is running`
- `Fatal Error!`

### Consequences

- Future CI/local automation can consume compile status as structured data, enabling stricter quality gates.
- If the team later prefers GUI-persistent workflows, this ADR sets a baseline that any new in-editor trigger must match in observability and failure semantics.
