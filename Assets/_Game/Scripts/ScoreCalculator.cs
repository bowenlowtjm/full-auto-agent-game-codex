using UnityEngine;

namespace Pully.Game
{
    public static class ScoreCalculator
    {
        public const float StartingCombo = 1f;

        public static float NextCombo(float currentCombo, float step, float cap)
            => Mathf.Min(currentCombo * step, cap);

        public static int ScoreFor(int baseReward, float combo)
            => Mathf.RoundToInt(baseReward * combo);

        public static float ResetCombo() => StartingCombo;
    }
}
