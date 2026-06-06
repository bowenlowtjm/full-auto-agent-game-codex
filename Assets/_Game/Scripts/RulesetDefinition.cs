using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pully.Game
{
    // Data-driven ruleset for Pully. Create an asset via:
    //   Assets > Create > Pully > Ruleset
    // Populate it from spec/RULESET.md. Gameplay code must read from this SO,
    // never hardcode the shape/color -> gesture mapping.
    [CreateAssetMenu(fileName = "Ruleset", menuName = "Pully/Ruleset")]
    public class RulesetDefinition : ScriptableObject
    {
        public enum Shape { Circle, Square, Triangle, Star }

        public enum Gesture { SingleTap, DoubleTap, LongPress, SwipeTap, TwoFingerTap }

        [Serializable]
        public struct TargetRule
        {
            public Shape shape;
            public Color color;
            public Gesture requiredGesture;
            public int baseReward;
        }

        [Header("Target mapping (see spec/RULESET.md)")]
        public List<TargetRule> rules = new List<TargetRule>();

        [Header("Combo & scoring")]
        public float comboStep = 1.1f;
        public float comboCap = 5f;

        [Header("Round / session")]
        public int lives = 3;
        public float roundSeconds = 60f;
        public float targetLifetime = 1.6f;
        public float spawnIntervalStart = 1.2f;
        public float spawnIntervalEnd = 0.6f;
        public int maxConcurrentTargets = 4;

        [Header("Gesture thresholds")]
        public float doubleTapWindow = 0.30f;   // seconds
        public float longPressDuration = 0.50f; // seconds
        public float swipeMinDistance = 50f;    // pixels

        [Header("Determinism")]
        public int seed = 12345;
    }
}
