using System;
using System.Linq;
using GameLogic;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ChallengeBatchValidator
{
    private static readonly ChallengeObjectiveType[] FirstTenObjectives =
    {
        ChallengeObjectiveType.ReachPlatforms,
        ChallengeObjectiveType.CollectItems,
        ChallengeObjectiveType.ReachPlatforms,
        ChallengeObjectiveType.CollectItems,
        ChallengeObjectiveType.RaceBot,
        ChallengeObjectiveType.ReachPlatforms,
        ChallengeObjectiveType.CollectItems,
        ChallengeObjectiveType.RaceBot,
        ChallengeObjectiveType.ReachPlatforms,
        ChallengeObjectiveType.CatchCriminal
    };

    private static readonly int[] FirstTenTargets = { 10, 5, 16, 8, 20, 24, 10, 24, 32, 0 };

    public static void Validate()
    {
        try
        {
            ValidateCatalog();
            ValidateFirstTenLevels();
            ValidateAssets();
            ValidateGameScene();

            Debug.Log("CHALLENGE_VALIDATION_SUCCESS: 50 levels, Future City 1-10, 16 items, 4 hazards and Game scene validated.");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void ValidateCatalog()
    {
        Assert(ChallengeLevelCatalog.All.Count == ChallengeLevelCatalog.LevelCount, "Catalog must contain 50 levels.");

        var previousSpeed = 0f;
        for (var level = 1; level <= ChallengeLevelCatalog.LevelCount; level++)
        {
            var definition = ChallengeLevelCatalog.Get(level);
            Assert(definition.Level == level, $"Level {level} is out of sequence.");
            Assert(definition.PlayerSpeed >= previousSpeed, $"Player speed decreases at level {level}.");
            Assert(definition.HazardChance >= 0f && definition.HazardChance <= 1f, $"Invalid hazard chance at level {level}.");
            Assert(definition.MovingPlatformChance >= 0f && definition.MovingPlatformChance <= 1f,
                $"Invalid moving-platform chance at level {level}.");
            Assert(definition.Objective == ChallengeObjectiveType.CatchCriminal || definition.TargetCount > 0,
                $"Level {level} has no target.");
            previousSpeed = definition.PlayerSpeed;
        }

        Assert(ChallengeLevelCatalog.All.Count(x => x.Objective == ChallengeObjectiveType.ReachPlatforms) == 16,
            "Unexpected platform mission count.");
        Assert(ChallengeLevelCatalog.All.Count(x => x.Objective == ChallengeObjectiveType.CollectItems) == 16,
            "Unexpected collect mission count.");
        Assert(ChallengeLevelCatalog.All.Count(x => x.Objective == ChallengeObjectiveType.RaceBot) == 10,
            "Unexpected race mission count.");
        Assert(ChallengeLevelCatalog.All.Count(x => x.Objective == ChallengeObjectiveType.CatchCriminal) == 8,
            "Unexpected criminal mission count.");

        var assignedItems = ChallengeLevelCatalog.All
            .Where(x => x.Objective == ChallengeObjectiveType.CollectItems)
            .Select(x => x.MissionItemVariant)
            .ToArray();
        Assert(assignedItems.SequenceEqual(Enumerable.Range(0, ChallengeAssetCatalog.MissionItemCount)),
            "Collect missions must use every mission item exactly once.");

        Assert(ChallengeLevelCatalog.Get(10).HazardWeights.RotatingSpikes == 0f &&
               ChallengeLevelCatalog.Get(11).HazardWeights.RotatingSpikes > 0f,
            "Rotating spikes must be introduced at level 11.");
        Assert(ChallengeLevelCatalog.Get(20).HazardWeights.PopUpSpikes == 0f &&
               ChallengeLevelCatalog.Get(21).HazardWeights.PopUpSpikes > 0f,
            "Pop-up spikes must be introduced at level 21.");
        Assert(ChallengeLevelCatalog.Get(30).HazardWeights.StickySurface == 0f &&
               ChallengeLevelCatalog.Get(31).HazardWeights.StickySurface > 0f,
            "Sticky surfaces must be introduced at level 31.");

        var lateWeights = ChallengeLevelCatalog.Get(41).HazardWeights;
        Assert(lateWeights.RotatingSpikes > 0f && lateWeights.PopUpSpikes > 0f &&
               lateWeights.StickySurface > 0f && lateWeights.RotatingLaser > 0f,
            "New hazards are not enabled in the late-game weight table.");
    }

    private static void ValidateFirstTenLevels()
    {
        var previousSpeed = 0f;
        for (var index = 0; index < FirstTenObjectives.Length; index++)
        {
            var definition = ChallengeLevelCatalog.Get(index + 1);
            Assert(definition.Location == ChallengeLocation.FutureCity, $"Level {index + 1} is not in Future City.");
            Assert(definition.Objective == FirstTenObjectives[index], $"Wrong objective at level {index + 1}.");
            Assert(definition.TargetCount == FirstTenTargets[index], $"Wrong target at level {index + 1}.");
            Assert(definition.PlayerSpeed >= previousSpeed, $"Speed curve decreases at level {index + 1}.");
            Assert(definition.HazardWeights.RotatingSpikes == 0f &&
                   definition.HazardWeights.PopUpSpikes == 0f &&
                   definition.HazardWeights.StickySurface == 0f &&
                   definition.HazardWeights.RotatingLaser == 0f,
                $"A late-game hazard is enabled too early at level {index + 1}.");
            previousSpeed = definition.PlayerSpeed;
        }

        Assert(ChallengeLevelCatalog.Get(1).HazardChance == 0f, "Level 1 must be hazard-free.");
        Assert(ChallengeLevelCatalog.Get(6).UsesStreamingField, "Level 6 must use the streaming field.");
        Assert(ChallengeLevelCatalog.Get(10).UsesStreamingField, "The criminal mission must use the streaming field.");
    }

    private static void ValidateAssets()
    {
        for (var index = 0; index < ChallengeAssetCatalog.MissionItemCount; index++)
            Assert(ChallengeAssetCatalog.LoadMissionItem(index) != null, $"Mission item {index + 1:00} is missing.");

        Assert(ChallengeAssetCatalog.LoadHazardFrames("blades").Length == 10, "Rotating-spike frames are incomplete.");
        Assert(ChallengeAssetCatalog.LoadHazardFrames("barrier").Length == 10, "Rotating-laser frames are incomplete.");
        Assert(ChallengeAssetCatalog.LoadHazard("spike") != null, "Pop-up spike sprite is missing.");
        Assert(ChallengeAssetCatalog.LoadHazard("sticky") != null, "Sticky-surface sprite is missing.");
    }

    private static void ValidateGameScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);

        Assert(Find<GameController>() != null, "GameController is missing from Game scene.");
        Assert(Find<ObjectController>() != null, "ObjectController is missing from Game scene.");
        Assert(Find<HeroController>() != null, "HeroController is missing from Game scene.");
        Assert(Find<UIController>() != null, "UIController is missing from Game scene.");
    }

    private static T Find<T>() where T : UnityEngine.Object
    {
        return UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include).FirstOrDefault();
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
