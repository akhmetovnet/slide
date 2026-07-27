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
        private static Sprite[] _rocketFrames;
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
            return gameController != null &&
                   gameController.Mode == GameMode.Challenge &&
                   gameController.CurrentChallengeDefinition != null &&
                   gameController.CurrentChallengeDefinition.Location == ChallengeLocation.Jungle;
        }

        public static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

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

            var config = Config;
            if (config == null || config.Visuals.PlatformPaths == null)
                return _platformFrames = Array.Empty<Sprite>();

            return _platformFrames = config.Visuals.PlatformPaths
                .Select(LoadSprite)
                .Where(sprite => sprite != null)
                .ToArray();
        }

        public static Sprite[] LoadRocketFrames()
        {
            if (_rocketFrames != null)
                return _rocketFrames;

            var config = Config;
            if (config == null || config.Visuals.RocketFramePaths == null ||
                config.Visuals.RocketFramePaths.Length < 3)
                return _rocketFrames = Array.Empty<Sprite>();

            var first = LoadSprite(config.Visuals.RocketFramePaths[0]);
            var second = LoadSprite(config.Visuals.RocketFramePaths[1]);
            var third = LoadSprite(config.Visuals.RocketFramePaths[2]);
            _rocketFrames = new[] { first, second, third, second }
                .Where(sprite => sprite != null)
                .ToArray();
            return _rocketFrames;
        }
    }
}
