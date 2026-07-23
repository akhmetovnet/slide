#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Installers;
using TMPro;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CharacterMenuSceneBuilder
{
    private const string MenuSpritesPath = "Assets/Sprites/CharacterMenu";
    private const string PlayerSpritesPath = "Assets/Sprites/NewSprites/player";
    private const string ScenePath = "Assets/Scenes/CharacterMenu.unity";
    private const string SettingsPath = "Assets/Configs/SoInstaller.asset";
    private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Slide/Build Character Menu Scene")]
    public static void BuildCharacterMenuScene()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var previousScenePath = SceneManager.GetActiveScene().path;

        AssetDatabase.Refresh();
        ConfigureSprites(MenuSpritesPath);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateEventSystem();

        var canvasRoot = CreateCanvas();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);

        CreateFullImage("Background", Sprite("background"), canvasRoot, Color.white);
        var fan = CreateImage("Fan", Sprite("fan"), canvasRoot, new Vector2(2f, 242f));
        CreateFullImage("Grid", Sprite("background_grid"), canvasRoot, Color.white);
        var ringLight = CreateFullImage("RingLight", Sprite("background_light"), canvasRoot, new Color(1f, 1f, 1f, 0.72f));

        var selectionRoot = CreateRect("SelectionRoot", canvasRoot, Vector2.zero, new Vector2(1280f, 2304f));
        var selectionGroup = selectionRoot.gameObject.AddComponent<CanvasGroup>();

        var podiumGlow3 = CreateImage("PodiumGlow3", Sprite("podium_glow_3"), selectionRoot, new Vector2(-10f, -116f));
        var podiumGlow2 = CreateImage("PodiumGlow2", Sprite("podium_glow_2"), selectionRoot, new Vector2(-12f, -256f));
        var podiumGlow1 = CreateImage("PodiumGlow1", Sprite("podium_glow_1"), selectionRoot, new Vector2(-118f, -278f));
        CreateImage("Podium", Sprite("podium"), selectionRoot, new Vector2(-2f, -703f));

        var robotRoot = CreateRect("RobotRoot", selectionRoot, Vector2.zero, new Vector2(1280f, 2304f));
        var leftFire = CreateImage("LeftFire", Sprite("fire_pair"), robotRoot, new Vector2(-92f, -338f), new Vector2(84f, 136f));
        var rightFire = CreateImage("RightFire", Sprite("fire_pair"), robotRoot, new Vector2(92f, -338f), new Vector2(84f, 136f));
        rightFire.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        var legs = CreateImage("Legs", Sprite("legs"), robotRoot, new Vector2(0f, -118f));
        var body = CreateImage("Body", Sprite("body"), robotRoot, new Vector2(6f, 292f));
        var arms = CreateImage("Arms", Sprite("arms"), robotRoot, new Vector2(6f, 82f));
        var head = CreateImage("Head", Sprite("head"), robotRoot, new Vector2(0f, 422f));

        var lockedCharacter = CreateImage("LockedCharacter", Sprite("locked_character"), selectionRoot, new Vector2(6f, 112f));
        var lockIcon = CreateImage("LockIcon", Sprite("lock_icon"), selectionRoot, new Vector2(0f, 178f), new Vector2(140f, 168f));

        var leftButton = CreateButton("PreviousButton", Sprite("arrow_left"), canvasRoot, new Vector2(-470f, -64f), null, string.Empty, font);
        var rightButton = CreateButton("NextButton", Sprite("arrow_right"), canvasRoot, new Vector2(474f, -64f), null, string.Empty, font);
        var actionButton = CreateButton("ActionButton", Sprite("button_wide_low"), canvasRoot, new Vector2(0f, -1008f), new Vector2(520f, 190f), "УЛУЧШИТЬ", font);
        var backButton = CreateButton("BackButton", Sprite("button_back"), canvasRoot, new Vector2(-430f, -1002f), new Vector2(230f, 174f), string.Empty, font);

        CreateImage("ShopIcon", Sprite("shop_icon"), canvasRoot, new Vector2(428f, -1010f), new Vector2(92f, 98f));
        CreateText("Title", canvasRoot, "МЕНЮ ПЕРСОНАЖЕЙ", new Vector2(0f, 974f), new Vector2(1040f, 86f), 48f, new Color32(70, 244, 255, 255), TextAlignmentOptions.Center, font);
        var nameText = CreateText("NameText", canvasRoot, string.Empty, new Vector2(0f, 872f), new Vector2(980f, 74f), 48f, new Color32(255, 200, 58, 255), TextAlignmentOptions.Center, font);
        var descriptionText = CreateText("DescriptionText", canvasRoot, string.Empty, new Vector2(0f, 777f), new Vector2(930f, 104f), 28f, new Color32(220, 250, 255, 255), TextAlignmentOptions.Center, font);
        var stateText = CreateText("StateText", canvasRoot, string.Empty, new Vector2(0f, -792f), new Vector2(660f, 54f), 30f, new Color32(92, 245, 255, 255), TextAlignmentOptions.Center, font);
        var balanceText = CreateText("BalanceText", canvasRoot, string.Empty, new Vector2(430f, -918f), new Vector2(190f, 54f), 32f, Color.white, TextAlignmentOptions.Center, font);
        var priceText = CreateText("PriceText", canvasRoot, string.Empty, new Vector2(0f, -910f), new Vector2(260f, 54f), 32f, Color.white, TextAlignmentOptions.Center, font);
        var actionText = actionButton.GetComponentInChildren<TMP_Text>(true);

        var controller = new GameObject("CharacterMenuController").AddComponent<CharacterMenuController>();
        controller.transform.SetParent(canvasRoot, false);
        ConfigureController(
            controller,
            selectionRoot,
            selectionGroup,
            robotRoot,
            head.rectTransform,
            arms.rectTransform,
            legs.rectTransform,
            leftFire.rectTransform,
            rightFire.rectTransform,
            fan,
            ringLight,
            lockedCharacter,
            lockIcon,
            head,
            body,
            arms,
            legs,
            leftFire,
            rightFire,
            podiumGlow1,
            podiumGlow2,
            podiumGlow3,
            new[] { head, body, arms, legs },
            leftButton,
            rightButton,
            actionButton,
            backButton,
            nameText,
            descriptionText,
            balanceText,
            priceText,
            stateText,
            actionText);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode && !string.IsNullOrEmpty(previousScenePath))
            EditorSceneManager.OpenScene(previousScenePath);

        Debug.Log($"Character menu scene rebuilt: {ScenePath}");
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
        scaler.matchWidthOrHeight = 1f;

        return (RectTransform)canvasObject.transform;
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
        var rectTransform = image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        image.preserveAspect = false;
        return image;
    }

    private static Image CreateImage(string name, Sprite sprite, RectTransform parent, Vector2 position, Vector2? size = null)
    {
        var image = CreateImageObject(name, parent, sprite, Color.white);
        var rectTransform = image.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size ?? SpriteSize(sprite);
        image.preserveAspect = true;
        return image;
    }

    private static Image CreateImageObject(string name, RectTransform parent, Sprite sprite, Color color)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rectTransform = (RectTransform)gameObject.transform;
        rectTransform.SetParent(parent, false);
        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform CreateRect(string name, RectTransform parent, Vector2 position, Vector2 size)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        var rectTransform = (RectTransform)gameObject.transform;
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        return rectTransform;
    }

    private static Button CreateButton(string name, Sprite sprite, RectTransform parent, Vector2 position, Vector2? size, string label, TMP_FontAsset font)
    {
        var image = CreateImage(name, sprite, parent, position, size);
        image.raycastTarget = true;

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.8f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.62f, 0.92f, 1f, 1f);
        colors.disabledColor = new Color(0.35f, 0.45f, 0.48f, 0.72f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        image.gameObject.AddComponent<SciFiButtonPulse>();

        if (!string.IsNullOrEmpty(label))
            CreateText($"{name}Label", image.rectTransform, label, Vector2.zero, image.rectTransform.sizeDelta, 34f, new Color32(12, 84, 91, 255), TextAlignmentOptions.Center, font);

        return button;
    }

    private static TMP_Text CreateText(string name, RectTransform parent, string text, Vector2 position, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var rectTransform = (RectTransform)gameObject.transform;
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        var label = gameObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        return label;
    }

    private static void ConfigureController(
        CharacterMenuController controller,
        RectTransform selectionRoot,
        CanvasGroup selectionGroup,
        RectTransform robotRoot,
        RectTransform headTransform,
        RectTransform armsTransform,
        RectTransform legsTransform,
        RectTransform leftFireTransform,
        RectTransform rightFireTransform,
        Image fan,
        Image ringLight,
        Image lockedCharacter,
        Image lockIcon,
        Image headImage,
        Image bodyImage,
        Image armsImage,
        Image legsImage,
        Image leftFire,
        Image rightFire,
        Image podiumGlow1,
        Image podiumGlow2,
        Image podiumGlow3,
        Image[] tintedRobotImages,
        Button previousButton,
        Button nextButton,
        Button actionButton,
        Button backButton,
        TMP_Text nameText,
        TMP_Text descriptionText,
        TMP_Text balanceText,
        TMP_Text priceText,
        TMP_Text stateText,
        TMP_Text actionText)
    {
        var skinSprites = LoadSkinSprites();
        var count = Mathf.Max(skinSprites.Length, 12);
        var serializedObject = new SerializedObject(controller);

        SetReference(serializedObject, "_selectionRoot", selectionRoot);
        SetReference(serializedObject, "_selectionGroup", selectionGroup);
        SetReference(serializedObject, "_robotRoot", robotRoot);
        SetReference(serializedObject, "_headTransform", headTransform);
        SetReference(serializedObject, "_armsTransform", armsTransform);
        SetReference(serializedObject, "_legsTransform", legsTransform);
        SetReference(serializedObject, "_leftFireTransform", leftFireTransform);
        SetReference(serializedObject, "_rightFireTransform", rightFireTransform);
        SetReference(serializedObject, "_fan", fan);
        SetReference(serializedObject, "_ringLight", ringLight);
        SetReference(serializedObject, "_lockedCharacter", lockedCharacter);
        SetReference(serializedObject, "_lockIcon", lockIcon);
        SetReference(serializedObject, "_headImage", headImage);
        SetReference(serializedObject, "_bodyImage", bodyImage);
        SetReference(serializedObject, "_armsImage", armsImage);
        SetReference(serializedObject, "_legsImage", legsImage);
        SetReference(serializedObject, "_leftFire", leftFire);
        SetReference(serializedObject, "_rightFire", rightFire);
        SetReference(serializedObject, "_podiumGlow1", podiumGlow1);
        SetReference(serializedObject, "_podiumGlow2", podiumGlow2);
        SetReference(serializedObject, "_podiumGlow3", podiumGlow3);
        SetReferenceArray(serializedObject, "_tintedRobotImages", tintedRobotImages);
        SetReference(serializedObject, "_previousButton", previousButton);
        SetReference(serializedObject, "_nextButton", nextButton);
        SetReference(serializedObject, "_actionButton", actionButton);
        SetReference(serializedObject, "_backButton", backButton);
        SetReference(serializedObject, "_nameText", nameText);
        SetReference(serializedObject, "_descriptionText", descriptionText);
        SetReference(serializedObject, "_balanceText", balanceText);
        SetReference(serializedObject, "_priceText", priceText);
        SetReference(serializedObject, "_stateText", stateText);
        SetReference(serializedObject, "_actionText", actionText);
        SetReferenceArray(serializedObject, "_skinSprites", skinSprites);
        SetIntArray(serializedObject, "_prices", LoadPrices(count));
        SetStringArray(serializedObject, "_names", CharacterNames(count));
        SetStringArray(serializedObject, "_descriptions", CharacterDescriptions(count));
        SetColorArray(serializedObject, "_skinTints", CharacterTints(count));
        serializedObject.FindProperty("_gameSceneName").stringValue = "Game";
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureSprites(string folder)
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
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
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{MenuSpritesPath}/{name}.png");
        if (sprite == null)
            Debug.LogWarning($"Character menu sprite not found: {name}");
        return sprite;
    }

    private static Vector2 SpriteSize(Sprite sprite)
    {
        if (sprite == null)
            return new Vector2(100f, 100f);

        return new Vector2(sprite.rect.width, sprite.rect.height);
    }

    private static Sprite[] LoadSkinSprites()
    {
        var sprites = new List<Sprite>();
        for (var i = 0; i < 32; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlayerSpritesPath}/skin_{i}_idle.png");
            if (sprite != null)
                sprites.Add(sprite);
        }

        return sprites.ToArray();
    }

    private static int[] LoadPrices(int count)
    {
        var result = Enumerable.Repeat(0, count).ToArray();
        var installer = AssetDatabase.LoadAssetAtPath<SoInstaller>(SettingsPath);
        var skins = installer?.gameSettings?.skins;
        if (skins == null)
            return result;

        for (var i = 0; i < result.Length && i < skins.Length; i++)
            result[i] = skins[i].price;

        return result;
    }

    private static string[] CharacterNames(int count)
    {
        var values = new[]
        {
            "БКР-01 Сварог",
            "Искра-М",
            "Вектор-7",
            "Гранит",
            "Ласточка",
            "Неон",
            "Барс-3",
            "Рубин",
            "Тайга",
            "Орбита",
            "Пульсар",
            "Север"
        };
        return Fit(values, count);
    }

    private static string[] CharacterDescriptions(int count)
    {
        var values = new[]
        {
            "Базовый бункерный разведчик. Стабилен на длинных спусках и быстро выходит из скольжения.",
            "Импульсный модуль с ускоренной реакцией сервоприводов. Любит короткие серии платформ.",
            "Легкий курьерский корпус для нижних уровней комплекса. Точно держит траекторию.",
            "Тяжелый ремонтный робот с усиленной рамой. Хорош для осторожного ритма.",
            "Маневровый экспериментальный дрон с мягкими посадочными узлами.",
            "Ночной лабораторный прототип с холодной подсветкой и быстрым откликом.",
            "Сервисный корпус шахтного сектора. Надежен в плотном потоке препятствий.",
            "Красная серия аварийной службы. Спроектирован для рискованных спусков.",
            "Лесной инженерный модуль для автономной работы без связи с поверхностью.",
            "Орбитальная сборка закрытого отдела. Будет доступна в одном из следующих обновлений.",
            "Высоковольтный прототип с нестабильной катушкой. Будет доступен позже.",
            "Северный корпус особой серии. Будет открыт после расширения бункера."
        };
        return Fit(values, count);
    }

    private static Color[] CharacterTints(int count)
    {
        var values = new Color[]
        {
            new Color32(70, 242, 255, 255),
            new Color32(255, 103, 54, 255),
            new Color32(75, 255, 118, 255),
            new Color32(114, 172, 255, 255),
            new Color32(255, 194, 61, 255),
            new Color32(111, 255, 219, 255),
            new Color32(78, 188, 255, 255),
            new Color32(255, 92, 92, 255),
            new Color32(129, 255, 84, 255),
            new Color32(255, 128, 214, 255),
            new Color32(166, 140, 255, 255),
            new Color32(242, 255, 255, 255)
        };
        return Fit(values, count);
    }

    private static T[] Fit<T>(T[] source, int count)
    {
        var result = new T[count];
        for (var i = 0; i < count; i++)
            result[i] = source[i % source.Length];
        return result;
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        serializedObject.FindProperty(propertyName).objectReferenceValue = value;
    }

    private static void SetReferenceArray<T>(SerializedObject serializedObject, string propertyName, T[] values) where T : UnityEngine.Object
    {
        var property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void SetIntArray(SerializedObject serializedObject, string propertyName, int[] values)
    {
        var property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).intValue = values[i];
    }

    private static void SetStringArray(SerializedObject serializedObject, string propertyName, string[] values)
    {
        var property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).stringValue = values[i];
    }

    private static void SetColorArray(SerializedObject serializedObject, string propertyName, Color[] values)
    {
        var property = serializedObject.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).colorValue = values[i];
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(scene => scene.path == scenePath))
            return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
