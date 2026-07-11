#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class GameplayResultOverlayBuilder
{
    private const string SourceScenePath = "Assets/_Game/Scenes/Sandbox/LNTLoi/score.unity";
    private const string GameplayScenePath = "Assets/_Game/Scenes/Sandbox/tndKhoa/KhoaCuBu.unity";
    private const string PrefabPath = "Assets/_Game/Prefabs/Core/UI/RG Gameplay Result Overlay.prefab";
    private const string OverlayName = "RG Gameplay Result Overlay";
    private const string ControllerName = "RG Gameplay Result Controller";

    [MenuItem("Tools/RhythmGame/Gameplay/Build Result Overlay From LNTLoi")]
    public static void Build()
    {
        Scene gameplayScene = SceneManager.GetActiveScene();
        if (gameplayScene.path != GameplayScenePath)
        {
            EditorUtility.DisplayDialog(
                "Open Gameplay Scene",
                "Mở scene KhoaCuBu trước, sau đó chạy lại Tools/RhythmGame/Gameplay/Build Result Overlay From LNTLoi.",
                "OK");
            return;
        }

        Scene sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);
        try
        {
            Canvas sourceCanvas = FindCanvas(sourceScene);
            if (sourceCanvas == null)
            {
                Debug.LogError("GameplayResultOverlayBuilder: Could not find the result Canvas in LNTLoi/score.");
                return;
            }

            RemoveExisting(OverlayName);
            RemoveExisting(ControllerName);
            EnsureDirectory(PrefabPath);

            GameObject overlay = Object.Instantiate(sourceCanvas.gameObject);
            overlay.name = OverlayName;
            SceneManager.MoveGameObjectToScene(overlay, gameplayScene);
            ConfigureOverlayCanvas(overlay);

            ResultPro resultPro = overlay.GetComponentInChildren<ResultPro>(true);
            if (resultPro == null)
            {
                Object.DestroyImmediate(overlay);
                Debug.LogError("GameplayResultOverlayBuilder: ResultPro was not found in copied LNTLoi UI.");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(overlay, PrefabPath);
            Object.DestroyImmediate(overlay);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject instantiatedOverlay = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameplayScene);
            instantiatedOverlay.name = OverlayName;
            instantiatedOverlay.SetActive(false);

            GameObject controllerObject = new GameObject(ControllerName);
            SceneManager.MoveGameObjectToScene(controllerObject, gameplayScene);
            GameplayResultController controller = controllerObject.AddComponent<GameplayResultController>();
            controller.Configure(instantiatedOverlay);

            EditorSceneManager.MarkSceneDirty(gameplayScene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = controllerObject;
            Debug.Log("GameplayResultOverlayBuilder: Added LNTLoi result UI to KhoaCuBu. It will appear 2 seconds after the chart ends.");
        }
        finally
        {
            if (sourceScene.IsValid() && sourceScene.isLoaded)
                EditorSceneManager.CloseScene(sourceScene, true);
        }
    }

    private static Canvas FindCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
                return canvas;
        }

        return null;
    }

    private static void ConfigureOverlayCanvas(GameObject overlay)
    {
        overlay.transform.localScale = Vector3.one;
        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = overlay.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        ResultPro resultPro = overlay.GetComponentInChildren<ResultPro>(true);
        if (resultPro != null)
        {
            SerializedObject serialized = new SerializedObject(resultPro);
            serialized.FindProperty("playOnStartForPreview").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // LNTLoi dùng nhãn GOOD/BAD. Runtime của RhythmGame có Perfect/Great/Good/Miss.
        foreach (TextMeshProUGUI label in overlay.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (label.text == "GOOD")
                label.text = "GREAT";
            else if (label.text == "BAD")
                label.text = "GOOD";
        }
    }

    private static void RemoveExisting(string objectName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = allObjects.Length - 1; i >= 0; i--)
        {
            GameObject existing = allObjects[i];
            if (existing == null || existing.name != objectName)
                continue;

            if (!existing.scene.IsValid() || existing.scene != SceneManager.GetActiveScene())
                continue;

            Object.DestroyImmediate(existing);
        }
    }

    private static void EnsureDirectory(string assetPath)
    {
        string directory = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        if (string.IsNullOrEmpty(directory) || AssetDatabase.IsValidFolder(directory))
            return;

        string[] parts = directory.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
