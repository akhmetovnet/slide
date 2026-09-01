using System;
using UnityEngine;

namespace GameLogic
{
    [CreateAssetMenu(fileName = "LocationConfig", menuName = "Slide/Location Config")]
    public sealed class LocationConfig : ScriptableObject
    {
        public ChallengeLocation Location;
        [Min(1)] public int FirstLevel = 1;
        [Min(1)] public int LastLevel = 10;
        public string ResourceRoot;
        public LocationEnvironmentLayer[] EnvironmentLayers = Array.Empty<LocationEnvironmentLayer>();
        public LocationStartAreaVisuals StartArea = new LocationStartAreaVisuals();
        public LocationPlatformVisuals Platforms = new LocationPlatformVisuals();
        public LocationHazardVisuals HazardVisuals = new LocationHazardVisuals();
        public LocationHazardSettings Hazards = new LocationHazardSettings();
        public LocationDifficultyStep[] Difficulty = Array.Empty<LocationDifficultyStep>();
        public Sprite MissionMenuBackground;

        public int LocalLevel(int level)
        {
            return Mathf.Clamp(level - FirstLevel + 1, 1, Mathf.Max(1, LastLevel - FirstLevel + 1));
        }

        public bool ContainsLevel(int level)
        {
            return level >= FirstLevel && level <= LastLevel;
        }

        public bool TryGetDifficulty(int level, out LocationDifficultyStep step)
        {
            var localLevel = LocalLevel(level);
            if (Difficulty != null)
            {
                foreach (var candidate in Difficulty)
                {
                    if (candidate != null && candidate.Contains(localLevel))
                    {
                        step = candidate;
                        return true;
                    }
                }
            }

            step = null;
            return false;
        }
    }

    [Serializable]
    public sealed class LocationEnvironmentLayer
    {
        public string Name;
        public string ResourcePath;
        public Vector2 Offset;
        [Range(0f, 1f)] public float VerticalSpeed;
        [Min(1f)] public float VerticalRepeatMultiplier = 1f;
        [Min(0f)] public float VerticalRepeatHeight;
        public bool AlignBottomToBaseline;
        [Range(-1f, 1f)] public float HorizontalSpeed;
        public int SortingOrder;
        [Range(0f, 1f)] public float Alpha = 1f;
    }

    [Serializable]
    public sealed class LocationStartAreaVisuals
    {
        public bool HideBaseBackground = true;
        public string LeftWallPath;
        public string RightWallPath;
        public string StartPlatformPath;
        public Vector2 StartPlatformOffset;
        public string StartDoorFramePath;
        public string LeftDoorPath;
        public string RightDoorPath;
        public string WallVfxPath;
        public float WallVfxFramesPerSecond = 10f;
        public float LeftWallLightningLocalX;
        public float RightWallLightningLocalX;
        public bool OverrideWallLightningOffsets;
        public bool StartWallsAreOut = true;
    }

    [Serializable]
    public sealed class LocationPlatformVisuals
    {
        public LocationPlatformVariant[] Variants = Array.Empty<LocationPlatformVariant>();
    }

    [Serializable]
    public sealed class LocationPlatformVariant
    {
        public string ResourcePath;
        [Min(1)] public int FirstLocalLevel = 1;
        public float ColliderAngle = 8f;
        public bool FlipSprite;
    }

    [Serializable]
    public sealed class LocationHazardVisuals
    {
        public bool AllowBaseVisualFallback;
        public string StaticBombPath;
        public string MovingBombPath;
        public string DronePath;
        public string BarrierLeftPath;
        public string BarrierRightPath;
        public string StaticBombVfxPath;
        public string MovingBombVfxPath;
        public string BarrierVfxPath;
    }

    [Serializable]
    public sealed class LocationHazardSettings
    {
        public ChallengeHazardWeights DefaultWeights;

        [Header("Rotating spikes")]
        public string RotatingSpikesVisualPath;
        public float RotatingSpikesDegreesPerSecond = 105f;
        public float RotatingSpikesStartAngle;
        public int RotatingSpikesSectionCount = 4;
        public float RotatingSpikesSectionDistance = 0.48f;
        public float RotatingSpikesColliderRadius = 0.18f;
        public float RotatingSpikesScale = 0.75f;

        [Header("Pop-up spikes")]
        public string PopUpSpikesVisualPath;
        public float PopUpWarningTime = 0.75f;
        public float PopUpExtendTime = 0.28f;
        public float PopUpActiveTime = 0.7f;
        public float PopUpRetractTime = 0.28f;
        public float PopUpCooldownTime = 0.75f;
        public float PopUpHiddenHeight = 0.55f;
        public float PopUpActiveHeight = 0.95f;
        public Vector2 PopUpColliderSize = new Vector2(0.85f, 0.42f);

        [Header("Sticky surface")]
        public string StickyVisualPath;
        [Range(0.1f, 1f)] public float StickyMovementMultiplier = 0.52f;
        public Vector2 StickyColliderSize = new Vector2(1.2f, 0.36f);

        [Header("Rotating electric barrier")]
        public float RotatingBarrierDegreesPerSecond = 72f;
        public float RotatingBarrierStartAngle;
        public float RotatingBarrierLength = 1.3f;
        public float RotatingBarrierThickness = 0.18f;
        public bool RotatingBarrierIsContinuous = true;
        public float RotatingBarrierActiveTime = 1f;
        public float RotatingBarrierInactiveTime = 0.4f;

        public static LocationHazardSettings FromLegacy(JungleHazardSettings source)
        {
            if (source == null)
                return null;

            return new LocationHazardSettings
            {
                DefaultWeights = source.HazardWeights,
                RotatingSpikesVisualPath = source.RotatingSpikesPlaceholderPath,
                RotatingSpikesDegreesPerSecond = source.RotatingSpikesDegreesPerSecond,
                RotatingSpikesStartAngle = source.RotatingSpikesStartAngle,
                RotatingSpikesSectionCount = source.RotatingSpikesSectionCount,
                RotatingSpikesSectionDistance = source.RotatingSpikesSectionDistance,
                RotatingSpikesColliderRadius = source.RotatingSpikesColliderRadius,
                RotatingSpikesScale = source.RotatingSpikesScale,
                PopUpSpikesVisualPath = source.PopUpSpikesPlaceholderPath,
                PopUpWarningTime = source.PopUpWarningTime,
                PopUpExtendTime = source.PopUpExtendTime,
                PopUpActiveTime = source.PopUpActiveTime,
                PopUpRetractTime = source.PopUpRetractTime,
                PopUpCooldownTime = source.PopUpCooldownTime,
                PopUpHiddenHeight = source.PopUpHiddenHeight,
                PopUpActiveHeight = source.PopUpActiveHeight,
                PopUpColliderSize = source.PopUpColliderSize,
                StickyVisualPath = source.StickyVisualPath,
                StickyMovementMultiplier = source.StickyMovementMultiplier,
                StickyColliderSize = source.StickyColliderSize,
                RotatingBarrierDegreesPerSecond = source.RotatingBarrierDegreesPerSecond,
                RotatingBarrierStartAngle = source.RotatingBarrierStartAngle,
                RotatingBarrierLength = source.RotatingBarrierLength,
                RotatingBarrierThickness = source.RotatingBarrierThickness,
                RotatingBarrierIsContinuous = source.RotatingBarrierIsContinuous,
                RotatingBarrierActiveTime = source.RotatingBarrierActiveTime,
                RotatingBarrierInactiveTime = source.RotatingBarrierInactiveTime
            };
        }
    }

    [Serializable]
    public sealed class LocationDifficultyStep
    {
        [Min(1)] public int FirstLocalLevel = 1;
        [Min(1)] public int LastLocalLevel = 1;
        public float PlayerSpeed;
        [Range(0f, 1f)] public float HazardChance;
        public float ObstacleSpeedMultiplier = 1f;
        [Range(0f, 1f)] public float MovingPlatformChance;
        public ChallengeHazardWeights HazardWeights;

        public bool Contains(int localLevel)
        {
            return localLevel >= FirstLocalLevel && localLevel <= LastLocalLevel;
        }
    }
}
