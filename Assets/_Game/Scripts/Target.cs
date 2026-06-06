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
        private SpriteRenderer _sr;
        private float _baseScale;
        private float _pulseSeed;

        public void Initialize(RulesetDefinition.TargetRule rule, float lifetime, Action<Target> onExpired)
        {
            Rule = rule;
            _lifetime = lifetime;
            _onExpired = onExpired;
            SpawnTime = Time.time;
            Resolved = false;

            _sr = GetComponent<SpriteRenderer>();
            _sr.color = rule.color;
            _sr.sprite = ShapeSpriteFactory.GetSprite(rule.shape);

            var c = GetComponent<CircleCollider2D>();
            c.radius = 0.45f;

            _baseScale = UnityEngine.Random.Range(0.9f, 1.1f);
            _pulseSeed = UnityEngine.Random.Range(0f, 10f);
            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (Resolved) return;

            float age = Time.time - SpawnTime;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, _lifetime));

            // Spawn pop + subtle pulse (juice)
            float pop = Mathf.Clamp01(age * 8f);
            float pulse = 1f + Mathf.Sin((Time.time + _pulseSeed) * 7f) * 0.05f;
            transform.localScale = Vector3.one * (_baseScale * pop * pulse);
            transform.Rotate(0f, 0f, 45f * Time.deltaTime);

            // Fade near expiry as warning cue
            var c = Rule.color;
            float alpha = t > 0.75f ? Mathf.Lerp(1f, 0.35f, (t - 0.75f) / 0.25f) : 1f;
            _sr.color = new Color(c.r, c.g, c.b, alpha);

            if (age >= _lifetime)
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
