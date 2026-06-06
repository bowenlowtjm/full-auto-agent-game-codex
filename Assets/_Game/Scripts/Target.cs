using System;
using UnityEngine;

namespace Pully.Game
{
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
    public class Target : MonoBehaviour
    {
        public RulesetDefinition.TargetRule Rule { get; private set; }
        public float SpawnTime { get; private set; }
        public bool Resolved { get; private set; }

        private float _lifetime;
        private Action<Target> _onExpired;

        public void Initialize(RulesetDefinition.TargetRule rule, float lifetime, Action<Target> onExpired)
        {
            Rule = rule;
            _lifetime = lifetime;
            _onExpired = onExpired;
            SpawnTime = Time.time;
            Resolved = false;

            var sr = GetComponent<SpriteRenderer>();
            sr.color = rule.color;
            sr.sprite = ShapeSpriteFactory.GetSprite(rule.shape);

            var c = GetComponent<CircleCollider2D>();
            c.radius = 0.45f;
        }

        private void Update()
        {
            if (!Resolved && Time.time - SpawnTime >= _lifetime)
            {
                Expire();
            }
        }

        public void Resolve()
        {
            if (Resolved) return;
            Resolved = true;
            Destroy(gameObject);
        }

        private void Expire()
        {
            if (Resolved) return;
            Resolved = true;
            _onExpired?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
