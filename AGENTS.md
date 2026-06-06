# AGENTS.md — Pully run repo

This repo is **one run** of the Pully experiment. The authoritative spec lives **outside** this repo, in the
Pully spec kit (`github.com/bowenlowtjm/agents-game`, cloned locally at `SPEC_REPO` — see the run parameters).
Read `$SPEC_REPO/AGENT-BRIEF.md` first.

## What this project is
A Unity 6 (3D project, 2D-sprite gameplay) Android arcade game. Build it to `spec/GAME-SPEC.md` + `spec/RULESET.md`, to the bar in `spec/ACCEPTANCE.md`.

## Roles (native Hermes role-agents)
Orchestrator drives a Think→Plan→Build→Review→Test→Ship→Reflect loop over these agents (defined in the spec `roles/` + harness config): `game-pm`, `game-art`, game-logic, unity-scene, build/ci, test/qa. Parallel work runs in separate `git worktree`s; orchestrator merges green branches only. **No Claude Code / gstack.**

## Conventions (enforce)
- All game code+assets under `Assets/_Game/`. One asmdef: `Pully.Game`.
- Ruleset is **data-driven** via `RulesetDefinition` ScriptableObject — never hardcode the mapping.
- Tests under `Assets/Tests/` (EditMode unit + PlayMode integration). Add tests with each feature; seeded RNG → deterministic sessions. A passing sample of each ships in the scaffold.
- CI: `ci.yml` runs tests on every PR — **merge only when green**. `build.yml` produces the Android APK artifact on `main` (GameCI builder). `Editor/Builder.cs` is for *local* batchmode only (→ `Builds/Android/pully.apk`); don't use it as GameCI `buildMethod`.
- Every significant change (incl. CI status): append `docs/run-log.md`, post to Discord per rung, link the `SAA-###` issue.

## Memory
- **Solo config:** `docs/decisions.md` (ADR-lite), `docs/CONVENTIONS.md`, `docs/GOTCHAS.md`, `docs/run-log.md`.
- **Team config:** OpenViking (`viking://`) as shared project memory; `docs/` still holds human-readable run-log + gotchas.
- `DESIGN.md` = art taste memory (palette/style). Game Art reads it before generating.

## Hard rules
- Verify with artifacts (clean console / passing test / APK path). No silent stubs, no fake "Done".
- Respect the autonomy rung in the run parameters.
- Never edit the external spec folder — it's read-only.
