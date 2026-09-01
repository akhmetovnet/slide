using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic
{
    public static class FutureCityTheme
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite[]> FrameCache =
            new Dictionary<string, Sprite[]>();

        public static bool IsActive(GameController gameController)
        {
            return LocationTheme.IsActive(gameController, ChallengeLocation.FutureCity);
        }

        public static Sprite LoadSprite(string path)
        {
            var config = LocationCatalog.Get(ChallengeLocation.FutureCity);
            if (config != null)
                return LocationTheme.LoadSprite(config, path);

            if (!SpriteCache.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>("FutureCity/" + path);
                SpriteCache[path] = sprite;
            }

            return sprite;
        }

        public static Sprite[] LoadFrames(string path)
        {
            var config = LocationCatalog.Get(ChallengeLocation.FutureCity);
            if (config != null)
                return LocationTheme.LoadFrames(config, path);

            if (!FrameCache.TryGetValue(path, out var frames))
            {
                frames = Resources.LoadAll<Sprite>("FutureCity/" + path)
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
                    .ToArray();
                FrameCache[path] = frames;
            }

            return frames;
        }
    }
}
