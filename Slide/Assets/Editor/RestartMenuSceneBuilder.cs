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
    private const string MissionSpritesPath = "Assets/Sprites/MissionMenu";
    private const string TmpFontPath = "Assets/Font/lofty-s SDF.asset";
    private const string ContinueValueFontPath = "Assets/Fonts/SDONE_0 SDF.asset";
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
        ConfigureSprites(ResultSpritesPath, ContinueSpritesPath);

        var controller = Resources.FindObjectsOfTypeAll<UIController>()
            .FirstOrDefault(item => item.gameObject.scene == scene);
        if (controller == null)
            throw new MissingReferenceException("UIController was not found in Game scene.");

        var canvas = controller.GetComponentInParent<Canvas>();
        var continuePanel = FindDescendant(canvas.transform, "ContinuePanel");
        var deathPanel = FindDescendant(canvas.transform, "DeathPanel");
        if (continuePanel == null || deathPanel == null)
            throw new MissingReferenceException("ContinuePanel or DeathPanel was not found in Game scene.");

        DestroyChild(continuePanel, "RestartContinueV2");
        DestroyChild(continuePanel, "ChallengeContinueOffer");
        DestroyChild(deathPanel, "RestartResultV2");
        DestroyChild(continuePanel, "Back");
        DestroyChild(deathPanel, "Back");
        DestroyDescendantsNamed(deathPanel, "HightscoreTable");
        DisablePanelBlocker(continuePanel);
        DisablePanelBlocker(deathPanel);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        var valueFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ContinueValueFontPath);
        var continueView = BuildContinueView((RectTransform)continuePanel, font, valueFont);
        var resultView = BuildResultView((RectTransform)deathPanel, font);

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

        Debug.Log("Challenge continue and final result menus rebuilt in Game scene.");
    }

    [MenuItem("Slide/Rebuild Continue Offer Only")]
    public static void BuildContinueOfferOnly()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var previousScenePath = SceneManager.GetActiveScene().path;
        var scene = previousScenePath == ScenePath
            ? SceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AssetDatabase.Refresh();
        ConfigureSprites(ContinueSpritesPath);

        var controller = Resources.FindObjectsOfTypeAll<UIController>()
            .FirstOrDefault(item => item.gameObject.scene == scene);
        if (controller == null)
            throw new MissingReferenceException("UIController was not found in Game scene.");

        var canvas = controller.GetComponentInParent<Canvas>();
        var continuePanel = FindDescendant(canvas.transform, "ContinuePanel");
        if (continuePanel == null)
            throw new MissingReferenceException("ContinuePanel was not found in Game scene.");

        DestroyChild(continuePanel, "RestartContinueV2");
        DestroyChild(continuePanel, "ChallengeContinueOffer");
        DestroyChild(continuePanel, "Back");
        DisablePanelBlocker(continuePanel);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        var valueFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ContinueValueFontPath);
        var continueView = BuildContinueView((RectTransform)continuePanel, font, valueFont);

        var serializedController = new SerializedObject(controller);
        SetReference(serializedController, "_continueOfferView", continueView);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath)
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log("Continue offer rebuilt in Game scene.");
    }

    [MenuItem("Slide/Rebuild Final Result Only")]
    public static void BuildFinalResultOnly()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var previousScenePath = SceneManager.GetActiveScene().path;
        var scene = previousScenePath == ScenePath
            ? SceneManager.GetActiveScene()
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        AssetDatabase.Refresh();
        ConfigureSprites(ResultSpritesPath);

        var controller = Resources.FindObjectsOfTypeAll<UIController>()
            .FirstOrDefault(item => item.gameObject.scene == scene);
        if (controller == null)
            throw new MissingReferenceException("UIController was not found in Game scene.");

        var canvas = controller.GetComponentInParent<Canvas>();
        var deathPanel = FindDescendant(canvas.transform, "DeathPanel");
        if (deathPanel == null)
            throw new MissingReferenceException("DeathPanel was not found in Game scene.");

        DestroyChild(deathPanel, "RestartResultV2");
        DestroyChild(deathPanel, "Back");
        DestroyDescendantsNamed(deathPanel, "HightscoreTable");
        DisablePanelBlocker(deathPanel);

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
        var resultView = BuildResultView((RectTransform)deathPanel, font);

        var serializedController = new SerializedObject(controller);
        SetReference(serializedController, "_resultMenuView", resultView);
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath)
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log("Final result menu rebuilt in Game scene.");
    }

    private static ContinueOfferView BuildContinueView(
        RectTransform panel,
        TMP_FontAsset font,
        TMP_FontAsset valueFont)
    {
        var root = CreateFullRect("ChallengeContinueOffer", panel);
        var group = root.gameObject.AddComponent<CanvasGroup>();
        var view = root.gameObject.AddComponent<ContinueOfferView>();

        CreateSolid("Backdrop", root, new Color(0.01f, 0.045f, 0.065f, 0.86f), true);
        var safeRoot = CreateFullRect("SafeArea", root);
        safeRoot.gameObject.AddComponent<SafeAreaLayout>();

        var artboard = CreateFullRect("Artboard", safeRoot);
        var fitter = artboard.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1280f / 2304f;

        CreateFullLayer("Plate", ContinueSprite("plate"), artboard);
        CreateTextureRegion(
            "Balance",
            ContinueSprite("balance"),
            artboard,
            new Rect(0f, 0f, 765f, 2304f),
            new Rect(0f, 0f, 765f, 2304f));
        CreateTextureRegion(
            "Collected",
            ContinueSprite("collected"),
            artboard,
            new Rect(0f, 0f, 820f, 2304f),
            new Rect(0f, 0f, 820f, 2304f));
        CreateTextureRegion(
            "Passed",
            ContinueSprite("passed"),
            artboard,
            new Rect(0f, 0f, 720f, 2304f),
            new Rect(0f, 0f, 720f, 2304f));
        CreateFullLayer("RewardedArt", ContinueSprite("rewarded"), artboard);
        CreateTextureRegion(
            "CoinsArtLeft",
            ContinueSprite("coins"),
            artboard,
            new Rect(0f, 0f, 836f, 2304f),
            new Rect(0f, 0f, 836f, 2304f));
        CreateTextureRegion(
            "CoinsArtValueBackground",
            ContinueSprite("coins"),
            artboard,
            new Rect(925f, 0f, 55f, 2304f),
            new Rect(836f, 0f, 144f, 2304f));
        CreateTextureRegion(
            "CoinsArtRight",
            ContinueSprite("coins"),
            artboard,
            new Rect(980f, 0f, 300f, 2304f),
            new Rect(980f, 0f, 300f, 2304f));

        var timer = CreateFullLayer("Timer", ContinueSprite("timer"), artboard);
        timer.type = Image.Type.Filled;
        timer.fillMethod = Image.FillMethod.Horizontal;
        timer.fillOrigin = (int)Image.OriginHorizontal.Left;
        timer.fillAmount = 1f;

        var balance = CreateReferenceValueText(
            "BalanceValue", artboard, "0", 765f, 96f, 210f, 110f, 160f, Yellow, valueFont);
        var collected = CreateReferenceValueText(
            "CollectedValue", artboard, "0", 808f, 510f, 180f, 100f, 150f, Yellow, valueFont);
        var passed = CreateReferenceValueText(
            "PassedValue", artboard, "0M", 710f, 680f, 220f, 105f, 144f, Navy, valueFont);

        var rewarded = CreateInspectorHitButton("RewardedContinue", artboard, 116f, 1324f, 624f, 376f);
        var coins = CreateHitButton("CoinsContinue", artboard, 528f, 1324f, 624f, 376f);
        var price = CreateReferenceValueText(
            "Price", artboard, "100", 825f, 1418f, 165f, 145f, 160f, Yellow, valueFont);

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
        SetColor(serializedView, "_passedValueColor", Navy);
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

    private static ResultMenuView BuildResultView(RectTransform panel, TMP_FontAsset font)
    {
        var root = CreateFullRect("RestartResultV2", panel);
        var view = root.gameObject.AddComponent<ResultMenuView>();
        CreateSolid("Backdrop", root, new Color(0.01f, 0.045f, 0.065f, 0.94f), true);

        var safeRoot = CreateFullRect("SafeArea", root);
        safeRoot.gameObject.AddComponent<SafeAreaLayout>();
        var artboard = CreateFullRect("Artboard", safeRoot);
        var fitter = artboard.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1280f / 2304f;

        CreateFullLayer("Plate", Sprite("result_plate"), artboard);

        var balance = CreateReferenceText(
            "Balance", artboard, "БАЛАНС: 0", 640f, 155f, 760f, 100f, 56f, Yellow, font);
        var score = CreatePixelNumber("Score", artboard, 640f, 485f);
        var record = CreateReferenceText(
            "Record", artboard, "РЕКОРД: 0М", 640f, 730f, 520f, 82f, 46f, Color.white, font);
        var mission = CreateReferenceText(
            "Mission", artboard, "МИССИЯ 1 — ПРОВАЛЕНА", 640f, 630f, 780f, 72f, 34f, Cyan, font);
        var details = CreateReferenceText(
            "Details",
            artboard,
            "ПРОЙДЕНО: 0М    СОБРАНО: 0\nУРОВНЕЙ ЗА СЕССИЮ: 0",
            640f,
            870f,
            950f,
            130f,
            31f,
            Navy,
            font);

        var offerRoot = CreateFullRect("StoreOffer", artboard);

        var noAdsRoot = CreateFullRect("NoAdsOffer", artboard);
        CreateFullLayer("NoAdsArt", Sprite("result_no_ads"), noAdsRoot);
        var noAdsButton = CreateHitButton("NoAdsButton", noAdsRoot, 236f, 1190f, 860f, 400f);
        var noAdsPrice = CreateReferenceText(
            "NoAdsPrice", noAdsRoot, string.Empty, 955f, 1460f, 250f, 58f, 28f, Navy, font);

        var primary = CreateButton(
            "Primary",
            Sprite("result_restart"),
            Sprite("result_restart_pressed"),
            artboard,
            ReferencePosition(640f, 1780f),
            new Vector2(460f, 168f) * ArtScale);
        var primaryText = CreateText(
            "PrimaryText",
            primary.transform as RectTransform,
            "СЛЕДУЮЩИЙ",
            Vector2.zero,
            new Vector2(420f, 120f) * ArtScale,
            34f * ArtScale,
            Navy,
            font);

        var characters = CreateButton(
            "Characters",
            Sprite("result_characters"),
            Sprite("result_characters_pressed"),
            artboard,
            ReferencePosition(360f, 2025f),
            new Vector2(224f, 168f) * ArtScale);
        CreateText(
            "CharactersLabel",
            characters.transform as RectTransform,
            "ПЕРСОНАЖИ",
            Vector2.zero,
            new Vector2(210f, 125f) * ArtScale,
            21f * ArtScale,
            Navy,
            font);

        var missions = CreateButton(
            "Missions",
            LoadSprite($"{MissionSpritesPath}/mission_active.png"),
            LoadSprite($"{MissionSpritesPath}/mission_active_pressed.png"),
            artboard,
            ReferencePosition(920f, 2025f),
            new Vector2(224f, 168f) * ArtScale);
        CreateText(
            "MissionsLabel",
            missions.transform as RectTransform,
            "МИССИИ",
            Vector2.zero,
            new Vector2(210f, 125f) * ArtScale,
            24f * ArtScale,
            Navy,
            font);

        var serializedView = new SerializedObject(view);
        SetReference(serializedView, "_balanceText", balance);
        SetReference(serializedView, "_recordText", record);
        SetReference(serializedView, "_detailsText", details);
        SetReference(serializedView, "_missionText", mission);
        SetReference(serializedView, "_scoreView", score);
        SetReference(serializedView, "_primaryImage", primary.image);
        SetReference(serializedView, "_primaryButtonText", primaryText);
        SetReference(serializedView, "_primaryButton", primary);
        SetReference(serializedView, "_charactersButton", characters);
        SetReference(serializedView, "_missionsButton", missions);
        SetReference(serializedView, "_restartNormal", Sprite("result_restart"));
        SetReference(serializedView, "_restartPressed", Sprite("result_restart_pressed"));
        SetReference(serializedView, "_continueNormal", LoadSprite($"{ResultSpritesPath}/button_wide.png"));
        SetReference(serializedView, "_continuePressed", LoadSprite($"{ResultSpritesPath}/button_wide_low.png"));
        SetReference(serializedView, "_storeOfferRoot", offerRoot.gameObject);
        SetReference(serializedView, "_noAdsRoot", noAdsRoot.gameObject);
        SetReference(serializedView, "_noAdsPriceText", noAdsPrice);
        SetReference(serializedView, "_noAdsButton", noAdsButton);
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        return view;
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

    private static PixelNumberView CreatePixelNumber(
        string name,
        RectTransform parent,
        float x,
        float y)
    {
        var root = CreateRect(name, parent, Vector2.zero, new Vector2(620f, 184f) * ArtScale);
        SetCanvasRect(root, x, y, 620f, 184f);

        var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 2f * ArtScale;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var view = root.gameObject.AddComponent<PixelNumberView>();
        var slots = new Image[11];
        for (var index = 0; index < slots.Length; index++)
        {
            slots[index] = CreateImage(
                $"Slot{index + 1}",
                Sprite("result_0"),
                root,
                Vector2.zero,
                new Vector2(140f, 184f) * ArtScale);
        }

        var serializedView = new SerializedObject(view);
        var digits = serializedView.FindProperty("_digits");
        digits.arraySize = 10;
        for (var index = 0; index < digits.arraySize; index++)
            digits.GetArrayElementAtIndex(index).objectReferenceValue = Sprite($"result_{index}");
        SetReference(serializedView, "_meterSuffix", Sprite("result_meter"));

        var slotProperty = serializedView.FindProperty("_slots");
        slotProperty.arraySize = slots.Length;
        for (var index = 0; index < slots.Length; index++)
            slotProperty.GetArrayElementAtIndex(index).objectReferenceValue = slots[index];
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        return view;
    }

    private static Vector2 ReferencePosition(float x, float y)
    {
        return new Vector2((x - 640f) * ArtScale, (1152f - y) * ArtScale);
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

    private static RawImage CreateTextureRegion(
        string name,
        Sprite sprite,
        RectTransform parent,
        Rect sourcePixels,
        Rect targetPixels)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        var rect = (RectTransform)gameObject.transform;
        rect.SetParent(parent, false);
        SetCanvasRect(
            rect,
            targetPixels.center.x,
            targetPixels.center.y,
            targetPixels.width,
            targetPixels.height);

        var image = gameObject.GetComponent<RawImage>();
        image.texture = sprite.texture;
        image.uvRect = new Rect(
            sourcePixels.xMin / sprite.texture.width,
            1f - sourcePixels.yMax / sprite.texture.height,
            sourcePixels.width / sprite.texture.width,
            sourcePixels.height / sprite.texture.height);
        image.raycastTarget = false;
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

    private static Button CreateInspectorHitButton(
        string name,
        RectTransform parent,
        float x,
        float y,
        float width,
        float height)
    {
        var image = CreateSolid(name, parent, Color.clear, false);
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = ReferencePosition(x + width * 0.5f, y + height * 0.5f);
        rect.sizeDelta = new Vector2(width, height) * ArtScale;
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

    private static TMP_Text CreateReferenceValueText(
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
        SetCanvasRect(
            text.rectTransform,
            x + width * 0.5f,
            y + height * 0.5f,
            width,
            height);
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(8f, fontSize * 0.45f * ArtScale);
        text.fontSizeMax = fontSize * ArtScale;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
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

    private static void ConfigureSprites(params string[] searchPaths)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", searchPaths))
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
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
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

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"UI sprite not found: {path}");
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

    private static void DestroyDescendantsNamed(Transform parent, string name)
    {
        for (var index = parent.childCount - 1; index >= 0; index--)
        {
            var child = parent.GetChild(index);
            if (child.name == name)
            {
                Object.DestroyImmediate(child.gameObject);
                continue;
            }

            DestroyDescendantsNamed(child, name);
        }
    }

    private static void DisablePanelBlocker(Transform panel)
    {
        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = Color.clear;
            image.raycastTarget = false;
        }

        var button = panel.GetComponent<Button>();
        if (button != null)
            button.enabled = false;
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new MissingFieldException(serializedObject.targetObject.GetType().Name, propertyName);
        property.objectReferenceValue = value;
    }

    private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
            throw new MissingFieldException(serializedObject.targetObject.GetType().Name, propertyName);
        property.colorValue = value;
    }

    private static readonly Color Cyan = new Color32(82, 236, 244, 255);
    private static readonly Color Yellow = new Color32(255, 204, 54, 255);
    private static readonly Color Navy = new Color32(7, 48, 65, 255);
}
#endif
