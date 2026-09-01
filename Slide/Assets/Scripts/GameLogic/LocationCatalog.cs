using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic
{
    public static class LocationCatalog
    {
        private const string ResourcesPath = "Locations";
        private static Dictionary<ChallengeLocation, LocationConfig> _configs;

        public static IReadOnlyCollection<LocationConfig> All
        {
            get
            {
                EnsureLoaded();
                return _configs.Values;
            }
        }

        public static LocationConfig Get(ChallengeLocation location)
        {
            EnsureLoaded();
            return _configs.TryGetValue(location, out var config) ? config : null;
        }

        public static LocationConfig GetByLevel(int level)
        {
            EnsureLoaded();
            return _configs.Values
                .Where(config => config != null && config.ContainsLevel(level))
                .OrderBy(config => config.FirstLevel)
                .FirstOrDefault();
        }

        public static LocationConfig GetActive(GameController gameController)
        {
            if (gameController == null || gameController.Mode != GameMode.Challenge ||
                gameController.CurrentChallengeDefinition == null)
                return null;

            return Get(gameController.CurrentChallengeDefinition.Location);
        }

        public static void Reload()
        {
            _configs = null;
            LocationTheme.ClearCache();
        }

        private static void EnsureLoaded()
        {
            if (_configs != null)
                return;

            _configs = new Dictionary<ChallengeLocation, LocationConfig>();
            foreach (var config in Resources.LoadAll<LocationConfig>(ResourcesPath))
            {
                if (config != null)
                    _configs[config.Location] = config;
            }

            if (!_configs.ContainsKey(ChallengeLocation.FutureCity))
                AddFallback(LocationDefaults.CreateFutureCity());
            if (!_configs.ContainsKey(ChallengeLocation.Jungle))
                AddFallback(LocationDefaults.CreateJungle());
        }

        private static void AddFallback(LocationConfig fallback)
        {
            if (fallback == null)
                return;

            fallback.hideFlags = HideFlags.HideAndDontSave;
            _configs[fallback.Location] = fallback;
        }
    }

    public static class LocationTheme
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite[]> FrameCache =
            new Dictionary<string, Sprite[]>();

        public static bool IsActive(GameController gameController, ChallengeLocation location)
        {
            return gameController != null && gameController.Mode == GameMode.Challenge &&
                   gameController.CurrentChallengeDefinition != null &&
                   gameController.CurrentChallengeDefinition.Location == location;
        }

        public static Sprite LoadSprite(LocationConfig config, string path)
        {
            if (config == null || string.IsNullOrEmpty(path))
                return null;

            var rootedPath = RootedPath(config, path);
            if (!SpriteCache.TryGetValue(rootedPath, out var sprite))
            {
                sprite = Resources.Load<Sprite>(rootedPath);
                if (sprite == null && !string.Equals(rootedPath, path, StringComparison.Ordinal))
                    sprite = Resources.Load<Sprite>(path);
                SpriteCache[rootedPath] = sprite;
            }

            return sprite;
        }

        public static Sprite[] LoadFrames(LocationConfig config, string path)
        {
            if (config == null || string.IsNullOrEmpty(path))
                return Array.Empty<Sprite>();

            var rootedPath = RootedPath(config, path);
            if (!FrameCache.TryGetValue(rootedPath, out var frames))
            {
                frames = Resources.LoadAll<Sprite>(rootedPath);
                if (frames.Length == 0 && !string.Equals(rootedPath, path, StringComparison.Ordinal))
                    frames = Resources.LoadAll<Sprite>(path);
                Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
                FrameCache[rootedPath] = frames;
            }

            return frames;
        }

        public static void ClearCache()
        {
            SpriteCache.Clear();
            FrameCache.Clear();
        }

        private static string RootedPath(LocationConfig config, string path)
        {
            return string.IsNullOrEmpty(config.ResourceRoot)
                ? path
                : config.ResourceRoot.TrimEnd('/') + "/" + path.TrimStart('/');
        }
    }
}
