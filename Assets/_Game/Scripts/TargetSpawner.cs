using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pully.Game
{
    public class TargetSpawner : MonoBehaviour
    {
        [SerializeField] private RulesetDefinition ruleset;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform targetContainer;

        private System.Random _random;
        private float _nextSpawnAt;
        private float _roundStart;

        public readonly List<Target> ActiveTargets = new();

        public void Initialize(RulesetDefinition rs, Camera cam, Transform container)
        {
            ruleset = rs;
            gameCamera = cam;
            targetContainer = container;
            _random = new System.Random(ruleset.seed);
            _roundStart = Time.time;
            _nextSpawnAt = Time.time + ruleset.spawnIntervalStart;
        }

        private void Update()
        {
            if (ruleset == null || gameCamera == null || targetContainer == null) return;
            ActiveTargets.RemoveAll(t => t == null);
            if (ActiveTargets.Count >= ruleset.maxConcurrentTargets) return;
            if (Time.time < _nextSpawnAt) return;

            SpawnOne();
            _nextSpawnAt = Time.time + CurrentInterval();
        }

        private float CurrentInterval()
        {
            float t = Mathf.Clamp01((Time.time - _roundStart) / Mathf.Max(0.01f, ruleset.roundSeconds));
            return Mathf.Lerp(ruleset.spawnIntervalStart, ruleset.spawnIntervalEnd, t);
        }

        private void SpawnOne()
        {
            if (ruleset.rules.Count == 0) return;
            var rule = ruleset.rules[_random.Next(ruleset.rules.Count)];
            var go = new GameObject($"Target-{rule.shape}-{rule.requiredGesture}");
            go.transform.SetParent(targetContainer, false);

            var vp = new Vector3((float)_random.NextDouble() * 0.8f + 0.1f, (float)_random.NextDouble() * 0.7f + 0.15f, 10f);
            go.transform.position = gameCamera.ViewportToWorldPoint(vp);

            var target = go.AddComponent<Target>();
            target.Initialize(rule, ruleset.targetLifetime, OnTargetExpired);
            ActiveTargets.Add(target);
        }

        private void OnTargetExpired(Target target)
        {
            ActiveTargets.Remove(target);
            var gm = GetComponent<GameManager>();
            gm?.OnTargetExpired(target);
        }

        public Target FindTargetAtScreenPos(Vector2 screenPos)
        {
            if (gameCamera == null) return null;
            var world = gameCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            var hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
            return hit == null ? null : hit.GetComponent<Target>();
        }
    }
}
