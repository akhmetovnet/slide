#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.Linq;
using GameLogic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ChallengeRuntimeSmokeTests
{
    [UnityTest]
    public IEnumerator FutureCityLevelsOneToTenStartWithoutRuntimeErrors()
    {
        var previousLevel = PlayerPrefs.GetInt("Level", 1);
        var hadUnlockedLevel = PlayerPrefs.HasKey(ChallengeProgress.UnlockedLevelKey);
        var previousUnlockedLevel = PlayerPrefs.GetInt(ChallengeProgress.UnlockedLevelKey, 1);
        var hadSelectedLevel = PlayerPrefs.HasKey(ChallengeProgress.SelectedLevelKey);
        var previousSelectedLevel = PlayerPrefs.GetInt(ChallengeProgress.SelectedLevelKey, 1);
        var previousTutorial = PlayerPrefs.GetInt("Tutorial", 0);
        yield return new EnterPlayMode();

        try
        {
            var startPlatform = FutureCityTheme.LoadSprite("Start/start_platform");
            Assert.That(startPlatform, Is.Not.Null);
            Assert.That(startPlatform.pivot.y, Is.EqualTo(16f).Within(0.1f),
                "The Future City start background pivot must stay on the platform surface.");

            for (var level = 1; level <= 10; level++)
            {
                PlayerPrefs.SetInt("Level", level);
                PlayerPrefs.SetInt(ChallengeProgress.UnlockedLevelKey, level);
                PlayerPrefs.SetInt(ChallengeProgress.SelectedLevelKey, level);
                PlayerPrefs.SetInt("Tutorial", 1);
                SceneManager.LoadScene("Game");
                yield return null;
                yield return null;

                var gameController = Object
                    .FindObjectsByType<GameController>(FindObjectsInactive.Exclude)
                    .FirstOrDefault();
                Assert.That(gameController, Is.Not.Null, $"GameController is missing at level {level}.");

                gameController.Mode = GameMode.Challenge;
                gameController.PlayGame();
                yield return null;

                Assert.That(gameController.CurrentLevel, Is.EqualTo(level));
                Assert.That(gameController.CurrentChallengeDefinition.Location, Is.EqualTo(ChallengeLocation.FutureCity));
                Assert.That(gameController.ChallengeObjective, Is.Not.Null);
                Assert.That(gameController.ChallengeObjective.IsActive, Is.True);
                Assert.That(gameController.ChallengeObjective.Definition.Level, Is.EqualTo(level));

                var lines = Object.FindObjectsByType<LineController>(FindObjectsInactive.Exclude);
                Assert.That(lines.Length, Is.GreaterThan(0), $"No platforms were generated at level {level}.");
                foreach (var line in lines)
                {
                    var renderer = line.GetComponent<SpriteRenderer>();
                    if (renderer == null || renderer.sprite == null ||
                        !renderer.sprite.name.StartsWith("platform_"))
                        continue;

                    var direction = line.GetDirection();
                    Assert.That(Mathf.Sign(direction.x), Is.EqualTo(-Mathf.Sign(line.AngleDegree)),
                        $"Downhill direction does not match {renderer.sprite.name}.");
                    var colliderAngle = Mathf.Abs(Mathf.DeltaAngle(
                        0f,
                        line.Collider.transform.localEulerAngles.z));
                    Assert.That(colliderAngle, Is.EqualTo(Mathf.Abs(line.AngleDegree)).Within(0.1f),
                        $"Collider angle does not match {renderer.sprite.name}.");

                    var needsHorizontalFlip = renderer.sprite.name.StartsWith("platform_2");
                    Assert.That(renderer.flipX, Is.EqualTo(needsHorizontalFlip),
                        $"Sprite orientation does not match {renderer.sprite.name}.");
                }

                if (level <= 3)
                    Assert.That(lines.All(x => x.GetComponent<SpriteRenderer>().sprite.name.StartsWith("platform_1")), Is.True);
                if (level == 4)
                    Assert.That(lines.Any(x => x.GetComponent<SpriteRenderer>().sprite.name.StartsWith("platform_2")), Is.True);
                if (level == 8)
                    Assert.That(lines.Any(x => x.GetComponent<SpriteRenderer>().sprite.name.StartsWith("platform_3")), Is.True);
            }

            var referenceObject = new GameObject("Hazard Test Reference");
            var referenceRenderer = referenceObject.AddComponent<SpriteRenderer>();
            var hazardObject = new GameObject("Hazard Runtime Test");
            var hazard = hazardObject.AddComponent<ChallengeHazardRuntime>();
            hazard.Initialize(referenceRenderer);

            var hazardTypes = new[]
            {
                ThornType.RotatingSpikes,
                ThornType.PopUpSpikes,
                ThornType.StickySurface,
                ThornType.RotatingLaser
            };
            foreach (var hazardType in hazardTypes)
            {
                hazard.Configure(hazardType, 1f);
                yield return null;
                Assert.That(hazardObject.activeSelf, Is.True, $"{hazardType} is inactive.");
                Assert.That(hazardObject.GetComponent<SpriteRenderer>().sprite, Is.Not.Null, $"{hazardType} has no sprite.");
                Assert.That(hazardObject.GetComponent<Collider2D>(), Is.Not.Null, $"{hazardType} has no collider.");
            }

            Object.Destroy(hazardObject);
            Object.Destroy(referenceObject);
        }
        finally
        {
            PlayerPrefs.SetInt("Level", previousLevel);
            if (hadUnlockedLevel)
                PlayerPrefs.SetInt(ChallengeProgress.UnlockedLevelKey, previousUnlockedLevel);
            else
                PlayerPrefs.DeleteKey(ChallengeProgress.UnlockedLevelKey);
            if (hadSelectedLevel)
                PlayerPrefs.SetInt(ChallengeProgress.SelectedLevelKey, previousSelectedLevel);
            else
                PlayerPrefs.DeleteKey(ChallengeProgress.SelectedLevelKey);
            PlayerPrefs.SetInt("Tutorial", previousTutorial);
        }

        yield return new ExitPlayMode();
    }
}
#endif
