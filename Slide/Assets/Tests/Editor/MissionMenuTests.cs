#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.Linq;
using GameLogic;
using NUnit.Framework;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class MissionMenuTests
{
    private readonly string[] _progressKeys =
    {
        ChallengeProgress.LegacyLevelKey,
        ChallengeProgress.UnlockedLevelKey,
        ChallengeProgress.SelectedLevelKey,
        ChallengeProgress.AutoStartKey,
        ChallengeProgress.SeenUnlockedLevelKey
    };

    private bool[] _hadKeys;
    private int[] _previousValues;

    [SetUp]
    public void SetUp()
    {
        _hadKeys = _progressKeys.Select(PlayerPrefs.HasKey).ToArray();
        _previousValues = _progressKeys.Select(key => PlayerPrefs.GetInt(key, 0)).ToArray();
        foreach (var key in _progressKeys)
            PlayerPrefs.DeleteKey(key);
    }

    [TearDown]
    public void TearDown()
    {
        for (var i = 0; i < _progressKeys.Length; i++)
        {
            if (_hadKeys[i])
                PlayerPrefs.SetInt(_progressKeys[i], _previousValues[i]);
            else
                PlayerPrefs.DeleteKey(_progressKeys[i]);
        }
    }

    [Test]
    public void SelectingCompletedMissionDoesNotRelockProgress()
    {
        PlayerPrefs.SetInt(ChallengeProgress.LegacyLevelKey, 18);
        ChallengeProgress.Initialize();

        Assert.That(ChallengeProgress.HighestUnlockedLevel, Is.EqualTo(18));
        Assert.That(ChallengeProgress.SelectedLevel, Is.EqualTo(18));
        Assert.That(ChallengeProgress.SelectLevel(4), Is.True);
        Assert.That(ChallengeProgress.SelectedLevel, Is.EqualTo(4));
        Assert.That(ChallengeProgress.HighestUnlockedLevel, Is.EqualTo(18));

        Assert.That(ChallengeProgress.CompleteLevel(4), Is.EqualTo(5));
        Assert.That(ChallengeProgress.SelectedLevel, Is.EqualTo(5));
        Assert.That(ChallengeProgress.HighestUnlockedLevel, Is.EqualTo(18));
        Assert.That(ChallengeProgress.SelectLevel(19), Is.False);
    }

    [Test]
    public void LocationOrderMatchesMissionMapChapters()
    {
        Assert.That(ChallengeLevelCatalog.Get(1).Location, Is.EqualTo(ChallengeLocation.FutureCity));
        Assert.That(ChallengeLevelCatalog.Get(11).Location, Is.EqualTo(ChallengeLocation.Jungle));
        Assert.That(ChallengeLevelCatalog.Get(21).Location, Is.EqualTo(ChallengeLocation.SpaceStation));
        Assert.That(ChallengeLevelCatalog.Get(31).Location, Is.EqualTo(ChallengeLocation.Cyberpunk));
    }

    [Test]
    public void MissionMenuSceneContainsScrollableFiftyLevelRouteAndFourBackgrounds()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MissionMenu.unity", OpenSceneMode.Additive);
        try
        {
            var roots = scene.GetRootGameObjects();
            var controller = roots
                .SelectMany(root => root.GetComponentsInChildren<MissionMenuController>(true))
                .FirstOrDefault();
            Assert.That(controller, Is.Not.Null);

            var missionButtons = roots
                .SelectMany(root => root.GetComponentsInChildren<Button>(true))
                .Where(button => button.name.StartsWith("Mission_"))
                .ToArray();
            Assert.That(missionButtons.Length, Is.EqualTo(ChallengeLevelCatalog.LevelCount));

            var serialized = new SerializedObject(controller);
            Assert.That(serialized.FindProperty("_backgroundSprites").arraySize, Is.EqualTo(4));
            Assert.That(
                serialized.FindProperty("_missionButtons").arraySize,
                Is.EqualTo(ChallengeLevelCatalog.LevelCount));
            Assert.That(serialized.FindProperty("_activeArrowFrames").arraySize, Is.EqualTo(4));
            Assert.That(
                roots.SelectMany(root => root.GetComponentsInChildren<ScrollRect>(true)).Count(),
                Is.EqualTo(1));
            Assert.That(
                roots.SelectMany(root => root.GetComponentsInChildren<Button>(true))
                    .Any(button => button.name == "NoAdsButton"),
                Is.True);
            Assert.That(
                roots.SelectMany(root => root.GetComponentsInChildren<RectTransform>(true))
                    .Any(rect => rect.name == "ActiveSelectionFrame"),
                Is.True);
            Assert.That(roots.Any(root => root.name == "Canvas"), Is.True);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [UnityTest]
    public IEnumerator PrepareWindowOpensFromMissionSelection()
    {
        PlayerPrefs.SetInt(ChallengeProgress.LegacyLevelKey, 1);
        PlayerPrefs.SetInt(ChallengeProgress.UnlockedLevelKey, 1);
        PlayerPrefs.SetInt(ChallengeProgress.SelectedLevelKey, 1);

        yield return new EnterPlayMode();
        SceneManager.LoadScene("MissionMenu");
        yield return null;
        yield return null;

        var missionButton = Object
            .FindObjectsByType<Button>(FindObjectsInactive.Include)
            .FirstOrDefault(button => button.name == "Mission_01");
        var preparePanel = Resources
            .FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(item => item.name == "PreparePanel" && item.scene.IsValid());

        Assert.That(missionButton, Is.Not.Null);
        Assert.That(preparePanel, Is.Not.Null);
        Assert.That(preparePanel.activeSelf, Is.False);

        missionButton.onClick.Invoke();
        yield return null;

        Assert.That(preparePanel.activeSelf, Is.True);
        Assert.That(preparePanel.GetComponent<CanvasGroup>().blocksRaycasts, Is.True);

        yield return new ExitPlayMode();
    }
}
#endif
