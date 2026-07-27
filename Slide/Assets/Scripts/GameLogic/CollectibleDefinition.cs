using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Shared visual contract for collectables. Mission items are normalized by
    /// their largest visible dimension, so every item remains close to the coin
    /// without changing the source pixel art.
    /// </summary>
    public static class CollectibleDefinition
    {
        public const float CoinVisualDiameter = 0.56f;
        public const float MissionItemVisualDiameter = 0.62f;
        public const float MaxMissionItemVisualDiameter = CoinVisualDiameter * 1.25f;

        public static float GetMissionItemScale(Sprite sprite)
        {
            if (sprite == null)
                return 1f;

            var largestDimension = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            return largestDimension > 0f
                ? Mathf.Min(MissionItemVisualDiameter, MaxMissionItemVisualDiameter) / largestDimension
                : 1f;
        }

        public static float GetMissionItemColliderRadius(Sprite sprite, float visualScale)
        {
            if (sprite == null || visualScale <= 0f)
                return 0f;

            // The CircleCollider lives on the same pooled root as the renderer.
            // Counter-scale its local radius so its world-space size follows the
            // visible (shorter) item dimension rather than the source PNG bounds.
            var visibleRadius = Mathf.Min(sprite.bounds.size.x, sprite.bounds.size.y) * visualScale * 0.5f;
            return visibleRadius / visualScale;
        }
    }
}
