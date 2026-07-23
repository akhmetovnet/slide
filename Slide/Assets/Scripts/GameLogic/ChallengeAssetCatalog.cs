using System.Linq;
using UnityEngine;

namespace GameLogic
{
    public static class ChallengeAssetCatalog
    {
        public const int MissionItemCount = 16;

        public static Sprite LoadMissionItem(int index)
        {
            var safeIndex = Mathf.Abs(index) % MissionItemCount + 1;
            return Resources.Load<Sprite>($"Challenge/Items/item_{safeIndex:00}");
        }

        public static Sprite[] LoadHazardFrames(string hazardName)
        {
            return Resources.LoadAll<Sprite>($"Challenge/Hazards/{hazardName}")
                .OrderBy(x => x.name)
                .ToArray();
        }

        public static Sprite LoadHazard(string hazardName)
        {
            return Resources.Load<Sprite>($"Challenge/Hazards/{hazardName}");
        }
    }
}
