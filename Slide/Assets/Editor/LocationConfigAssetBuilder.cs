using GameLogic;
using UnityEditor;
using UnityEngine;

public static class LocationConfigAssetBuilder
{
    private const string Folder = "Assets/Resources/Locations";

    [MenuItem("Slide/Locations/Create Missing Default Configs")]
    public static void EnsureAssets()
    {
        EnsureFolder();
        CreateIfMissing("FutureCity", LocationDefaults.CreateFutureCity(),
            "Assets/Sprites/MissionMenu/background_future_sky.png");
        CreateIfMissing("Jungle", LocationDefaults.CreateJungle(),
            "Assets/Sprites/MissionMenu/background_jungle.png");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        LocationCatalog.Reload();
    }

    private static void CreateIfMissing(string fileName, LocationConfig source,
        string backgroundAssetPath)
    {
        if (source == null)
            return;

        var assetPath = Folder + "/" + fileName + ".asset";
        if (AssetDatabase.LoadAssetAtPath<LocationConfig>(assetPath) != null)
        {
            Object.DestroyImmediate(source);
            return;
        }

        source.MissionMenuBackground = AssetDatabase.LoadAssetAtPath<Sprite>(backgroundAssetPath);
        source.hideFlags = HideFlags.None;
        AssetDatabase.CreateAsset(source, assetPath);
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/Resources", "Locations");
    }
}
