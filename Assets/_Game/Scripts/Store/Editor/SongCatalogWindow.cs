#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Bulk editor for deciding which SongData assets are Free or sold in the Store.</summary>
public class SongCatalogWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string search = string.Empty;
    private readonly List<SongData> songs = new();

    [MenuItem("Tools/RhythmGame/Store/Song Catalog")]
    public static void Open()
    {
        GetWindow<SongCatalogWindow>("Song Catalog");
    }

    private void OnEnable()
    {
        ReloadSongs();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Song Catalog", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Manage Free and Purchase songs in one place. Changes are written directly to SongData assets.", MessageType.Info);

        using (new EditorGUILayout.HorizontalScope())
        {
            search = EditorGUILayout.TextField("Search", search);
            if (GUILayout.Button("Reload", GUILayout.Width(80f)))
                ReloadSongs();
        }

        EditorGUILayout.Space(4f);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (SongData song in songs)
        {
            if (song == null || !MatchesSearch(song))
                continue;

            DrawSong(song);
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawSong(SongData song)
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(song.SongTitle) ? song.name : song.SongTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(song.songGroupId) ? "Missing group ID" : song.songGroupId, EditorStyles.miniLabel);

            EditorGUI.BeginChangeCheck();
            SongUnlockType availability = (SongUnlockType)EditorGUILayout.EnumPopup("Availability", song.unlockType);
            int moneyPrice = song.moneyPrice;
            int diamondPrice = song.diamondPrice;
            if (availability == SongUnlockType.Purchase)
            {
                moneyPrice = Mathf.Max(0, EditorGUILayout.IntField("Money Price", moneyPrice));
                diamondPrice = Mathf.Max(0, EditorGUILayout.IntField("Diamond Price", diamondPrice));
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(song, "Update Song Catalog Entry");
                song.unlockType = availability;
                song.moneyPrice = moneyPrice;
                song.diamondPrice = diamondPrice;
                EditorUtility.SetDirty(song);
            }
        }
    }

    private bool MatchesSearch(SongData song)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        string needle = search.Trim().ToLowerInvariant();
        return (song.SongTitle ?? string.Empty).ToLowerInvariant().Contains(needle) ||
               (song.songGroupId ?? string.Empty).ToLowerInvariant().Contains(needle) ||
               song.name.ToLowerInvariant().Contains(needle);
    }

    private void ReloadSongs()
    {
        songs.Clear();
        foreach (string guid in AssetDatabase.FindAssets("t:SongData"))
        {
            SongData song = AssetDatabase.LoadAssetAtPath<SongData>(AssetDatabase.GUIDToAssetPath(guid));
            if (song != null)
                songs.Add(song);
        }

        songs.Sort((left, right) => string.Compare(left.SongTitle, right.SongTitle, System.StringComparison.OrdinalIgnoreCase));
    }
}
#endif
