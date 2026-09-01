using System;
using UnityEngine;

namespace GameLogic
{
    public static class LocationDefaults
    {
        public static LocationConfig CreateFutureCity()
        {
            var config = ScriptableObject.CreateInstance<LocationConfig>();
            config.name = "FutureCityLocationConfig";
            config.Location = ChallengeLocation.FutureCity;
            config.FirstLevel = 1;
            config.LastLevel = 10;
            config.ResourceRoot = "FutureCity";
            config.EnvironmentLayers = new[]
            {
                Layer("Sky", "Environment/sky", 0.015f, 0f, 0, 1f, new Vector2(0f, 0f), 1f, false),
                Layer("Far City", "Environment/city_4", 0.12f, 0f, 10, 1f, new Vector2(0f, -0.9f), 1f, true, 8.05f),
                Layer("Mid City A", "Environment/city_3", 0.12f, 0f, 20, 1f, new Vector2(0f, -2.3f), 1f, true, 8.05f),
                Layer("Far Clouds", "Environment/clouds_far", 0.12f, 0.27f, 1, 0.72f, new Vector2(0f, 3.2f), 1f, false, 8.05f),
                Layer("Mid City B", "Environment/city_2", 0.12f, 0f, 30, 1f, new Vector2(0f, -1.65f), 1f, true, 8.05f),
                Layer("Near City", "Environment/city_1", 0.12f, 0f, 40, 1f, new Vector2(0f, 0f), 1f, true, 8.05f),
                Layer("Near Clouds", "Environment/clouds_near", 0.12f, -0.2f, 2, 0.36f, new Vector2(0f, -3.2f), 1f, false, 8.05f)
            };
            config.StartArea = new LocationStartAreaVisuals
            {
                HideBaseBackground = true,
                LeftWallPath = "Start/wall_left",
                RightWallPath = "Start/wall_right",
                StartPlatformPath = "Start/start_platform",
                StartDoorFramePath = "Start/start_door",
                LeftDoorPath = "Start/door_left",
                RightDoorPath = "Start/door_right",
                WallVfxPath = "VFX/Wall",
                WallVfxFramesPerSecond = 10f,
                StartWallsAreOut = true
            };
            config.Platforms = DefaultPlatforms();
            config.HazardVisuals = new LocationHazardVisuals
            {
                AllowBaseVisualFallback = true,
                DronePath = "Enemies/enemy_drone",
                BarrierLeftPath = "Enemies/enemy_laser_left",
                BarrierRightPath = "Enemies/enemy_laser_right",
                StaticBombVfxPath = "VFX/Circle",
                MovingBombVfxPath = "VFX/Circle",
                BarrierVfxPath = "VFX/Long"
            };
            config.Hazards = new LocationHazardSettings();
            config.Difficulty = BuildDifficulty(config.FirstLevel, config.LastLevel, null);
            return config;
        }

        public static LocationConfig CreateJungle()
        {
            var legacy = JungleTheme.Config;
            if (legacy == null)
                return null;

            var config = ScriptableObject.CreateInstance<LocationConfig>();
            config.name = "JungleLocationConfig";
            config.Location = ChallengeLocation.Jungle;
            config.FirstLevel = 11;
            config.LastLevel = 20;
            config.ResourceRoot = "Jungle";
            config.EnvironmentLayers = new LocationEnvironmentLayer[legacy.Layers.Length];
            for (var i = 0; i < legacy.Layers.Length; i++)
            {
                var source = legacy.Layers[i];
                config.EnvironmentLayers[i] = Layer(source.Name, source.ResourcePath,
                    source.VerticalSpeed, source.HorizontalSpeed, source.SortingOrder, source.Alpha,
                    source.Offset, source.VerticalRepeatMultiplier, source.AlignBottomToBaseline);
            }

            var visuals = legacy.Visuals;
            config.StartArea = new LocationStartAreaVisuals
            {
                HideBaseBackground = true,
                LeftWallPath = visuals.LeftWallPath,
                RightWallPath = visuals.RightWallPath,
                StartPlatformPath = visuals.StartPlatformPath,
                StartPlatformOffset = visuals.StartPlatformOffset,
                StartDoorFramePath = visuals.StartDoorFramePath,
                LeftDoorPath = visuals.LeftDoorPath,
                RightDoorPath = visuals.RightDoorPath,
                WallVfxPath = visuals.WallVfxPath,
                WallVfxFramesPerSecond = 10f,
                LeftWallLightningLocalX = visuals.LeftWallLightningLocalX,
                RightWallLightningLocalX = visuals.RightWallLightningLocalX,
                OverrideWallLightningOffsets = true,
                StartWallsAreOut = false
            };
            config.Platforms = DefaultPlatforms(visuals.PlatformPaths, true);
            config.HazardVisuals = new LocationHazardVisuals
            {
                StaticBombPath = visuals.StaticBombPath,
                MovingBombPath = visuals.MovingBombPath,
                BarrierLeftPath = visuals.BarrierLeftPath,
                BarrierRightPath = visuals.BarrierRightPath,
                StaticBombVfxPath = visuals.StaticBombVfxPath,
                MovingBombVfxPath = visuals.MovingBombVfxPath,
                BarrierVfxPath = visuals.BarrierVfxPath
            };
            config.Hazards = LocationHazardSettings.FromLegacy(legacy.Hazards);
            config.Difficulty = BuildDifficulty(config.FirstLevel, config.LastLevel,
                legacy.Hazards.HazardWeights);
            return config;
        }

        public static LocationDifficultyStep[] BuildDifficulty(int firstLevel, int lastLevel,
            ChallengeHazardWeights? fixedHazardWeights)
        {
            var result = new LocationDifficultyStep[lastLevel - firstLevel + 1];
            for (var level = firstLevel; level <= lastLevel; level++)
            {
                var localLevel = level - firstLevel + 1;
                result[localLevel - 1] = new LocationDifficultyStep
                {
                    FirstLocalLevel = localLevel,
                    LastLocalLevel = localLevel,
                    PlayerSpeed = LegacyPlayerSpeed(level),
                    HazardChance = LegacyHazardChance(level),
                    ObstacleSpeedMultiplier = Mathf.Lerp(0.85f, 1.75f, (level - 1f) / 49f),
                    MovingPlatformChance = 0f,
                    HazardWeights = fixedHazardWeights ?? LegacyHazardWeights(level)
                };
            }

            return result;
        }

        private static LocationPlatformVisuals DefaultPlatforms(string[] paths = null,
            bool allAvailable = false)
        {
            paths = paths == null || paths.Length == 0
                ? new[] { "Platforms/platform_1", "Platforms/platform_2", "Platforms/platform_3" }
                : paths;
            var variants = new LocationPlatformVariant[paths.Length];
            for (var i = 0; i < paths.Length; i++)
            {
                variants[i] = new LocationPlatformVariant
                {
                    ResourcePath = paths[i],
                    FirstLocalLevel = allAvailable ? 1 : i == 0 ? 1 : i == 1 ? 4 : 8,
                    ColliderAngle = i == 0 ? 7.12f : i == 1 ? 14.04f : 26.56f,
                    FlipSprite = i == 1
                };
            }
            return new LocationPlatformVisuals { Variants = variants };
        }

        private static LocationEnvironmentLayer Layer(string name, string path, float verticalSpeed,
            float horizontalSpeed, int sortingOrder, float alpha, Vector2 offset,
            float repeatMultiplier, bool alignBottom, float repeatHeight = 0f)
        {
            return new LocationEnvironmentLayer
            {
                Name = name,
                ResourcePath = path,
                VerticalSpeed = verticalSpeed,
                HorizontalSpeed = horizontalSpeed,
                SortingOrder = sortingOrder,
                Alpha = alpha,
                Offset = offset,
                VerticalRepeatMultiplier = repeatMultiplier,
                VerticalRepeatHeight = repeatHeight,
                AlignBottomToBaseline = alignBottom
            };
        }

        private static float LegacyPlayerSpeed(int level)
        {
            return Mathf.Lerp(2.65f, 4.15f,
                Mathf.SmoothStep(0f, 1f, (level - 1f) / 49f));
        }

        private static float LegacyHazardChance(int level)
        {
            if (level == 1) return 0f;
            if (level <= 5) return 0.35f;
            if (level <= 10) return 0.48f;
            if (level <= 20) return 0.58f;
            if (level <= 30) return 0.66f;
            if (level <= 40) return 0.72f;
            return 0.78f;
        }

        private static ChallengeHazardWeights LegacyHazardWeights(int level)
        {
            if (level <= 5)
                return new ChallengeHazardWeights { StaticBomb = 0.75f, Laser = 0.25f };
            return new ChallengeHazardWeights
            {
                StaticBomb = 0.45f,
                MovingBomb = level == 10 ? 0.2f : 0.08f,
                Laser = 0.35f,
                Drone = 0.12f
            };
        }
    }
}
