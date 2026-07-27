using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public enum ChallengeObjectiveType
    {
        ReachPlatforms,
        CollectItems,
        RaceBot,
        CatchCriminal
    }

    public enum ChallengeLocation
    {
        FutureCity,
        Jungle,
        Cyberpunk,
        SpaceStation,
        DeepBunker
    }

    [Serializable]
    public sealed class ChallengeLevelDefinition
    {
        public int Level;
        public ChallengeObjectiveType Objective;
        public int TargetCount;
        public ChallengeLocation Location;
        public float PlayerSpeed;
        public float HazardChance;
        public float ObstacleSpeedMultiplier;
        public float MovingPlatformChance;
        public float MissionItemChance;
        public int MissionItemVariant;
        public float RivalPlatformsPerSecond;
        public float RivalStartLead;
        public float RivalEscapeLead;
        public float CaptureDistance;
        public ChallengeHazardWeights HazardWeights;

        public bool UsesStreamingField =>
            Objective == ChallengeObjectiveType.CollectItems ||
            Objective == ChallengeObjectiveType.CatchCriminal ||
            TargetCount > 20;

        public string GetTitle()
        {
            switch (Objective)
            {
                case ChallengeObjectiveType.ReachPlatforms:
                    return $"ПРЕОДОЛЕЙ {TargetCount} ПЛАТФОРМ";
                case ChallengeObjectiveType.CollectItems:
                    return $"СОБЕРИ {TargetCount} ПРЕДМЕТОВ";
                case ChallengeObjectiveType.RaceBot:
                    return $"БУДЬ ПЕРВЫМ: {TargetCount} ПЛАТФОРМ";
                case ChallengeObjectiveType.CatchCriminal:
                    return "ПОЙМАЙ ПРЕСТУПНИКА";
                default:
                    return string.Empty;
            }
        }
    }

    [Serializable]
    public struct ChallengeHazardWeights
    {
        public float StaticBomb;
        public float MovingBomb;
        public float Laser;
        public float Drone;
        public float RotatingSpikes;
        public float PopUpSpikes;
        public float StickySurface;
        public float RotatingLaser;

        public float Get(ThornType type)
        {
            switch (type)
            {
                case ThornType.Static: return StaticBomb;
                case ThornType.Kinematic: return MovingBomb;
                case ThornType.Laser: return Laser;
                case ThornType.Drone: return Drone;
                case ThornType.RotatingSpikes: return RotatingSpikes;
                case ThornType.PopUpSpikes: return PopUpSpikes;
                case ThornType.StickySurface: return StickySurface;
                case ThornType.RotatingLaser: return RotatingLaser;
                default: return 0f;
            }
        }
    }

    public static class ChallengeLevelCatalog
    {
        public const int LevelCount = 50;

        private static readonly ChallengeLevelDefinition[] Levels = BuildLevels();

        public static IReadOnlyList<ChallengeLevelDefinition> All => Levels;

        public static ChallengeLevelDefinition Get(int level)
        {
            return Levels[Mathf.Clamp(level, 1, LevelCount) - 1];
        }

        private static ChallengeLevelDefinition[] BuildLevels()
        {
            var objectives = new[]
            {
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.RaceBot, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.RaceBot,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CatchCriminal,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.CatchCriminal, ChallengeObjectiveType.RaceBot,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.RaceBot,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.CatchCriminal,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.RaceBot, ChallengeObjectiveType.CatchCriminal,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.RaceBot,
                ChallengeObjectiveType.ReachPlatforms, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.RaceBot, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CatchCriminal, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.CatchCriminal,
                ChallengeObjectiveType.RaceBot, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.RaceBot, ChallengeObjectiveType.CollectItems,
                ChallengeObjectiveType.CatchCriminal, ChallengeObjectiveType.ReachPlatforms,
                ChallengeObjectiveType.CollectItems, ChallengeObjectiveType.RaceBot,
                ChallengeObjectiveType.CatchCriminal, ChallengeObjectiveType.CollectItems
            };

            // Targets are capped to keep a late-game mobile run under roughly two minutes.
            var targets = new[]
            {
                10, 5, 16, 8, 20, 24, 10, 24, 32, 0,
                40, 15, 0, 32, 50, 18, 60, 38, 22, 0,
                70, 26, 50, 0, 30, 80, 34, 90, 38, 60,
                100, 42, 70, 110, 0, 125, 48, 0, 80, 140,
                55, 160, 90, 65, 0, 180, 75, 100, 0, 90
            };

            var result = new ChallengeLevelDefinition[LevelCount];
            var collectItemIndex = 0;
            for (var i = 0; i < result.Length; i++)
            {
                var level = i + 1;
                var isCollectMission = objectives[i] == ChallengeObjectiveType.CollectItems;
                result[i] = new ChallengeLevelDefinition
                {
                    Level = level,
                    Objective = objectives[i],
                    TargetCount = targets[i],
                    Location = GetLocation(level),
                    PlayerSpeed = GetPlayerSpeed(level),
                    HazardChance = GetHazardChance(level),
                    ObstacleSpeedMultiplier = Mathf.Lerp(0.85f, 1.75f, (level - 1f) / 49f),
                    MovingPlatformChance = level < 8 ? 0f : Mathf.Lerp(0.08f, 0.3f, (level - 8f) / 42f),
                    MissionItemChance = isCollectMission ? 0.88f : 0f,
                    MissionItemVariant = isCollectMission
                        ? collectItemIndex++ % ChallengeAssetCatalog.MissionItemCount
                        : 0,
                    RivalPlatformsPerSecond = GetRivalSpeed(level, objectives[i]),
                    RivalStartLead = objectives[i] == ChallengeObjectiveType.CatchCriminal ? 4f : 0f,
                    RivalEscapeLead = Mathf.Lerp(10f, 7f, (level - 1f) / 49f),
                    CaptureDistance = 0.6f,
                    HazardWeights = GetHazardWeights(GetLocation(level), level)
                };
            }

            return result;
        }

        private static ChallengeLocation GetLocation(int level)
        {
            if (level <= 10) return ChallengeLocation.FutureCity;
            if (level <= 20) return ChallengeLocation.Jungle;
            if (level <= 30) return ChallengeLocation.SpaceStation;
            if (level <= 40) return ChallengeLocation.Cyberpunk;
            return ChallengeLocation.DeepBunker;
        }

        private static float GetPlayerSpeed(int level)
        {
            var normalized = (level - 1f) / 49f;
            return Mathf.Lerp(2.65f, 4.15f, Mathf.SmoothStep(0f, 1f, normalized));
        }

        private static float GetHazardChance(int level)
        {
            if (level == 1) return 0f;
            if (level <= 5) return 0.35f;
            if (level <= 10) return 0.48f;
            if (level <= 20) return 0.58f;
            if (level <= 30) return 0.66f;
            if (level <= 40) return 0.72f;
            return 0.78f;
        }

        private static float GetRivalSpeed(int level, ChallengeObjectiveType objective)
        {
            if (objective != ChallengeObjectiveType.RaceBot && objective != ChallengeObjectiveType.CatchCriminal)
                return 0f;

            var baseSpeed = objective == ChallengeObjectiveType.RaceBot ? 0.58f : 0.48f;
            return baseSpeed + Mathf.Clamp01((level - 1f) / 49f) * 0.22f;
        }

        private static ChallengeHazardWeights GetHazardWeights(ChallengeLocation location, int level)
        {
            if (location == ChallengeLocation.Jungle)
            {
                var jungleConfig = JungleTheme.Config;
                if (jungleConfig != null)
                    return jungleConfig.Hazards.HazardWeights;
            }

            if (level <= 5)
            {
                return new ChallengeHazardWeights
                {
                    StaticBomb = 0.75f,
                    Laser = 0.25f
                };
            }

            if (level <= 10)
            {
                return new ChallengeHazardWeights
                {
                    StaticBomb = 0.45f,
                    MovingBomb = level == 10 ? 0.2f : 0.08f,
                    Laser = 0.35f,
                    Drone = 0.12f
                };
            }

            if (level <= 30)
            {
                return new ChallengeHazardWeights
                {
                    StaticBomb = 0.24f,
                    MovingBomb = 0.28f,
                    Laser = 0.27f,
                    Drone = 0.21f
                };
            }

            if (level <= 40)
            {
                return new ChallengeHazardWeights
                {
                    StaticBomb = 0.21f,
                    MovingBomb = 0.28f,
                    Laser = 0.27f,
                    Drone = 0.24f
                };
            }

            return new ChallengeHazardWeights
            {
                StaticBomb = 0.18f,
                MovingBomb = 0.3f,
                Laser = 0.27f,
                Drone = 0.25f
            };
        }
    }
}
