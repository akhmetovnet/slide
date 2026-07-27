#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using GameLogic;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MissionMenuSceneBuilder
{
    private const string SpritesPath = "Assets/Sprites/MissionMenu";
    private const string ScenePath = "Assets/Scenes/MissionMenu.unity";
    private const string MainFontPath = "Assets/Font/lofty-s SDF.asset";
    private const string FallbackFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    private static readonly Color32 Cyan = new Color32(92, 244, 247, 255);
    private static readonly Color32 PaleCyan = new Color32(214, 255, 255, 255);
    private static readonly Color32 Navy = new Color32(5, 34, 48, 255);
    private static readonly Color32 Yellow = new Color32(255, 224, 35, 255);

    [MenuItem("Slide/Build Mission Menu Scene")]
    public static void BuildMissionMenuScene()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var previousScenePath = SceneManager.GetActiveScene().path;

        AssetDatabase.Refresh();
        ConfigureSprites();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateEventSystem();

        var canvasRoot = CreateCanvas();
        var font = LoadFont();

        var background = CreateFullImage("LocationBackground", Sprite("background_future_sky"), canvasRoot, Color.white);
        var futureCityBackgroundRoot = CreateFullRect("FutureCityBackground", canvasRoot);
        var futureCityBackgroundGroup = futureCityBackgroundRoot.gameObject.AddComponent<CanvasGroup>();
        CreateBottomImage("FarCity", Sprite("background_future_city_4"), futureCityBackgroundRoot, new Vector2(1280f, 1848f));
        CreateBottomImage("MidCityA", Sprite("background_future_city_3"), futureCityBackgroundRoot, new Vector2(1280f, 1226f));
        CreateBottomImage("MidCityB", Sprite("background_future_city_2"), futureCityBackgroundRoot, new Vector2(1280f, 1340f));
        CreateBottomImage("NearCity", Sprite("background_future_city_1"), futureCityBackgroundRoot, new Vector2(1280f, 1068f));
        CreateFullSolid("BackgroundTint", canvasRoot, new Color(0.02f, 0.12f, 0.18f, 0.27f), false);

        var scrollRoot = CreateFullRect("MissionScroll", canvasRoot);
        scrollRoot.gameObject.AddComponent<SafeAreaLayout>();
        var scrollSurface = scrollRoot.gameObject.AddComponent<Image>();
        scrollSurface.color = Color.clear;
        scrollSurface.raycastTarget = true;

        var viewport = CreateFullRect("Viewport", scrollRoot);
        viewport.gameObject.AddComponent<RectMask2D>();

        const float firstNodeOffset = 420f;
        const float nodeSpacing = 300f;
        var contentHeight = firstNodeOffset +
                            (ChallengeLevelCatalog.LevelCount - 1) * nodeSpacing +
                            500f;
        var mapRoot = CreateRect(
            "MapContent",
            viewport,
            Vector2.zero,
            new Vector2(1280f, contentHeight));
        mapRoot.anchorMin = mapRoot.anchorMax = new Vector2(0.5f, 1f);
        mapRoot.pivot = new Vector2(0.5f, 1f);
        mapRoot.anchoredPosition = Vector2.zero;

        var mapGroup = mapRoot.gameObject.AddComponent<CanvasGroup>();
        var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.content = mapRoot;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.08f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 65f;

        var nodePositions = new Vector2[ChallengeLevelCatalog.LevelCount];
        for (var i = 0; i < nodePositions.Length; i++)
            nodePositions[i] = new Vector2(0f, -firstNodeOffset - i * nodeSpacing);

        var connectors = new Image[nodePositions.Length - 1];
        for (var i = 0; i < nodePositions.Length - 1; i++)
        {
            connectors[i] = CreateConnector(
                $"Connector_{i + 1:00}",
                Sprite("connector"),
                mapRoot,
                nodePositions[i],
                nodePositions[i + 1]);
            connectors[i].gameObject.SetActive(false);
        }

        var selectionFrame = CreateSelectionFrame(mapRoot, nodePositions[0]);
        var missionButtons = new Button[nodePositions.Length];
        var missionImages = new Image[nodePositions.Length];
        var missionLabels = new TMP_Text[nodePositions.Length];

        for (var i = 0; i < nodePositions.Length; i++)
        {
            var button = CreateButton(
                $"Mission_{i + 1:00}",
                i == 0 ? Sprite("mission_active") : Sprite("mission_locked"),
                i == 0 ? Sprite("mission_active_pressed") : Sprite("mission_locked"),
                mapRoot,
                nodePositions[i],
                new Vector2(216f, 132f),
                false);
            SetTopAnchored(button.transform as RectTransform, nodePositions[i]);
            missionButtons[i] = button;
            missionImages[i] = button.GetComponent<Image>();

            var label = CreateText(
                "MissionNumber", button.transform as RectTransform, i == 0 ? "#1" : string.Empty,
                new Vector2(-142f, 94f), new Vector2(150f, 50f), 30f, PaleCyan, font);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            missionLabels[i] = label;
        }

        var arrowFrames = Sprites("active_arrow");
        var activeArrow = CreateImage(
            "ActiveArrow", arrowFrames.FirstOrDefault(), mapRoot,
            nodePositions[0] + new Vector2(0f, 155f), new Vector2(120f, 212f));
        SetTopAnchored(activeArrow.rectTransform, nodePositions[0] + new Vector2(0f, 155f));
        activeArrow.raycastTarget = false;

        var glass = CreateFullImage("Glass", Sprite("glass"), canvasRoot, new Color(1f, 1f, 1f, 0.26f));
        glass.raycastTarget = false;

        var backButton = CreateButton(
            "BackButton", Sprite("button_back"), Sprite("button_back_pressed"),
            canvasRoot, new Vector2(-500f, 1010f), new Vector2(224f, 164f));
        var settingsButton = CreateButton(
            "SettingsButton", Sprite("button_settings"), Sprite("button_settings_pressed"),
            canvasRoot, new Vector2(500f, 1010f), new Vector2(224f, 164f));
        var noAdsButton = CreateButton(
            "NoAdsButton", Sprite("button_no_ads"), Sprite("button_no_ads_pressed"),
            canvasRoot, new Vector2(500f, 810f), new Vector2(224f, 164f));
        var storeButton = CreateButton(
            "StoreButton", Sprite("button_store"), null,
            canvasRoot, new Vector2(500f, 615f), new Vector2(148f, 156f));

        var transitionFlash = CreateFullSolid(
            "TransitionFlash", canvasRoot, new Color(0.75f, 1f, 1f, 0f), false);
        transitionFlash.raycastTarget = false;
        var lightningLine = CreateImage(
            "LightningLine", Sprite("lightning_line"), canvasRoot,
            Vector2.zero, new Vector2(1140f, 36f));
        lightningLine.color = new Color(0.58f, 1f, 1f, 0f);
        lightningLine.raycastTarget = false;

        CreatePreparePanel(
            canvasRoot,
            font,
            out var preparePanel,
            out var prepareGroup,
            out var prepareMission,
            out var prepareObjective,
            out var startButton,
            out var cancelPrepareButton);

        CreateSettingsPanel(
            canvasRoot,
            font,
            out var settingsPanel,
            out var settingsGroup,
            out var soundToggle,
            out var vibrationToggle,
            out var closeSettingsButton);

        var controller = new GameObject("MissionMenuController").AddComponent<MissionMenuController>();
        controller.transform.SetParent(canvasRoot, false);
        ConfigureController(
            controller,
            background,
            glass,
            futureCityBackgroundGroup,
            mapGroup,
            scrollRect,
            viewport,
            mapRoot,
            selectionFrame,
            new[]
            {
                Sprite("background_future_sky"),
                Sprite("background_jungle"),
                Sprite("background_space_station"),
                Sprite("background_cyberpunk")
            },
            connectors,
            missionButtons,
            missionImages,
            missionLabels,
            activeArrow.rectTransform,
            activeArrow,
            arrowFrames,
            backButton,
            storeButton,
            settingsButton,
            noAdsButton,
            preparePanel,
            prepareGroup,
            prepareMission,
            prepareObjective,
            startButton,
            cancelPrepareButton,
            settingsPanel,
            settingsGroup,
            soundToggle,
            vibrationToggle,
            closeSettingsButton,
            transitionFlash,
            lightningLine);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath))
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log($"Mission menu scene rebuilt: {ScenePath}");
    }

    private static void CreatePreparePanel(
        RectTransform canvasRoot,
        TMP_FontAsset font,
        out GameObject panel,
        out CanvasGroup group,
        out TMP_Text mission,
        out TMP_Text objective,
        out Button startButton,
        out Button cancelButton)
    {
        panel = CreateFullRect("PreparePanel", canvasRoot).gameObject;
        var dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0.04f, 0.07f, 0.86f);
        dim.raycastTarget = true;

        var frame = CreateRect("PrepareFrame", panel.transform as RectTransform, Vector2.zero, new Vector2(980f, 790f));
        var frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.color = new Color(0.02f, 0.14f, 0.2f, 0.98f);
        var outline = frame.gameObject.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(5f, -5f);

        CreateText("PrepareTitle", frame, "ПРИГОТОВЬТЕСЬ", new Vector2(0f, 278f), new Vector2(820f, 90f), 58f, Yellow, font);
        mission = CreateText("PrepareMission", frame, "МИССИЯ 1", new Vector2(0f, 165f), new Vector2(720f, 64f), 40f, Cyan, font);
        objective = CreateText(
            "PrepareObjective", frame, string.Empty, new Vector2(0f, -35f),
            new Vector2(800f, 280f), 31f, PaleCyan, font);

        startButton = CreateButton(
            "StartButton", Sprite("mission_active"), Sprite("mission_active_pressed"),
            frame, new Vector2(180f, -292f), new Vector2(260f, 172f));
        CreateText("StartLabel", startButton.transform as RectTransform, "СТАРТ", Vector2.zero, new Vector2(190f, 62f), 38f, Navy, font);

        cancelButton = CreateButton(
            "CancelButton", Sprite("button_back"), Sprite("button_back_pressed"),
            frame, new Vector2(-245f, -292f), new Vector2(150f, 112f));

        group = panel.AddComponent<CanvasGroup>();
        panel.SetActive(false);
    }

    private static void CreateSettingsPanel(
        RectTransform canvasRoot,
        TMP_FontAsset font,
        out GameObject panel,
        out CanvasGroup group,
        out Toggle soundToggle,
        out Toggle vibrationToggle,
        out Button closeButton)
    {
        panel = CreateFullRect("SettingsPanel", canvasRoot).gameObject;
        var dim = panel.AddComponent<Image>();
        dim.color = new Color(0f, 0.04f, 0.07f, 0.82f);
        dim.raycastTarget = true;

        var frame = CreateRect("SettingsFrame", panel.transform as RectTransform, Vector2.zero, new Vector2(820f, 650f));
        var frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.color = new Color(0.02f, 0.14f, 0.2f, 0.98f);
        var outline = frame.gameObject.AddComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(5f, -5f);

        CreateText("SettingsTitle", frame, "НАСТРОЙКИ", new Vector2(0f, 220f), new Vector2(680f, 80f), 52f, Yellow, font);
        soundToggle = CreateToggle("SoundToggle", frame, new Vector2(0f, 70f), "ЗВУК", font);
        vibrationToggle = CreateToggle("VibrationToggle", frame, new Vector2(0f, -70f), "ВИБРАЦИЯ", font);
        closeButton = CreateButton(
            "CloseSettingsButton", Sprite("button_back"), Sprite("button_back_pressed"),
            frame, new Vector2(0f, -230f), new Vector2(160f, 118f));

        group = panel.AddComponent<CanvasGroup>();
        panel.SetActive(false);
    }

    private static Toggle CreateToggle(string name, RectTransform parent, Vector2 position, string label, TMP_FontAsset font)
    {
        var root = CreateRect(name, parent, position, new Vector2(620f, 110f));
        var labelText = CreateText("Label", root, label, new Vector2(-100f, 0f), new Vector2(360f, 70f), 38f, PaleCyan, font);
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        var background = CreateSolid("Background", root, new Color(0.08f, 0.42f, 0.5f, 1f), false);
        background.rectTransform.anchorMin = background.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        background.rectTransform.anchoredPosition = new Vector2(220f, 0f);
        background.rectTransform.sizeDelta = new Vector2(90f, 70f);

        var checkmark = CreateSolid("Checkmark", background.rectTransform, Yellow, false);
        checkmark.rectTransform.anchorMin = checkmark.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        checkmark.rectTransform.anchoredPosition = Vector2.zero;
        checkmark.rectTransform.sizeDelta = new Vector2(54f, 38f);

        var toggle = root.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.navigation = new Navigation { mode = Navigation.Mode.None };
        toggle.isOn = true;
        return toggle;
    }

    private static void ConfigureController(
        MissionMenuController controller,
        Image background,
        Image glass,
        CanvasGroup futureCityBackgroundGroup,
        CanvasGroup mapGroup,
        ScrollRect scrollRect,
        RectTransform viewport,
        RectTransform mapRoot,
        RectTransform selectionFrame,
        Sprite[] backgrounds,
        Image[] connectors,
        Button[] missionButtons,
        Image[] missionImages,
        TMP_Text[] missionLabels,
        RectTransform activeArrow,
        Image activeArrowImage,
        Sprite[] activeArrowFrames,
        Button backButton,
        Button storeButton,
        Button settingsButton,
        Button noAdsButton,
        GameObject preparePanel,
        CanvasGroup prepareGroup,
        TMP_Text prepareMission,
        TMP_Text prepareObjective,
        Button startButton,
        Button cancelPrepareButton,
        GameObject settingsPanel,
        CanvasGroup settingsGroup,
        Toggle soundToggle,
        Toggle vibrationToggle,
        Button closeSettingsButton,
        Image transitionFlash,
        Image lightningLine)
    {
        var serialized = new SerializedObject(controller);
        SetReference(serialized, "_background", background);
        SetReference(serialized, "_glass", glass);
        SetReference(serialized, "_futureCityBackgroundGroup", futureCityBackgroundGroup);
        SetReference(serialized, "_mapGroup", mapGroup);
        SetReference(serialized, "_scrollRect", scrollRect);
        SetReference(serialized, "_viewport", viewport);
        SetReference(serialized, "_mapRoot", mapRoot);
        SetReference(serialized, "_selectionFrame", selectionFrame);
        SetReferenceArray(serialized, "_backgroundSprites", backgrounds);
        SetReferenceArray(serialized, "_connectors", connectors);
        SetReferenceArray(serialized, "_missionButtons", missionButtons);
        SetReferenceArray(serialized, "_missionImages", missionImages);
        SetReferenceArray(serialized, "_missionLabels", missionLabels);
        SetReference(serialized, "_activeSprite", Sprite("mission_active"));
        SetReference(serialized, "_activePressedSprite", Sprite("mission_active_pressed"));
        SetReference(serialized, "_completedSprite", Sprite("mission_completed"));
        SetReference(serialized, "_completedPressedSprite", Sprite("mission_completed_pressed"));
        SetReference(serialized, "_lockedSprite", Sprite("mission_locked"));
        SetReference(serialized, "_activeArrow", activeArrow);
        SetReference(serialized, "_activeArrowImage", activeArrowImage);
        SetReferenceArray(serialized, "_activeArrowFrames", activeArrowFrames);
        SetReference(serialized, "_backButton", backButton);
        SetReference(serialized, "_storeButton", storeButton);
        SetReference(serialized, "_settingsButton", settingsButton);
        SetReference(serialized, "_noAdsButton", noAdsButton);
        SetReference(serialized, "_preparePanel", preparePanel);
        SetReference(serialized, "_prepareGroup", prepareGroup);
        SetReference(serialized, "_prepareMission", prepareMission);
        SetReference(serialized, "_prepareObjective", prepareObjective);
        SetReference(serialized, "_startButton", startButton);
        SetReference(serialized, "_cancelPrepareButton", cancelPrepareButton);
        SetReference(serialized, "_settingsPanel", settingsPanel);
        SetReference(serialized, "_settingsGroup", settingsGroup);
        SetReference(serialized, "_soundToggle", soundToggle);
        SetReference(serialized, "_vibrationToggle", vibrationToggle);
        SetReference(serialized, "_closeSettingsButton", closeSettingsButton);
        SetReference(serialized, "_transitionFlash", transitionFlash);
        SetReference(serialized, "_lightningLine", lightningLine);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform CreateSelectionFrame(RectTransform parent, Vector2 position)
    {
        var frame = CreateRect("ActiveSelectionFrame", parent, position, new Vector2(340f, 250f));
        SetTopAnchored(frame, position);

        CreateFrameLine("Top", frame, new Vector2(0f, 123f), new Vector2(340f, 6f));
        CreateFrameLine("Bottom", frame, new Vector2(0f, -123f), new Vector2(340f, 6f));
        CreateFrameLine("Left", frame, new Vector2(-167f, 0f), new Vector2(6f, 250f));
        CreateFrameLine("Right", frame, new Vector2(167f, 0f), new Vector2(6f, 250f));
        return frame;
    }

    private static void CreateFrameLine(string name, RectTransform parent, Vector2 position, Vector2 size)
    {
        var line = CreateSolid(name, parent, new Color(0.62f, 1f, 1f, 0.9f), false);
        line.rectTransform.anchorMin = line.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        line.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        line.rectTransform.anchoredPosition = position;
        line.rectTransform.sizeDelta = size;
    }

    private static void SetTopAnchored(RectTransform rect, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
    }

    private static Image CreateConnector(string name, Sprite sprite, RectTransform parent, Vector2 start, Vector2 end)
    {
        var difference = end - start;
        var image = CreateImage(name, sprite, parent, (start + end) * 0.5f, new Vector2(16f, difference.magnitude - 110f));
        SetTopAnchored(image.rectTransform, (start + end) * 0.5f);
        image.rectTransform.localEulerAngles = new Vector3(
            0f,
            0f,
            Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90f);
        image.raycastTarget = false;
        return image;
    }

    private static Button CreateButton(
        string name,
        Sprite sprite,
        Sprite pressedSprite,
        RectTransform parent,
        Vector2 position,
        Vector2 size,
        bool pulse = true)
    {
        var image = CreateImage(name, sprite, parent, position, size);
        image.raycastTarget = true;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.transition = pressedSprite == null ? Selectable.Transition.ColorTint : Selectable.Transition.SpriteSwap;
        if (pressedSprite != null)
        {
            var state = button.spriteState;
            state.pressedSprite = pressedSprite;
            state.selectedSprite = pressedSprite;
            button.spriteState = state;
        }

        if (pulse)
            image.gameObject.AddComponent<SciFiButtonPulse>();
        return button;
    }

    private static RectTransform CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 2304f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvasObject.transform as RectTransform;
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";
    }

    private static void CreateEventSystem()
    {
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Image CreateFullImage(string name, Sprite sprite, RectTransform parent, Color color)
    {
        var image = CreateImageObject(name, parent, sprite, color);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.offsetMin = Vector2.zero;
        image.rectTransform.offsetMax = Vector2.zero;
        image.preserveAspect = false;
        return image;
    }

    private static Image CreateBottomImage(string name, Sprite sprite, RectTransform parent, Vector2 size)
    {
        var image = CreateImageObject(name, parent, sprite, Color.white);
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        image.rectTransform.pivot = new Vector2(0.5f, 0f);
        image.rectTransform.anchoredPosition = Vector2.zero;
        image.rectTransform.sizeDelta = size;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateFullSolid(string name, RectTransform parent, Color color, bool raycastTarget)
    {
        var image = CreateImageObject(name, parent, null, color);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.offsetMin = Vector2.zero;
        image.rectTransform.offsetMax = Vector2.zero;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static Image CreateSolid(string name, RectTransform parent, Color color, bool raycastTarget)
    {
        var image = CreateImageObject(name, parent, null, color);
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static Image CreateImage(string name, Sprite sprite, RectTransform parent, Vector2 position, Vector2 size)
    {
        var image = CreateImageObject(name, parent, sprite, Color.white);
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = image.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        image.preserveAspect = true;
        return image;
    }

    private static Image CreateImageObject(string name, RectTransform parent, Sprite sprite, Color color)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = gameObject.transform as RectTransform;
        rect.SetParent(parent, false);

        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(
        string name,
        RectTransform parent,
        string value,
        Vector2 position,
        Vector2 size,
        float fontSize,
        Color color,
        TMP_FontAsset font)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var rect = gameObject.transform as RectTransform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        text.characterSpacing = 0f;
        return text;
    }

    private static RectTransform CreateFullRect(string name, RectTransform parent)
    {
        var rect = CreateRect(name, parent, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static RectTransform CreateRect(string name, RectTransform parent, Vector2 position, Vector2 size)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        var rect = gameObject.transform as RectTransform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static TMP_FontAsset LoadFont()
    {
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MainFontPath) ??
               AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackFontPath);
    }

    private static void ConfigureSprites()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesPath }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            var isArrowSheet = path.EndsWith("/active_arrow.png");
            importer.spriteImportMode = isArrowSheet ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();

            if (isArrowSheet)
                SliceArrowSprite(importer);
        }
    }

    private static void SliceArrowSprite(TextureImporter importer)
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null)
            return;

        provider.InitSpriteEditorDataProvider();
        var existing = provider.GetSpriteRects();
        if (existing != null && existing.Length == 4)
            return;

        var rects = new SpriteRect[4];
        for (var i = 0; i < rects.Length; i++)
        {
            rects[i] = new SpriteRect
            {
                name = $"active_arrow_{i:00}",
                rect = new Rect(i * 102f, 0f, 102f, 180f),
                alignment = SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                spriteID = GUID.Generate()
            };
        }

        provider.SetSpriteRects(rects);
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Sprite Sprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{name}.png");
        if (sprite == null)
            Debug.LogWarning($"Mission menu sprite not found: {name}");
        return sprite;
    }

    private static Sprite[] Sprites(string name)
    {
        return AssetDatabase.LoadAllAssetsAtPath($"{SpritesPath}/{name}.png")
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
    }

    private static void SetReferenceArray<T>(
        SerializedObject serializedObject,
        string propertyName,
        T[] values) where T : Object
    {
        var property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(item => item.path == scenePath))
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
