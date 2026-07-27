#if UNITY_INCLUDE_TESTS
using System.Linq;
using GameLogic;
using NUnit.Framework;
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

        for (var index = 0; index < ChallengeAssetCatalog.MissionItemCount; index++)
        {
            var sprite = ChallengeAssetCatalog.LoadMissionItem(index);
            Assert.That(sprite, Is.Not.Null, $"Mission item {index + 1}");

            var scale = CollectibleDefinition.GetMissionItemScale(sprite);
            var visibleSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y) * scale;
            Assert.That(visibleSize, Is.EqualTo(CollectibleDefinition.MissionItemVisualDiameter).Within(0.0001f));
            Assert.That(visibleSize, Is.LessThanOrEqualTo(CollectibleDefinition.MaxMissionItemVisualDiameter));
            Assert.That(CollectibleDefinition.GetMissionItemColliderRadius(sprite, scale), Is.GreaterThan(0f));
        }
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
