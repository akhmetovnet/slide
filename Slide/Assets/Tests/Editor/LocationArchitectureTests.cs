#if UNITY_INCLUDE_TESTS
using GameLogic;
using NUnit.Framework;
using UnityEngine;

public sealed class LocationArchitectureTests
{
    [SetUp]
    public void SetUp()
    {
        LocationCatalog.Reload();
    }

    [Test]
    public void CatalogResolvesFutureCityAndJungleByLocationAndLevel()
    {
        Assert.That(Resources.Load<LocationConfig>("Locations/FutureCity"), Is.Not.Null);
        Assert.That(Resources.Load<LocationConfig>("Locations/Jungle"), Is.Not.Null);
        var futureCity = LocationCatalog.Get(ChallengeLocation.FutureCity);
        var jungle = LocationCatalog.Get(ChallengeLocation.Jungle);

        Assert.That(futureCity, Is.Not.Null);
        Assert.That(jungle, Is.Not.Null);
        Assert.That(LocationCatalog.GetByLevel(1), Is.SameAs(futureCity));
        Assert.That(LocationCatalog.GetByLevel(10), Is.SameAs(futureCity));
        Assert.That(LocationCatalog.GetByLevel(11), Is.SameAs(jungle));
        Assert.That(LocationCatalog.GetByLevel(20), Is.SameAs(jungle));
        Assert.That(LocationCatalog.GetByLevel(21), Is.Null);
    }

    [Test]
    public void ConfigsContainIndependentDifficultyAndGameplayData()
    {
        foreach (var location in new[] { ChallengeLocation.FutureCity, ChallengeLocation.Jungle })
        {
            var config = LocationCatalog.Get(location);
            Assert.That(config.Difficulty, Is.Not.Empty, location.ToString());
            Assert.That(config.EnvironmentLayers, Is.Not.Empty, location.ToString());
            Assert.That(config.Platforms.Variants, Is.Not.Empty, location.ToString());
            Assert.That(config.TryGetDifficulty(config.FirstLevel, out var difficulty), Is.True);
            Assert.That(difficulty.PlayerSpeed, Is.GreaterThan(0f));
            Assert.That(difficulty.ObstacleSpeedMultiplier, Is.GreaterThan(0f));
            Assert.That(config.MissionMenuBackground, Is.Not.Null);
        }
    }

    [Test]
    public void FirstTwentyLevelsUseTheirLocationDifficultySteps()
    {
        for (var level = 1; level <= 20; level++)
        {
            var definition = ChallengeLevelCatalog.Get(level);
            var config = LocationCatalog.Get(definition.Location);
            Assert.That(config.TryGetDifficulty(level, out var step), Is.True, "Level " + level);
            Assert.That(definition.PlayerSpeed, Is.EqualTo(step.PlayerSpeed), "Level " + level);
            Assert.That(definition.HazardChance, Is.EqualTo(step.HazardChance), "Level " + level);
            Assert.That(definition.ObstacleSpeedMultiplier,
                Is.EqualTo(step.ObstacleSpeedMultiplier), "Level " + level);
            Assert.That(definition.MovingPlatformChance,
                Is.EqualTo(step.MovingPlatformChance), "Level " + level);
        }
    }

    [Test]
    public void ConfiguredResourcesResolveForCurrentLocations()
    {
        foreach (var location in new[] { ChallengeLocation.FutureCity, ChallengeLocation.Jungle })
        {
            var config = LocationCatalog.Get(location);
            foreach (var layer in config.EnvironmentLayers)
                Assert.That(LocationTheme.LoadSprite(config, layer.ResourcePath), Is.Not.Null,
                    location + ": " + layer.ResourcePath);
            foreach (var platform in config.Platforms.Variants)
                Assert.That(LocationTheme.LoadSprite(config, platform.ResourcePath), Is.Not.Null,
                    location + ": " + platform.ResourcePath);
        }
    }

    [Test]
    public void EveryWeightedHazardTypeCanBeSelected()
    {
        var weights = new ChallengeHazardWeights
        {
            StaticBomb = 1f,
            MovingBomb = 1f,
            Laser = 1f,
            Drone = 1f,
            RotatingSpikes = 1f,
            PopUpSpikes = 1f,
            StickySurface = 1f,
            RotatingLaser = 1f
        };
        var expected = new[]
        {
            ThornType.Static, ThornType.Kinematic, ThornType.Laser, ThornType.Drone,
            ThornType.RotatingSpikes, ThornType.PopUpSpikes,
            ThornType.StickySurface, ThornType.RotatingLaser
        };

        for (var i = 0; i < expected.Length; i++)
            Assert.That(ThornController.SelectWeightedType(weights, (i + 0.5f) / expected.Length),
                Is.EqualTo(expected[i]));
    }

    [Test]
    public void CurrentLocationConfigsHaveAllRequiredRuntimeAssets()
    {
        foreach (var location in new[] { ChallengeLocation.FutureCity, ChallengeLocation.Jungle })
            Assert.That(LocationConfigValidator.Validate(LocationCatalog.Get(location)), Is.Empty,
                location.ToString());
    }

    [Test]
    public void ValidatorReportsWeightedHazardWithMissingAssets()
    {
        var config = ScriptableObject.CreateInstance<LocationConfig>();
        config.Platforms.Variants = new[]
        {
            new LocationPlatformVariant { ResourcePath = "Missing/platform" }
        };
        config.Difficulty = new[]
        {
            new LocationDifficultyStep
            {
                HazardWeights = new ChallengeHazardWeights { RotatingSpikes = 1f }
            }
        };

        try
        {
            Assert.That(LocationConfigValidator.Validate(config), Is.Not.Empty);
        }
        finally
        {
            Object.DestroyImmediate(config);
        }
    }
}
#endif
