using UnityEditor;
using UnityEngine;
using UI;

public sealed class FutureCityTexturePostprocessor : AssetPostprocessor
{
    private const string FutureCityRoot = "Assets/Resources/FutureCity/";
    private static readonly Vector2 StartPlatformPivot = new Vector2(0.5f, 16f / 362f);

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(FutureCityRoot))
            return;

        Configure((TextureImporter)assetImporter);
    }

    [InitializeOnLoadMethod]
    private static void ConfigureExistingAssets()
    {
        EditorApplication.delayCall += () =>
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { FutureCityRoot });
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
        var needsTopPivot = importer.assetPath.EndsWith("/Start/start_platform.png");
        var textureSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(textureSettings);
        return importer.textureType != TextureImporterType.Sprite ||
               importer.spriteImportMode != SpriteImportMode.Single ||
               !Mathf.Approximately(importer.spritePixelsPerUnit, 100f) ||
               importer.filterMode != FilterMode.Point ||
               importer.mipmapEnabled ||
               importer.textureCompression != TextureImporterCompression.Uncompressed ||
               importer.wrapMode != TextureWrapMode.Clamp ||
               (needsTopPivot && (textureSettings.spriteAlignment != (int)SpriteAlignment.Custom ||
                                  (textureSettings.spritePivot - StartPlatformPivot).sqrMagnitude > 0.000001f));
    }

    private static void Configure(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        if (importer.assetPath.EndsWith("/Start/start_platform.png"))
        {
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            // The source platform surface is 16 px above the bottom of the 362 px image.
            textureSettings.spritePivot = StartPlatformPivot;
            importer.SetTextureSettings(textureSettings);
        }
    }

    [MenuItem("Slide/Future City/Start Playtest")]
    private static void StartPlaytest()
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode before starting the Future City playtest.");
            return;
        }

        var uiController = Object.FindAnyObjectByType<UIController>();
        if (uiController != null)
            uiController.PlayGame(true);
    }
}
