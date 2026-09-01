namespace GameLogic
{
    public readonly struct ResolvedLocationDifficulty
    {
        public readonly float PlayerSpeed;
        public readonly float HazardChance;
        public readonly float ObstacleSpeedMultiplier;
        public readonly float MovingPlatformChance;
        public readonly ChallengeHazardWeights HazardWeights;

        public ResolvedLocationDifficulty(float playerSpeed, float hazardChance,
            float obstacleSpeedMultiplier, float movingPlatformChance,
            ChallengeHazardWeights hazardWeights)
        {
            PlayerSpeed = playerSpeed;
            HazardChance = hazardChance;
            ObstacleSpeedMultiplier = obstacleSpeedMultiplier;
            MovingPlatformChance = movingPlatformChance;
            HazardWeights = hazardWeights;
        }
    }

    public static class LocationDifficultyResolver
    {
        public static ResolvedLocationDifficulty Resolve(int level, ChallengeLocation location,
            ResolvedLocationDifficulty fallback)
        {
            var config = LocationCatalog.Get(location);
            if (config == null || !config.TryGetDifficulty(level, out var step))
                return fallback;

            return new ResolvedLocationDifficulty(
                step.PlayerSpeed,
                step.HazardChance,
                step.ObstacleSpeedMultiplier,
                step.MovingPlatformChance,
                step.HazardWeights);
        }
    }
}
