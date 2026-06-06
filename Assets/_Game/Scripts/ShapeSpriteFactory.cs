using System.Collections.Generic;
using UnityEngine;

namespace Pully.Game
{
    public static class ShapeSpriteFactory
    {
        private static readonly Dictionary<RulesetDefinition.Shape, Sprite> Cache = new();

        public static Sprite GetSprite(RulesetDefinition.Shape shape)
        {
            if (Cache.TryGetValue(shape, out var sprite)) return sprite;
            sprite = shape switch
            {
                RulesetDefinition.Shape.Circle => Build(64, IsCircle),
                RulesetDefinition.Shape.Square => Build(64, IsSquare),
                RulesetDefinition.Shape.Triangle => Build(64, IsTriangle),
                RulesetDefinition.Shape.Star => Build(64, IsStar),
                _ => Build(64, IsCircle)
            };
            Cache[shape] = sprite;
            return sprite;
        }

        private static Sprite Build(int size, System.Func<float, float, bool> inside)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x / (size - 1f)) * 2f - 1f;
                    float ny = (y / (size - 1f)) * 2f - 1f;
                    px[y * size + x] = inside(nx, ny) ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static bool IsCircle(float x, float y) => (x * x + y * y) <= 0.95f;
        private static bool IsSquare(float x, float y) => Mathf.Abs(x) <= 0.9f && Mathf.Abs(y) <= 0.9f;
        private static bool IsTriangle(float x, float y)
        {
            float yy = y + 0.85f;
            if (yy < 0f || yy > 1.7f) return false;
            float half = yy * 0.6f;
            return Mathf.Abs(x) <= half;
        }
        private static bool IsStar(float x, float y)
        {
            float r = Mathf.Sqrt(x * x + y * y);
            if (r > 0.98f) return false;
            float a = Mathf.Atan2(y, x);
            float wave = 0.55f + 0.2f * Mathf.Cos(5f * a);
            return r <= wave;
        }
    }
}
