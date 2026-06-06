# GOTCHAS.md — accumulated Unity/agent traps

Read before each phase. Append any new trap + its fix so the next run (and the next phase) avoids it. Seeded with common Unity-agent pitfalls; confirm/replace with what you actually hit.

## Seeded (verify in your setup)
- **.meta files:** every asset has a `.meta` with a GUID. Don't delete/regenerate casually — it breaks references. Commit them.
- **Scene wiring via MCP:** assigning serialized references through the Editor bridge often needs the object to exist + scene saved first; expect retries.
- **Input System:** project must be set to the new Input System (Player Settings) or touch reads return nothing.
- **Batchmode build:** no enabled scenes in Build Settings → silent empty build. `Builder.cs` exits 2 to catch this.
- **Android module:** missing SDK/NDK/JDK → build fails late. Verify in M0.
- **Sprite import:** pixel art blurs unless filter mode = Point and compression = None.
- **Determinism:** any `Time.deltaTime`-driven spawn without a fixed/seeded step breaks replay. Drive spawns from the seeded sequence.
- **GameCI + Builder.cs:** don't pass `Builder.BuildAndroid` as GameCI `buildMethod` — its `EditorApplication.Exit` kills GameCI's flow. CI uses GameCI's default builder (→ `build/Android`); `Builder.cs` is local-only (→ `Builds/Android`).
- **Unity license in CI:** `ci.yml`/`build.yml` need `UNITY_LICENSE`+`UNITY_EMAIL`+`UNITY_PASSWORD` secrets; missing → CI fails at activation, not at your code.
- **Test asmdefs:** test assemblies need `UNITY_INCLUDE_TESTS` define constraint + `nunit.framework.dll` precompiled ref, else they compile into builds or fail to find NUnit.
- **PlayMode tests + Input System:** simulate input via `InputTestFixture` (Unity.InputSystem.TestFramework), not real devices.

## Discovered this experiment
- 2026-06-06 — Fresh run repo may not include a Unity project skeleton (`Packages/`, `ProjectSettings/`) even after template copy → open once in Unity and let project metadata generate before expecting batch workflows.
- 2026-06-06 — Unity test runner can exit 0 yet not emit `-testResults` XML in some local batch invocations → verify artifact path explicitly and avoid over-claiming pass counts.
- 2026-06-06 — `EnhancedTouch.Touch` clashes with `UnityEngine.Touch` namespace in mixed imports → alias the EnhancedTouch type (`using ETouch = ...`) to avoid CS0104.
