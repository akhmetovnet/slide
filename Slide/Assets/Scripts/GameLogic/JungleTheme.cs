using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic
{
    public static class JungleTheme
    {
        private const string ConfigPath = "Jungle/JungleEnvironmentConfig";

        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite[]> FrameCache =
            new Dictionary<string, Sprite[]>();
        private static Sprite[] _platformFrames;
        private static JungleEnvironmentConfig _config;

        public static JungleEnvironmentConfig Config
        {
            get
            {
                if (_config == null)
                    _config = Resources.Load<JungleEnvironmentConfig>(ConfigPath);
                return _config;
            }
        }

        public static bool IsActive(GameController gameController)
        {
            return LocationTheme.IsActive(gameController, ChallengeLocation.Jungle);
        }

        public static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var locationConfig = LocationCatalog.Get(ChallengeLocation.Jungle);
            if (locationConfig != null)
                return LocationTheme.LoadSprite(locationConfig, path);

            if (!SpriteCache.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>("Jungle/" + path);
                SpriteCache[path] = sprite;
            }

            return sprite;
        }

        public static Sprite[] LoadFrames(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Array.Empty<Sprite>();

            var locationConfig = LocationCatalog.Get(ChallengeLocation.Jungle);
            if (locationConfig != null)
                return LocationTheme.LoadFrames(locationConfig, path);

            if (!FrameCache.TryGetValue(path, out var frames))
            {
                frames = Resources.LoadAll<Sprite>("Jungle/" + path)
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
                    .ToArray();
                FrameCache[path] = frames;
            }

            return frames;
        }

        public static Sprite[] LoadPlatformFrames()
        {
            if (_platformFrames != null)
                return _platformFrames;

            var locationConfig = LocationCatalog.Get(ChallengeLocation.Jungle);
            if (locationConfig != null && locationConfig.Platforms != null &&
                locationConfig.Platforms.Variants != null)
            {
                return _platformFrames = locationConfig.Platforms.Variants
                    .Select(variant => LocationTheme.LoadSprite(locationConfig, variant.ResourcePath))
                    .Where(sprite => sprite != null)
                    .ToArray();
            }

            var config = Config;
            if (config == null || config.Visuals.PlatformPaths == null)
                return _platformFrames = Array.Empty<Sprite>();

            return _platformFrames = config.Visuals.PlatformPaths
                .Select(LoadSprite)
                .Where(sprite => sprite != null)
                .ToArray();
        }

    }
}
