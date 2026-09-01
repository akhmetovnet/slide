#if UNITY_INCLUDE_TESTS
using System.Linq;
using GameLogic;
using NUnit.Framework;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CollectibleAndMenuLayoutTests
{
    [Test]
    public void FiveMissionItemsShareOneBoundedVisualSize()
    {
        Assert.That(ChallengeAssetCatalog.MissionItemCount, Is.EqualTo(5));
        Assert.That(CollectibleDefinition.MissionItemVisualDiameter, Is.EqualTo(0.34f).Within(0.0001f));

        for (var index = 0; index < ChallengeAssetCatalog.MissionItemCount; index++)
        {
            var sprite = ChallengeAssetCatalog.LoadMissionItem(index);
            Assert.That(sprite, Is.Not.Null, $"Mission item {index + 1}");

            var scale = CollectibleDefinition.GetMissionItemScale(sprite);
            var visibleSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) * scale;
            Assert.That(visibleSize, Is.EqualTo(CollectibleDefinition.MissionItemVisualDiameter).Within(0.0001f));
            Assert.That(visibleSize, Is.LessThanOrEqualTo(CollectibleDefinition.MaxMissionItemVisualDiameter));
            var colliderRadius = CollectibleDefinition.GetMissionItemColliderRadius(sprite, scale);
            Assert.That(colliderRadius, Is.GreaterThan(0f));
            Assert.That(colliderRadius * scale,
                Is.EqualTo(Mathf.Min(sprite.bounds.size.x, sprite.bounds.size.y) * scale * 0.5f).Within(0.0001f));
        }
    }

    [Test]
    public void MissionItemsUseEnemyLaneWhileCoinsAndAccelerationUseBonusLane()
    {
        const float positionX = 0.75f;
        const float angle = 0.2f;
        const int lineIndex = 4;
        var slopeOffset = Mathf.Sin(angle) * positionX;

        var missionItemPosition = BonusController.GetSpawnPosition(BonusType.MissionItem, positionX, angle, lineIndex);
        var coinPosition = BonusController.GetSpawnPosition(BonusType.Coin, positionX, angle, lineIndex);
        var accelerationPosition = BonusController.GetSpawnPosition(BonusType.Acceleration, positionX, angle, lineIndex);

        Assert.That(missionItemPosition.y,
            Is.EqualTo(BonusController.EnemyLaneBaseY - lineIndex * 2f + slopeOffset).Within(0.0001f));
        Assert.That(coinPosition.y,
            Is.EqualTo(BonusController.BonusLaneBaseY - lineIndex * 2f + slopeOffset).Within(0.0001f));
        Assert.That(accelerationPosition.y, Is.EqualTo(coinPosition.y).Within(0.0001f));
    }

    [Test]
    public void MissionItemLinesAreReservedFromHazards()
    {
        Assert.That(ObjectController.IsLineAvailableForHazard(BonusType.MissionItem), Is.False);
        Assert.That(ObjectController.IsLineAvailableForHazard(BonusType.Coin), Is.True);
        Assert.That(ObjectController.IsLineAvailableForHazard(BonusType.Acceleration), Is.True);
        Assert.That(ObjectController.IsLineAvailableForHazard(null), Is.True);
    }

    [Test]
    public void DischargeSheetIsPointFilteredAndBoundToPooledThorn()
    {
        const string vfxPath = "Assets/Sprites/NewSprites/obj fx/vfx_discharge_1.png";
        var importer = AssetImporter.GetAtPath(vfxPath) as TextureImporter;
        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));

        var frames = AssetDatabase.LoadAllAssetsAtPath(vfxPath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
        Assert.That(frames, Has.Length.EqualTo(6));
        Assert.That(frames.All(frame => frame.rect.size == new Vector2(46f, 46f)), Is.True);

        var thorn = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Thorn.prefab");
        var serialized = new SerializedObject(thorn.GetComponent<ThornController>());
        Assert.That(serialized.FindProperty("_dischargeFrames").arraySize, Is.EqualTo(frames.Length));
    }

    [Test]
    public void MainAndMissionMenusUseSafeAreasAndScreenSizeScaling()
    {
        AssertMenuLayout("Assets/Scenes/Game.unity", "MainPanel", new Vector2(320f, 576f), false);
        AssertMenuLayout("Assets/Scenes/MissionMenu.unity", "MissionScroll", new Vector2(1280f, 2304f), false);
    }

    [Test]
    public void RestartMenusUseFullReferenceLayersAndContainNoLeaderboard()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Additive);
        try
        {
            var roots = scene.GetRootGameObjects();
            var transforms = roots.SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            Assert.That(transforms.Any(item => item.name == "HightscoreTable"), Is.False);

            var continueView = roots.SelectMany(root => root.GetComponentsInChildren<ContinueOfferView>(true)).Single();
            var resultView = roots.SelectMany(root => root.GetComponentsInChildren<ResultMenuView>(true)).Single();
            Assert.That(continueView.GetComponentsInChildren<Button>(true).Count(button => button.interactable),
                Is.GreaterThanOrEqualTo(3));
            Assert.That(resultView.GetComponentsInChildren<Button>(true).Length, Is.GreaterThanOrEqualTo(4));

            var storeOffer = resultView.GetComponentsInChildren<Transform>(true)
                .Single(item => item.name == "StoreOffer");
            Assert.That(storeOffer.childCount, Is.Zero);
            Assert.That(storeOffer.GetComponent<RectTransform>().anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(storeOffer.GetComponent<RectTransform>().anchorMax, Is.EqualTo(Vector2.one));

            var continuePanel = transforms.Single(item => item.name == "ContinuePanel");
            Assert.That(continuePanel.GetComponent<Image>().raycastTarget, Is.False);
            Assert.That(continuePanel.GetComponent<Button>().enabled, Is.False);

            foreach (var path in new[]
                     {
                         "Assets/Sprites/RestartMenu/result_plate.png",
                         "Assets/Sprites/RestartMenu/result_no_ads.png",
                         "Assets/Sprites/ContinueOffer/plate.png"
                     })
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point), path);

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                Assert.That(settings.spriteMeshType, Is.EqualTo(SpriteMeshType.FullRect), path);
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void ContinueOfferUsesSingleReadableValueLayerPerMetric()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Additive);
        try
        {
            var roots = scene.GetRootGameObjects();
            var continueView = roots.SelectMany(root => root.GetComponentsInChildren<ContinueOfferView>(true)).Single();
            var transforms = continueView.GetComponentsInChildren<Transform>(true);
            var valueFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SDONE_0 SDF.asset");

            Assert.That(transforms.Any(item => item.name.EndsWith("ValueMask")), Is.False);
            foreach (var name in new[] { "BalanceValue", "CollectedValue", "PassedValue", "Price" })
            {
                var value = transforms.Where(item => item.name == name)
                    .Select(item => item.GetComponent<TMP_Text>())
                    .Single();
                Assert.That(value, Is.Not.Null, name);
                Assert.That(value.font, Is.SameAs(valueFont), name);
                Assert.That(value.raycastTarget, Is.False, name);
                Assert.That(value.textWrappingMode, Is.EqualTo(TextWrappingModes.NoWrap), name);
                Assert.That(value.fontSizeMax, Is.GreaterThanOrEqualTo(36f), name);
                Assert.That(value.rectTransform.anchorMin.x, Is.InRange(0f, 1f), name);
                Assert.That(value.rectTransform.anchorMin.y, Is.InRange(0f, 1f), name);
                Assert.That(value.rectTransform.anchorMax.x, Is.InRange(0f, 1f), name);
                Assert.That(value.rectTransform.anchorMax.y, Is.InRange(0f, 1f), name);
            }

            foreach (var name in new[] { "CoinsArtLeft", "CoinsArtValueBackground", "CoinsArtRight" })
                Assert.That(transforms.Single(item => item.name == name).GetComponent<RawImage>(), Is.Not.Null, name);

            foreach (var name in new[] { "RewardedContinue", "CoinsContinue", "Skip" })
            {
                var button = transforms.Single(item => item.name == name).GetComponent<Button>();
                Assert.That(button, Is.Not.Null, name);
                Assert.That(button.targetGraphic.raycastTarget, Is.True, name);
            }

            var rewardedTransform = transforms.Single(item => item.name == "RewardedContinue") as RectTransform;
            Assert.That(rewardedTransform.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(rewardedTransform.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
            Assert.That(rewardedTransform.anchoredPosition, Is.Not.EqualTo(Vector2.zero));

            var continueViewSerialized = new SerializedObject(continueView);
            Assert.That(continueViewSerialized.FindProperty("_passedValueColor").colorValue,
                Is.EqualTo((Color)new Color32(7, 48, 65, 255)));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void RestartOfferOrderCyclesThroughExistingStoreCatalog()
    {
        var keys = new[] { "Skin9", "Skin10", "Skin11" };
        var hadKeys = keys.Select(PlayerPrefs.HasKey).ToArray();
        var values = keys.Select(key => PlayerPrefs.GetInt(key, 0)).ToArray();
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Additive);
        try
        {
            foreach (var key in keys)
                PlayerPrefs.SetInt(key, 0);

            var store = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<StoreController>(true))
                .Single();
            Assert.That(store.GetRestartOffer(0).ProductId, Is.EqualTo("skin10"));
            Assert.That(store.GetRestartOffer(1).ProductId, Is.EqualTo("skin11"));
            Assert.That(store.GetRestartOffer(2).ProductId, Is.EqualTo("skin12"));
            Assert.That(store.GetRestartOffer(3).ProductId, Is.EqualTo("skin10"));
        }
        finally
        {
            for (var index = 0; index < keys.Length; index++)
            {
                if (hadKeys[index])
                    PlayerPrefs.SetInt(keys[index], values[index]);
                else
                    PlayerPrefs.DeleteKey(keys[index]);
            }

            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void AssertMenuLayout(
        string scenePath,
        string safeAreaRoot,
        Vector2 referenceResolution,
        bool requiresReservedHeader)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        try
        {
            var roots = scene.GetRootGameObjects();
            var safeRoot = roots.SelectMany(root => root.GetComponentsInChildren<SafeAreaLayout>(true))
                .FirstOrDefault(layout => layout.name == safeAreaRoot);
            Assert.That(safeRoot, Is.Not.Null);
            var scaler = safeRoot.GetComponentInParent<CanvasScaler>();
            Assert.That(scaler, Is.Not.Null);
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight));
            Assert.That(scaler.referenceResolution, Is.EqualTo(referenceResolution));
            if (requiresReservedHeader)
            {
                var serialized = new SerializedObject(safeRoot);
                Assert.That(serialized.FindProperty("_topReservedSpace").floatValue, Is.GreaterThanOrEqualTo(500f));
            }

            var missionButtons = roots.SelectMany(root => root.GetComponentsInChildren<Button>(true))
                .Where(button => button.name.StartsWith("Mission_"))
                .ToArray();
            foreach (var button in missionButtons)
            {
                var size = (button.transform as RectTransform).rect.size;
                Assert.That(size.x, Is.GreaterThanOrEqualTo(44f));
                Assert.That(size.y, Is.GreaterThanOrEqualTo(44f));
            }
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
#endif
