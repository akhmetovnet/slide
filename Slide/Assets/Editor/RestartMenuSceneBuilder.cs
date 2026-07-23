#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class RestartMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Game.unity";
    private const string SpritesPath = "Assets/Sprites/RestartMenu";
    private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const float ArtScale = 0.25f;

    [MenuItem("Slide/Build Restart Menus")]
    public static void BuildRestartMenus()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var previousScenePath = SceneManager.GetActiveScene().path;
        var scene = previousScenePath == ScenePath
            ? SceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AssetDatabase.Refresh();
        ConfigureSprites();

        var controller = Resources.FindObjectsOfTypeAll<UIController>()
            .FirstOrDefault(item => item.gameObject.scene == scene);
        if (controller == null)
            throw new MissingReferenceException("UIController was not found in Game scene.");

        var canvas = controller.GetComponentInParent<Canvas>();
        var continuePanel = FindDescendant(canvas.transform, "ContinuePanel");
        var deathPanel = FindDescendant(canvas.transform, "DeathPanel");
        if (continuePanel == null || deathPanel == null)
            throw new MissingReferenceException("ContinuePanel or DeathPanel was not found in Game scene.");

        var leaderboard = FindDescendant(deathPanel, "HightscoreTable") as RectTransform;
        if (leaderboard != null)
            leaderboard.SetParent(deathPanel, false);

        DestroyChild(continuePanel, "RestartContinueV2");
        DestroyChild(deathPanel, "RestartResultV2");

        var oldContinue = FindDirectChild(continuePanel, "Back");
        var oldResult = FindDirectChild(deathPanel, "Back");
        if (oldContinue != null)
            oldContinue.gameObject.SetActive(false);
        if (oldResult != null)
            oldResult.gameObject.SetActive(false);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        var continueView = BuildContinueView((RectTransform)continuePanel, font);
        var resultView = BuildResultView((RectTransform)deathPanel, leaderboard, font);

        var serializedController = new SerializedObject(controller);
        SetReference(serializedController, "_continueOfferView", continueView);
        SetReference(serializedController, "_resultMenuView", resultView);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath)
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log("Restart and continue menus rebuilt in Game scene.");
    }

    private static ContinueOfferView BuildContinueView(RectTransform panel, TMP_FontAsset font)
    {
        var root = CreateFullRect("RestartContinueV2", panel);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        var view = root.gameObject.AddComponent<ContinueOfferView>();

        CreateSolid("Backdrop", root, new Color(0.01f, 0.045f, 0.065f, 0.86f), true);
        CreateImage("Plate", Sprite("continue_plate"), root, new Vector2(0f, -7f));

        var score = CreateText("Score", root, "СЧЕТ 0", new Vector2(0f, 13f), new Vector2(190f, 18f), 13f, Color.white, font);
        var level = CreateText("Level", root, "УРОВЕНЬ 1", new Vector2(0f, -7f), new Vector2(190f, 16f), 10f, Cyan, font);

        var timer = CreateImage("Timer", Sprite("continue_timer"), root, new Vector2(-2f, -36f));
        timer.type = Image.Type.Filled;
        timer.fillMethod = Image.FillMethod.Horizontal;
        timer.fillOrigin = (int)Image.OriginHorizontal.Left;
        timer.fillAmount = 1f;
        var timerText = CreateText("TimerValue", root, "5", new Vector2(-2f, -35f), new Vector2(40f, 22f), 16f, Navy, font);

        var rewarded = CreateButton("RewardedContinue", Sprite("continue_ad"), null, root, new Vector2(-53f, -90f));
        CreateText("RewardedLabel", rewarded.transform as RectTransform, "РЕКЛАМА", new Vector2(0f, -30f), new Vector2(100f, 14f), 8f, Navy, font);

        var coins = CreateButton("CoinsContinue", Sprite("continue_coins"), null, root, new Vector2(50f, -90f));
        var priceMask = CreateRect("PriceMask", coins.transform as RectTransform, new Vector2(8f, -24f), new Vector2(48f, 18f));
        var priceMaskImage = priceMask.gameObject.AddComponent<Image>();
        priceMaskImage.color = new Color32(37, 196, 211, 255);
        priceMaskImage.raycastTarget = false;
        var price = CreateText("Price", priceMask, "100", Vector2.zero, priceMask.sizeDelta, 11f, Navy, font);

        var skip = CreateButton(
            "Skip",
            Sprite("continue_skip"),
            Sprite("continue_skip_pressed"),
            root,
            new Vector2(0f, -144f));

        var balance = CreateText("Balance", root, "0", new Vector2(118f, 248f), new Vector2(70f, 22f), 12f, Yellow, font);
        CreateText("BalanceLabel", root, "МОНЕТЫ", new Vector2(118f, 264f), new Vector2(70f, 12f), 7f, Cyan, font);
        var status = CreateText("Status", root, string.Empty, new Vector2(0f, -55f), new Vector2(210f, 14f), 8f, Yellow, font);

        var serializedView = new SerializedObject(view);
        SetReference(serializedView, "_canvasGroup", group);
        SetReference(serializedView, "_timerFill", timer);
        SetReference(serializedView, "_timerText", timerText);
        SetReference(serializedView, "_scoreText", score);
        SetReference(serializedView, "_levelText", level);
        SetReference(serializedView, "_balanceText", balance);
        SetReference(serializedView, "_priceText", price);
        SetReference(serializedView, "_statusText", status);
        SetReference(serializedView, "_rewardedButton", rewarded);
        SetReference(serializedView, "_coinsButton", coins);
        SetReference(serializedView, "_skipButton", skip);
        SetReference(serializedView, "_skipTransform", skip.transform);
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    private static ResultMenuView BuildResultView(RectTransform panel, RectTransform leaderboard, TMP_FontAsset font)
    {
        var root = CreateFullRect("RestartResultV2", panel);
        var view = root.gameObject.AddComponent<ResultMenuView>();

        CreateSolid("Backdrop", root, new Color(0.01f, 0.045f, 0.065f, 0.9f), true);
        CreateImage("Plate", Sprite("result_plate"), root, new Vector2(-13f, 18f));
        CreateImage("RecordBadge", Sprite("result_record"), root, new Vector2(0f, 99f));

        var title = CreateText("Title", root, "СПУСК ПРЕРВАН", new Vector2(0f, 126f), new Vector2(280f, 28f), 17f, Yellow, font);
        var stats = CreateText(
            "Stats",
            root,
            "СЧЕТ  0\nРЕКОРД  0\nУРОВНЕЙ ЗА СЕССИЮ  0",
            new Vector2(0f, 35f),
            new Vector2(220f, 66f),
            12f,
            Color.white,
            font);
        stats.lineSpacing = 7f;
        var mission = CreateText("Mission", root, "МИССИЯ 1 - ПРОВАЛЕНА", new Vector2(0f, -43f), new Vector2(250f, 20f), 10f, Cyan, font);

        if (leaderboard != null)
        {
            leaderboard.SetParent(root, false);
            leaderboard.anchorMin = leaderboard.anchorMax = new Vector2(0.5f, 0.5f);
            leaderboard.pivot = new Vector2(0.5f, 0.5f);
            leaderboard.anchoredPosition = new Vector2(0f, 20f);
            leaderboard.sizeDelta = new Vector2(190f, 126f);
            leaderboard.gameObject.SetActive(true);
        }

        var primary = CreateButton("Primary", Sprite("result_restart"), Sprite("result_restart_pressed"), root, new Vector2(0f, -94f), new Vector2(116f, 43f));
        var primaryMask = CreateRect("PrimaryMask", primary.transform as RectTransform, Vector2.zero, new Vector2(72f, 30f));
        var primaryMaskImage = primaryMask.gameObject.AddComponent<Image>();
        primaryMaskImage.color = new Color32(50, 218, 224, 255);
        primaryMaskImage.raycastTarget = false;
        var primaryText = CreateText("PrimaryText", primaryMask, "ПОВТОРИТЬ", Vector2.zero, primaryMask.sizeDelta, 10f, Navy, font);

        var characters = CreateSmallButton("Characters", root, new Vector2(-96f, -148f), "ПЕРСОНАЖИ", font);
        var missions = CreateSmallButton("Missions", root, new Vector2(0f, -148f), "МИССИИ", font);
        var main = CreateSmallButton("MainMenu", root, new Vector2(96f, -148f), "МЕНЮ", font);

        var serializedView = new SerializedObject(view);
        SetReference(serializedView, "_titleText", title);
        SetReference(serializedView, "_statsText", stats);
        SetReference(serializedView, "_missionText", mission);
        SetReference(serializedView, "_primaryButtonText", primaryText);
        SetReference(serializedView, "_primaryButton", primary);
        SetReference(serializedView, "_charactersButton", characters);
        SetReference(serializedView, "_missionsButton", missions);
        SetReference(serializedView, "_mainMenuButton", main);
        SetReference(serializedView, "_leaderboardRoot", leaderboard != null ? leaderboard.gameObject : null);
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    private static Button CreateSmallButton(string name, RectTransform parent, Vector2 position, string label, TMP_FontAsset font)
    {
        var button = CreateButton(
            name,
            Sprite("result_characters"),
            Sprite("result_characters_pressed"),
            parent,
            position,
            new Vector2(88f, 38f));
        CreateText($"{name}Label", button.transform as RectTransform, label, Vector2.zero, new Vector2(82f, 30f), 9f, Navy, font);
        return button;
    }

    private static Button CreateButton(
        string name,
        Sprite sprite,
        Sprite pressedSprite,
        RectTransform parent,
        Vector2 position,
        Vector2? size = null)
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
        else
        {
            var colors = button.colors;
            colors.disabledColor = new Color(0.28f, 0.35f, 0.38f, 0.62f);
            colors.pressedColor = new Color(0.68f, 0.94f, 1f, 1f);
            button.colors = colors;
        }

        image.gameObject.AddComponent<SciFiButtonPulse>();
        return button;
    }

    private static Image CreateImage(string name, Sprite sprite, RectTransform parent, Vector2 position, Vector2? size = null)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size ?? SpriteSize(sprite);

        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateSolid(string name, RectTransform parent, Color color, bool stretch)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        var image = gameObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
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
        var rect = (RectTransform)gameObject.transform;
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
        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
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
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }
    }

    private static Sprite Sprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/{name}.png");
        if (sprite == null)
            Debug.LogWarning($"Restart menu sprite not found: {name}");
        return sprite;
    }

    private static Vector2 SpriteSize(Sprite sprite)
    {
        if (sprite == null)
            return new Vector2(80f, 40f);
        return new Vector2(sprite.rect.width * ArtScale, sprite.rect.height * ArtScale);
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            var nested = FindDescendant(child, name);
            if (nested != null)
                return nested;
        }
        return null;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        return parent.Cast<Transform>().FirstOrDefault(child => child.name == name);
    }

    private static void DestroyChild(Transform parent, string name)
    {
        var child = FindDirectChild(parent, name);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new MissingFieldException(serializedObject.targetObject.GetType().Name, propertyName);
        property.objectReferenceValue = value;
    }

    private static readonly Color Cyan = new Color32(82, 236, 244, 255);
    private static readonly Color Yellow = new Color32(255, 204, 54, 255);
    private static readonly Color Navy = new Color32(7, 48, 65, 255);
}
#endif
