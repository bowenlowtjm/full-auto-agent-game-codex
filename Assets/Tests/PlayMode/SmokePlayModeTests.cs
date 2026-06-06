using System.Collections;
using NUnit.Framework;
using Pully.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pully.Tests
{
    public class SmokePlayModeTests
    {
        [UnityTest]
        public IEnumerator GameManager_CorrectGesture_IncreasesScore()
        {
            var root = new GameObject("root");
            var gm = root.AddComponent<GameManager>();
            var rs = RulesetFactory.CreateDefault();
            gm.Initialize(rs);

            var go = new GameObject("t");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<CircleCollider2D>();
            var t = go.AddComponent<Target>();
            t.Initialize(rs.rules[0], 10f, _ => { });

            gm.OnGesturePerformed(t, rs.rules[0].requiredGesture);
            yield return null;

            Assert.Greater(gm.Score, 0);
            Object.Destroy(root);
            Object.Destroy(go);
        }
    }
}
