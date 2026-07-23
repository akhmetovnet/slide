using UnityEngine;

namespace GameLogic
{
    public static class ChallengeProgress
    {
        public const string LegacyLevelKey = "Level";
        public const string UnlockedLevelKey = "UnlockedChallengeLevel";
        public const string SelectedLevelKey = "SelectedChallengeLevel";
        public const string AutoStartKey = "MissionAutoStart";
        public const string SeenUnlockedLevelKey = "MissionMenuSeenUnlockedLevel";

        public static int HighestUnlockedLevel
        {
            get
            {
                var legacyLevel = Clamp(PlayerPrefs.GetInt(LegacyLevelKey, 1));
                return Clamp(Mathf.Max(legacyLevel, PlayerPrefs.GetInt(UnlockedLevelKey, legacyLevel)));
            }
        }

        public static int SelectedLevel
        {
            get
            {
                var unlockedLevel = HighestUnlockedLevel;
                return Mathf.Clamp(
                    PlayerPrefs.GetInt(SelectedLevelKey, PlayerPrefs.GetInt(LegacyLevelKey, unlockedLevel)),
                    1,
                    unlockedLevel);
            }
        }

        public static void Initialize()
        {
            var unlockedLevel = HighestUnlockedLevel;
            var selectedLevel = SelectedLevel;

            PlayerPrefs.SetInt(UnlockedLevelKey, unlockedLevel);
            PlayerPrefs.SetInt(SelectedLevelKey, selectedLevel);
            PlayerPrefs.SetInt(LegacyLevelKey, selectedLevel);
        }

        public static bool IsUnlocked(int level)
        {
            return Clamp(level) <= HighestUnlockedLevel;
        }

        public static bool SelectLevel(int level)
        {
            Initialize();
            if (level < 1 || level > HighestUnlockedLevel)
                return false;

            var selectedLevel = Clamp(level);
            PlayerPrefs.SetInt(SelectedLevelKey, selectedLevel);
            PlayerPrefs.SetInt(LegacyLevelKey, selectedLevel);
            PlayerPrefs.Save();
            return true;
        }

        public static int CompleteLevel(int completedLevel)
        {
            Initialize();

            var nextLevel = Clamp(completedLevel + 1);
            var unlockedLevel = Mathf.Max(HighestUnlockedLevel, nextLevel);
            PlayerPrefs.SetInt(UnlockedLevelKey, unlockedLevel);
            PlayerPrefs.SetInt(SelectedLevelKey, nextLevel);
            PlayerPrefs.SetInt(LegacyLevelKey, nextLevel);
            PlayerPrefs.Save();
            return nextLevel;
        }

        private static int Clamp(int level)
        {
            return Mathf.Clamp(level, 1, ChallengeLevelCatalog.LevelCount);
        }
    }
}
