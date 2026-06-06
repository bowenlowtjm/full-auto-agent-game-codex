# run-log.md

Append one entry per significant change during the run, and a final summary entry at the end. Mirror the same line into the experiment results table (spec `07-Metrics`).

## Final summary template
```
Run: <RUN_ID>            e.g. pully-B-L3-20260604
Config / Rung / Memory: <A|B> / <L1|L3|L4> / <flat-docs|OpenViking>
Models: orch=<…> workers=<…>
Linear: SAA epic #__, <x>/<y> issues Done
Outcome: APK <built/blocked> · gestures <ok/partial> · art atlas <ok/—>
Gates passed: <n>/9 (see spec/ACCEPTANCE.md)
Code quality: __/15    Gameplay quality: __/10  (latency __ms, __ softlocks)
Human interventions: <count + type>
Time: __h__m    Tokens: ~__ (per-role if team)
Bottleneck: <what cost the most retries>
New gotchas: <promoted to GOTCHAS.md>
Self-report (honest): <what's done / stubbed / known issues>
```

## Running log
- 2026-06-06T11:12:04Z — scaffold copied from spec kit, Unity project initialized, core gameplay scripts + bootstrap + scene setup tool added, compile reached clean state via batch refresh; test runner invocation currently exits 0 but does not emit xml in this local setup (issue n/a, commit pending)
