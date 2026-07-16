#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class GameplaySfxCatalogBuilder
{
    private const string CatalogPath = "Assets/Resources/GameplaySfxCatalog.asset";

    [MenuItem("Tools/RhythmGame/Gameplay/Create or Update SFX Catalog")]
    public static void CreateOrUpdate()
    {
        EnsureResourcesFolder();

        GameplaySfxCatalog catalog = AssetDatabase.LoadAssetAtPath<GameplaySfxCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<GameplaySfxCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.fadeIn = LoadClip("FadeIn.wav");
        catalog.fadeOut = LoadClip("FateOut.wav");
        catalog.tap = LoadClip("tap.wav");
        catalog.arc = LoadClip("arc.wav");
        catalog.hpClear = LoadClip("hp_clear.wav");
        catalog.trackClear = LoadClip("track_clear.wav");
        catalog.trackFail = LoadClip("track_fail.wav");
        catalog.trackAllPerfect = LoadClip("track_AP.wav");
        catalog.unlock = LoadClip("unlock.wav");
        catalog.titleAllPerfect = LoadSprite("Title-Allperfect.png");
        catalog.titleFullCombo = LoadSprite("Title-fullcombo.png");
        catalog.titleTrackComplete = LoadSprite("Title-trackcomplete.png");
        catalog.titleTrackLost = LoadSprite("Title-tracklost.png");

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Gameplay] SFX catalog is ready at " + CatalogPath);
    }

    private static AudioClip LoadClip(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/" + fileName);
    }

    private static Sprite LoadSprite(string fileName)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Sprites/GamePlay/Chart/" + fileName);
    }

    private static void EnsureResourcesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
    }
}
#endif
