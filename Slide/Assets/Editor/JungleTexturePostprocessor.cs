using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public sealed class JungleTexturePostprocessor : AssetPostprocessor
{
    private const string JungleRoot = "Assets/Resources/Jungle/";
    private const string EnvironmentRoot = JungleRoot + "Environment/";
    private static readonly Vector2 StartPlatformPivot = new Vector2(0.5f, 64f / 1448f);

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(JungleRoot))
            return;

        Configure((TextureImporter)assetImporter);
    }

    [InitializeOnLoadMethod]
    private static void ConfigureExistingAssets()
    {
        EditorApplication.delayCall += () =>
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { JungleRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || !NeedsUpdate(importer))
                    continue;

                Configure(importer);
                importer.SaveAndReimport();
            }
        };
    }

    private static bool NeedsUpdate(TextureImporter importer)
    {
        var usesSheet = TryGetSheetLayout(importer.assetPath, out var columns, out var rows);
        var needsStartPlatformPivot = importer.assetPath.EndsWith("/Start/start_platform.png");
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        return importer.textureType != TextureImporterType.Sprite ||
               importer.spriteImportMode != (usesSheet ? SpriteImportMode.Multiple : SpriteImportMode.Single) ||
               !Mathf.Approximately(importer.spritePixelsPerUnit, GetPixelsPerUnit(importer.assetPath)) ||
               importer.filterMode != FilterMode.Point ||
               importer.mipmapEnabled ||
               importer.textureCompression != TextureImporterCompression.Uncompressed ||
               importer.wrapMode != TextureWrapMode.Clamp ||
               (needsStartPlatformPivot &&
                (textureSettings.spriteAlignment != (int)SpriteAlignment.Custom ||
                 (textureSettings.spritePivot - StartPlatformPivot).sqrMagnitude > 0.000001f));
    }

    private static void Configure(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = TryGetSheetLayout(importer.assetPath, out var columns, out var rows)
            ? SpriteImportMode.Multiple
            : SpriteImportMode.Single;
        importer.spritePixelsPerUnit = GetPixelsPerUnit(importer.assetPath);
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        if (columns > 0 && rows > 0)
            ConfigureSpritesheet(importer, columns, rows);

        if (importer.assetPath.EndsWith("/Start/start_platform.png"))
        {
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            textureSettings.spritePivot = StartPlatformPivot;
            importer.SetTextureSettings(textureSettings);
        }
    }

    private static float GetPixelsPerUnit(string assetPath)
    {
        // Background source art is 320 px wide; gameplay art was supplied at four times
        // the project grid resolution and must retain the existing collider dimensions.
        return assetPath.StartsWith(EnvironmentRoot) && !assetPath.EndsWith("/Environment/glass.png")
            ? 100f
            : 400f;
    }

    private static bool TryGetSheetLayout(string assetPath, out int columns, out int rows)
    {
        columns = 0;
        rows = 0;
        if (assetPath.EndsWith("/VFX/static_bomb.png"))
        {
            columns = 3;
            rows = 2;
        }
        else if (assetPath.EndsWith("/VFX/barrier.png"))
        {
            columns = 2;
            rows = 2;
        }
        else if (assetPath.EndsWith("/VFX/moving_bomb.png"))
        {
            columns = 4;
            rows = 2;
        }
        else if (assetPath.EndsWith("/VFX/wall.png"))
        {
            columns = 6;
            rows = 1;
        }

        return columns > 0;
    }

    private static void ConfigureSpritesheet(TextureImporter importer, int columns, int rows)
    {
        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var dataProvider = factories.GetSpriteEditorDataProviderFromObject(importer);
        if (dataProvider == null)
            return;

        dataProvider.InitSpriteEditorDataProvider();
        dataProvider.SetSpriteRects(CreateSpritesheet(importer.assetPath, columns, rows));
        dataProvider.Apply();
    }

    private static SpriteRect[] CreateSpritesheet(string assetPath, int columns, int rows)
    {
        if (!TryGetSheetSize(assetPath, out var width, out var height))
            return new SpriteRect[0];

        var sprites = new SpriteRect[columns * rows];
        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var index = row * columns + column;
                var xMin = width * column / columns;
                var xMax = width * (column + 1) / columns;
                var yMin = height * (rows - row - 1) / rows;
                var yMax = height * (rows - row) / rows;
                sprites[index] = new SpriteRect
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(assetPath) + "_" + index.ToString("00"),
                    rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin),
                    pivot = new Vector2(0.5f, 0.5f),
                    alignment = SpriteAlignment.Center,
                    spriteID = GUID.Generate()
                };
            }
        }

        return sprites;
    }

    private static bool TryGetSheetSize(string assetPath, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (assetPath.EndsWith("/VFX/static_bomb.png"))
        {
            width = 552;
            height = 364;
        }
        else if (assetPath.EndsWith("/VFX/barrier.png"))
        {
            width = 752;
            height = 336;
        }
        else if (assetPath.EndsWith("/VFX/moving_bomb.png"))
        {
            width = 416;
            height = 456;
        }
        else if (assetPath.EndsWith("/VFX/wall.png"))
        {
            width = 692;
            height = 156;
        }

        return width > 0;
    }
}
