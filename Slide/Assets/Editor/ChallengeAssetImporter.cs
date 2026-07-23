using UnityEditor;
using UnityEngine;

public sealed class ChallengeAssetImporter : AssetPostprocessor
{
    private const string ChallengeAssetRoot = "Assets/Resources/Challenge/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ChallengeAssetRoot))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
    }
}
