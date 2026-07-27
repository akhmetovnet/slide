#if UNITY_INCLUDE_TESTS
using System.Linq;
using GameLogic;
using NUnit.Framework;
using UnityEngine;

public sealed class ChallengeBalanceTests
{
    [Test]
    public void CatalogContainsFiftySequentialLevels()
    {
        Assert.That(ChallengeLevelCatalog.All.Count, Is.EqualTo(ChallengeLevelCatalog.LevelCount));
        for (var i = 0; i < ChallengeLevelCatalog.All.Count; i++)
            Assert.That(ChallengeLevelCatalog.All[i].Level, Is.EqualTo(i + 1));
    }

    [Test]
    public void DifficultyProgressesWithoutLargeResets()
    {
        var levels = ChallengeLevelCatalog.All;
        for (var i = 1; i < levels.Count; i++)
        {
            Assert.That(levels[i].PlayerSpeed, Is.GreaterThanOrEqualTo(levels[i - 1].PlayerSpeed));
            Assert.That(levels[i].HazardChance, Is.GreaterThanOrEqualTo(levels[i - 1].HazardChance));
        }

        AssertMonotonicTargets(ChallengeObjectiveType.ReachPlatforms);
        AssertMonotonicTargets(ChallengeObjectiveType.CollectItems);
        AssertMonotonicTargets(ChallengeObjectiveType.RaceBot);
    }

    [Test]
    public void FirstTenLevelsUseFutureCityAndRespectIntroductionCurve()
    {
        for (var level = 1; level <= 10; level++)
            Assert.That(ChallengeLevelCatalog.Get(level).Location, Is.EqualTo(ChallengeLocation.FutureCity));

        Assert.That(ChallengeLevelCatalog.Get(1).HazardChance, Is.Zero);
        Assert.That(ChallengeLevelCatalog.Get(9).HazardWeights.MovingBomb, Is.LessThan(0.2f));
        Assert.That(ChallengeLevelCatalog.Get(10).HazardWeights.MovingBomb, Is.EqualTo(0.2f).Within(0.001f));
        Assert.That(ChallengeLevelCatalog.Get(10).HazardWeights.RotatingSpikes, Is.Zero);
        Assert.That(ChallengeLevelCatalog.Get(11).HazardWeights.RotatingSpikes, Is.GreaterThan(0f));
        Assert.That(ChallengeLevelCatalog.Get(20).HazardWeights.PopUpSpikes, Is.Zero);
        Assert.That(ChallengeLevelCatalog.Get(21).HazardWeights.PopUpSpikes, Is.GreaterThan(0f));
        Assert.That(ChallengeLevelCatalog.Get(30).HazardWeights.StickySurface, Is.Zero);
        Assert.That(ChallengeLevelCatalog.Get(31).HazardWeights.StickySurface, Is.GreaterThan(0f));
        Assert.That(ChallengeLevelCatalog.Get(40).HazardWeights.RotatingLaser, Is.Zero);
        Assert.That(ChallengeLevelCatalog.Get(41).HazardWeights.RotatingLaser, Is.GreaterThan(0f));
    }

    [Test]
    public void EveryLevelHasUsableObjectiveAndHazardData()
    {
        foreach (var level in ChallengeLevelCatalog.All)
        {
            if (level.Objective != ChallengeObjectiveType.CatchCriminal)
                Assert.That(level.TargetCount, Is.GreaterThan(0), $"Level {level.Level}");
            else
            {
                Assert.That(level.RivalStartLead, Is.GreaterThan(level.CaptureDistance));
                Assert.That(level.RivalEscapeLead, Is.GreaterThan(level.RivalStartLead));
            }

            Assert.That(GetWeightSum(level), Is.GreaterThan(0f), $"Level {level.Level}");
        }
    }

    [Test]
    public void ChallengeSpritesAreImportedAndLoadable()
    {
        for (var i = 0; i < ChallengeAssetCatalog.MissionItemCount; i++)
            Assert.That(ChallengeAssetCatalog.LoadMissionItem(i), Is.Not.Null, $"Mission item {i + 1}");

        var assignedItems = ChallengeLevelCatalog.All
            .Where(x => x.Objective == ChallengeObjectiveType.CollectItems)
            .Select(x => x.MissionItemVariant)
            .ToArray();
        Assert.That(assignedItems.All(x => x >= 0 && x < ChallengeAssetCatalog.MissionItemCount), Is.True);
        Assert.That(assignedItems.Distinct(), Is.EqualTo(Enumerable.Range(0, ChallengeAssetCatalog.MissionItemCount)));

        Assert.That(ChallengeAssetCatalog.LoadHazard("spike"), Is.Not.Null);
        Assert.That(ChallengeAssetCatalog.LoadHazard("sticky"), Is.Not.Null);
        Assert.That(ChallengeAssetCatalog.LoadHazardFrames("blades").Length, Is.EqualTo(10));
        Assert.That(ChallengeAssetCatalog.LoadHazardFrames("barrier").Length, Is.EqualTo(10));
    }

    private static void AssertMonotonicTargets(ChallengeObjectiveType objective)
    {
        var targets = ChallengeLevelCatalog.All
            .Where(x => x.Objective == objective)
            .Select(x => x.TargetCount)
            .ToArray();
        for (var i = 1; i < targets.Length; i++)
            Assert.That(targets[i], Is.GreaterThanOrEqualTo(targets[i - 1]));
    }

    private static float GetWeightSum(ChallengeLevelDefinition level)
    {
        return level.HazardWeights.StaticBomb +
               level.HazardWeights.MovingBomb +
               level.HazardWeights.Laser +
               level.HazardWeights.Drone +
               level.HazardWeights.RotatingSpikes +
               level.HazardWeights.PopUpSpikes +
               level.HazardWeights.StickySurface +
               level.HazardWeights.RotatingLaser;
    }
}
#endif
