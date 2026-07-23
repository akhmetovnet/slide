using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLogic
{
    public static class FutureCityTheme
    {
        public const int FirstMission = 1;
        public const int LastMission = 10;

        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite[]> FrameCache =
            new Dictionary<string, Sprite[]>();

        public static bool IsActive(GameController gameController)
        {
            return gameController != null &&
                   gameController.Mode == GameMode.Challenge &&
                   gameController.CurrentLevel >= FirstMission &&
                   gameController.CurrentLevel <= LastMission;
        }

        public static Sprite LoadSprite(string path)
        {
            if (!SpriteCache.TryGetValue(path, out var sprite))
            {
                sprite = Resources.Load<Sprite>("FutureCity/" + path);
                SpriteCache[path] = sprite;
            }

            return sprite;
        }

        public static Sprite[] LoadFrames(string path)
        {
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
