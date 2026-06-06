using NUnit.Framework;
using Pully.Game;
using UnityEngine;

namespace Pully.Tests
{
    public class ScoreCalculatorTests
    {
        [Test]
        public void NextCombo_AppliesStep() => Assert.AreEqual(1.1f, ScoreCalculator.NextCombo(1f, 1.1f, 5f), 1e-4f);

        [Test]
        public void NextCombo_ClampsToCap() => Assert.AreEqual(5f, ScoreCalculator.NextCombo(4.8f, 1.1f, 5f), 1e-4f);

        [TestCase(1, 1f, 1)]
        [TestCase(8, 1.1f, 9)]
        [TestCase(5, 5f, 25)]
        public void ScoreFor_RoundsToNearest(int baseReward, float combo, int expected)
            => Assert.AreEqual(expected, ScoreCalculator.ScoreFor(baseReward, combo));

        [Test]
        public void RulesetFactory_HasAllFiveMappings()
        {
            var rs = RulesetFactory.CreateDefault();
            Assert.AreEqual(5, rs.rules.Count);
            Assert.AreEqual(RulesetDefinition.Gesture.SingleTap, Find(rs, RulesetDefinition.Shape.Circle, Color.green).requiredGesture);
            Assert.AreEqual(RulesetDefinition.Gesture.LongPress, Find(rs, RulesetDefinition.Shape.Circle, Color.red).requiredGesture);
            Assert.AreEqual(RulesetDefinition.Gesture.DoubleTap, Find(rs, RulesetDefinition.Shape.Square, Color.blue).requiredGesture);
        }

        private static RulesetDefinition.TargetRule Find(RulesetDefinition rs, RulesetDefinition.Shape shape, Color color)
        {
            foreach (var r in rs.rules)
            {
                if (r.shape == shape && Approximately(r.color, color)) return r;
            }
            Assert.Fail($"Rule missing: {shape} {color}");
            return default;
        }

        private static bool Approximately(Color a, Color b)
            => Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
    }
}
