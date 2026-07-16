using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public static class SceneLoadUtility
{
    public static void ReloadActiveScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        LoadSceneByName(SceneManager.GetActiveScene().name);
    }

    public static void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneLoadUtility: Scene name is empty.");
            return;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (!SceneTransitionController.RequestScene(sceneName))
            Debug.LogWarning("SceneLoadUtility: A scene transition is already in progress.");
    }

    internal static bool LoadSceneImmediately(string sceneName)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return true;
        }

#if UNITY_EDITOR
        if (TryLoadSceneInEditor(sceneName))
            return true;
#endif

        Debug.LogError("SceneLoadUtility: Cannot load scene '" + sceneName + "'.");
        return false;
    }

    internal static AsyncOperation LoadSceneAsync(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
            return null;

        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

#if UNITY_EDITOR
    private static bool TryLoadSceneInEditor(string sceneName)
    {
        string[] guids = AssetDatabase.FindAssets(sceneName + " t:Scene");
        for (int i = 0; i < guids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (System.IO.Path.GetFileNameWithoutExtension(scenePath) != sceneName)
                continue;

            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
        }

        return false;
    }
#endif
}
