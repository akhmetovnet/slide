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
    private const string ResultSpritesPath = "Assets/Sprites/RestartMenu";
    private const string ContinueSpritesPath = "Assets/Sprites/ContinueOffer";
    private const string TmpFontPath = "Assets/Font/lofty-s SDF.asset";
    private const float ArtScale = 0.25f;

    [MenuItem("Slide/Build Challenge Continue Offer")]
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

        var oldContinue = FindDirectChild(continuePanel, "Back");
        var oldResult = FindDirectChild(deathPanel, "Back");
        var generatedResult = FindDirectChild(deathPanel, "RestartResultV2");
        var leaderboard = generatedResult != null
            ? FindDescendant(generatedResult, "HightscoreTable") as RectTransform
            : null;
        if (leaderboard != null && oldResult != null)
            leaderboard.SetParent(oldResult, false);

        DestroyChild(continuePanel, "RestartContinueV2");
        DestroyChild(continuePanel, "ChallengeContinueOffer");
        DestroyChild(deathPanel, "RestartResultV2");
        if (oldContinue != null)
            oldContinue.gameObject.SetActive(false);
        if (oldResult != null)
            oldResult.gameObject.SetActive(true);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        var continueView = BuildContinueView((RectTransform)continuePanel, font);

        var serializedController = new SerializedObject(controller);
        SetReference(serializedController, "_continueOfferView", continueView);
        SetReference(serializedController, "_resultMenuView", null);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath)
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log("Challenge continue offer rebuilt and original result window restored in Game scene.");
    }

    private static ContinueOfferView BuildContinueView(RectTransform panel, TMP_FontAsset font)
    {
        var root = CreateFullRect("ChallengeContinueOffer", panel);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        var view = root.gameObject.AddComponent<ContinueOfferView>();

        var safeRoot = CreateFullRect("SafeArea", root);
        safeRoot.gameObject.AddComponent<SafeAreaLayout>();
        CreateSolid("Backdrop", root, new Color(0.01f, 0.045f, 0.065f, 0.86f), true);

        var artboard = CreateFullRect("Artboard", safeRoot);
        var fitter = artboard.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1280f / 2304f;

        CreateFullLayer("Plate", ContinueSprite("plate"), artboard);
        CreateFullLayer("Balance", ContinueSprite("balance"), artboard);
        CreateFullLayer("Collected", ContinueSprite("collected"), artboard);
        CreateFullLayer("Passed", ContinueSprite("passed"), artboard);
        CreateFullLayer("RewardedArt", ContinueSprite("rewarded"), artboard);
        CreateFullLayer("CoinsArt", ContinueSprite("coins"), artboard);

        var timer = CreateFullLayer("Timer", ContinueSprite("timer"), artboard);
        timer.type = Image.Type.Filled;
        timer.fillMethod = Image.FillMethod.Horizontal;
        timer.fillOrigin = (int)Image.OriginHorizontal.Left;
        timer.fillAmount = 1f;

        var balanceMask = CreateSolid("BalanceValueMask", artboard, new Color32(202, 119, 25, 255), false);
        SetCanvasRect(balanceMask.rectTransform, 812.5f, 150f, 125f, 80f);
        var balance = CreateReferenceText("BalanceValue", artboard, "0", 814f, 151f, 140f, 82f, 56f, Yellow, font);

        var collectedMask = CreateSolid("CollectedValueMask", artboard, new Color32(196, 113, 24, 255), false);
        SetCanvasRect(collectedMask.rectTransform, 809f, 566f, 126f, 68f);
        var collected = CreateReferenceText("CollectedValue", artboard, "0", 808f, 567f, 140f, 72f, 46f, Yellow, font);

        var passedMask = CreateSolid("PassedValueMask", artboard, new Color32(197, 201, 202, 255), false);
        SetCanvasRect(passedMask.rectTransform, 765f, 718f, 170f, 60f);
        var passed = CreateReferenceText("PassedValue", artboard, "0 M", 765f, 719f, 190f, 66f, 40f, Navy, font);

        var rewarded = CreateHitButton("RewardedContinue", artboard, 116f, 1324f, 624f, 376f);
        var coins = CreateHitButton("CoinsContinue", artboard, 528f, 1324f, 624f, 376f);
        var priceMask = CreateSolid("PriceValueMask", artboard, new Color32(43, 225, 228, 255), false);
        SetCanvasRect(priceMask.rectTransform, 845f, 1510f, 130f, 72f);
        var price = CreateReferenceText("Price", artboard, "100", 846f, 1510f, 146f, 78f, 44f, Yellow, font);

        var skipNormal = CreateFullLayer("SkipNormal", ContinueSprite("skip_normal"), artboard);
        var skipPressed = CreateFullLayer("SkipPressed", ContinueSprite("skip_pressed"), artboard);
        var skip = CreateHitButton("Skip", artboard, 437f, 1656f, 400f, 143f);
        var status = CreateReferenceText("Status", artboard, string.Empty, 640f, 1450f, 550f, 58f, 30f, Yellow, font);

        var serializedView = new SerializedObject(view);
        SetReference(serializedView, "_canvasGroup", group);
        SetReference(serializedView, "_timerFill", timer);
        SetReference(serializedView, "_collectedText", collected);
        SetReference(serializedView, "_passedText", passed);
        SetReference(serializedView, "_balanceText", balance);
        SetReference(serializedView, "_priceText", price);
        SetReference(serializedView, "_statusText", status);
        SetReference(serializedView, "_rewardedButton", rewarded);
        SetReference(serializedView, "_coinsButton", coins);
        SetReference(serializedView, "_skipButton", skip);
        SetReference(serializedView, "_skipNormal", skipNormal.gameObject);
        SetReference(serializedView, "_skipPressed", skipPressed.gameObject);
        SetReference(serializedView, "_skipNormalTransform", skipNormal.rectTransform);
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

    private static Image CreateFullLayer(string name, Sprite sprite, RectTransform parent)
    {
        var image = CreateImage(name, sprite, parent, Vector2.zero, Vector2.zero);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.offsetMin = Vector2.zero;
        image.rectTransform.offsetMax = Vector2.zero;
        image.preserveAspect = true;
        return image;
    }

    private static Button CreateHitButton(string name, RectTransform parent, float x, float y, float width, float height)
    {
        var image = CreateSolid(name, parent, Color.clear, false);
        SetCanvasRect(image.rectTransform, x + width * 0.5f, y + height * 0.5f, width, height);
        image.raycastTarget = true;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        return button;
    }

    private static void SetCanvasRect(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(
            (x - width * 0.5f) / 1280f,
            1f - (y + height * 0.5f) / 2304f);
        rect.anchorMax = new Vector2(
            (x + width * 0.5f) / 1280f,
            1f - (y - height * 0.5f) / 2304f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static TMP_Text CreateReferenceText(
        string name,
        RectTransform parent,
        string value,
        float x,
        float y,
        float width,
        float height,
        float fontSize,
        Color color,
        TMP_FontAsset font)
    {
        var text = CreateText(
            name,
            parent,
            value,
            Vector2.zero,
            new Vector2(width, height) * ArtScale,
            fontSize * ArtScale,
            color,
            font);
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(x / 1280f, 1f - y / 2304f);
        rect.anchoredPosition = Vector2.zero;
        return text;
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
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ResultSpritesPath, ContinueSpritesPath }))
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
            importer.maxTextureSize = 4096;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }
    }

    private static Sprite Sprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ResultSpritesPath}/{name}.png");
        if (sprite == null)
            Debug.LogWarning($"Restart menu sprite not found: {name}");
        return sprite;
    }

    private static Sprite ContinueSprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ContinueSpritesPath}/{name}.png");
        if (sprite == null)
            Debug.LogWarning($"Continue offer sprite not found: {name}");
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
