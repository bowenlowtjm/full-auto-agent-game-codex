using UnityEngine;

namespace Pully.Game
{
    public static class RulesetFactory
    {
        public static RulesetDefinition CreateDefault()
        {
            var rs = ScriptableObject.CreateInstance<RulesetDefinition>();
            rs.rules.Clear();

            rs.rules.Add(new RulesetDefinition.TargetRule
            {
                shape = RulesetDefinition.Shape.Circle,
                color = Color.green,
                requiredGesture = RulesetDefinition.Gesture.SingleTap,
                baseReward = 1
            });
            rs.rules.Add(new RulesetDefinition.TargetRule
            {
                shape = RulesetDefinition.Shape.Circle,
                color = Color.red,
                requiredGesture = RulesetDefinition.Gesture.LongPress,
                baseReward = 5
            });
            rs.rules.Add(new RulesetDefinition.TargetRule
            {
                shape = RulesetDefinition.Shape.Square,
                color = Color.blue,
                requiredGesture = RulesetDefinition.Gesture.DoubleTap,
                baseReward = 3
            });
            rs.rules.Add(new RulesetDefinition.TargetRule
            {
                shape = RulesetDefinition.Shape.Triangle,
                color = Color.yellow,
                requiredGesture = RulesetDefinition.Gesture.SwipeTap,
                baseReward = 5
            });
            rs.rules.Add(new RulesetDefinition.TargetRule
            {
                shape = RulesetDefinition.Shape.Star,
                color = new Color(0.6f, 0.2f, 0.8f, 1f),
                requiredGesture = RulesetDefinition.Gesture.TwoFingerTap,
                baseReward = 8
            });

            rs.comboStep = 1.1f;
            rs.comboCap = 5f;
            rs.lives = 3;
            rs.roundSeconds = 60f;
            rs.targetLifetime = 1.6f;
            rs.spawnIntervalStart = 1.2f;
            rs.spawnIntervalEnd = 0.6f;
            rs.maxConcurrentTargets = 4;
            rs.doubleTapWindow = 0.30f;
            rs.longPressDuration = 0.50f;
            rs.swipeMinDistance = 50f;
            rs.seed = 12345;
            return rs;
        }
    }
}
