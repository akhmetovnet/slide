using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public static class LocationConfigValidator
    {
        public static IReadOnlyList<string> Validate(LocationConfig config)
        {
            var errors = new List<string>();
            if (config == null)
            {
                errors.Add("Location config is missing.");
                return errors;
            }

            if (config.FirstLevel < 1 || config.LastLevel < config.FirstLevel)
                errors.Add(config.name + ": invalid level range.");

            ValidateEnvironment(config, errors);
            ValidateStartArea(config, errors);
            ValidatePlatforms(config, errors);
            ValidateHazards(config, errors);
            if (config.MissionMenuBackground == null)
                errors.Add(config.name + ": mission menu background is missing.");
            return errors;
        }

        private static void ValidateEnvironment(LocationConfig config, ICollection<string> errors)
        {
            if (config.EnvironmentLayers == null || config.EnvironmentLayers.Length == 0)
            {
                errors.Add(config.name + ": no environment layers configured.");
                return;
            }
            foreach (var layer in config.EnvironmentLayers)
            {
                if (layer == null || string.IsNullOrEmpty(layer.ResourcePath) ||
                    LocationTheme.LoadSprite(config, layer.ResourcePath) == null)
                    errors.Add(config.name + ": missing environment sprite " +
                               (layer != null ? layer.ResourcePath : "<null>"));
            }
        }

        private static void ValidateStartArea(LocationConfig config, ICollection<string> errors)
        {
            var start = config.StartArea;
            if (start == null)
            {
                errors.Add(config.name + ": start area is missing.");
                return;
            }

            var spritePaths = new[]
            {
                start.LeftWallPath, start.RightWallPath, start.StartPlatformPath,
                start.StartDoorFramePath, start.LeftDoorPath, start.RightDoorPath
            };
            foreach (var path in spritePaths)
            {
                if (!HasSprite(config, path))
                    errors.Add(config.name + ": missing start area sprite " + path);
            }
            if (!HasFrames(config, start.WallVfxPath))
                errors.Add(config.name + ": missing wall VFX " + start.WallVfxPath);
        }

        private static void ValidatePlatforms(LocationConfig config, ICollection<string> errors)
        {
            if (config.Platforms == null || config.Platforms.Variants == null ||
                config.Platforms.Variants.Length == 0)
            {
                errors.Add(config.name + ": no platform variants configured.");
                return;
            }

            foreach (var variant in config.Platforms.Variants)
            {
                if (variant == null || LocationTheme.LoadSprite(config, variant.ResourcePath) == null)
                    errors.Add(config.name + ": missing platform sprite " +
                               (variant != null ? variant.ResourcePath : "<null>"));
            }
        }

        private static void ValidateHazards(LocationConfig config, ICollection<string> errors)
        {
            var used = new HashSet<ThornType>();
            if (config.Difficulty != null)
            {
                foreach (var step in config.Difficulty)
                {
                    if (step == null)
                        continue;
                    foreach (ThornType type in System.Enum.GetValues(typeof(ThornType)))
                    {
                        if (type != ThornType.None && step.HazardWeights.Get(type) > 0f)
                            used.Add(type);
                    }
                }
            }

            foreach (var type in used)
            {
                if (!HasRequiredAssets(config, type))
                    errors.Add(config.name + ": " + type + " has weight but required assets are missing.");
            }
        }

        private static bool HasRequiredAssets(LocationConfig config, ThornType type)
        {
            var visuals = config.HazardVisuals;
            var settings = config.Hazards;
            if (visuals == null)
                return false;

            switch (type)
            {
                case ThornType.Static:
                    return visuals.AllowBaseVisualFallback || HasSprite(config, visuals.StaticBombPath);
                case ThornType.Kinematic:
                    return visuals.AllowBaseVisualFallback || HasSprite(config, visuals.MovingBombPath);
                case ThornType.Laser:
                    return visuals.AllowBaseVisualFallback ||
                           HasSprite(config, visuals.BarrierLeftPath) &&
                           HasSprite(config, visuals.BarrierRightPath) &&
                           HasFrames(config, visuals.BarrierVfxPath);
                case ThornType.Drone:
                    return visuals.AllowBaseVisualFallback || HasSprite(config, visuals.DronePath) ||
                           HasSprite(config, visuals.MovingBombPath);
                case ThornType.RotatingSpikes:
                    return settings != null && HasSpriteOrFrames(config, settings.RotatingSpikesVisualPath);
                case ThornType.PopUpSpikes:
                    return settings != null && HasSprite(config, settings.PopUpSpikesVisualPath);
                case ThornType.StickySurface:
                    return settings != null && HasSprite(config, settings.StickyVisualPath);
                case ThornType.RotatingLaser:
                    return settings != null && HasSprite(config, visuals.BarrierLeftPath) &&
                           HasSprite(config, visuals.BarrierRightPath) &&
                           HasFrames(config, visuals.BarrierVfxPath);
                default:
                    return false;
            }
        }

        private static bool HasSprite(LocationConfig config, string path)
        {
            return !string.IsNullOrEmpty(path) && LocationTheme.LoadSprite(config, path) != null;
        }

        private static bool HasFrames(LocationConfig config, string path)
        {
            return !string.IsNullOrEmpty(path) && LocationTheme.LoadFrames(config, path).Length > 0;
        }

        private static bool HasSpriteOrFrames(LocationConfig config, string path)
        {
            return HasSprite(config, path) || HasFrames(config, path);
        }
    }
}
