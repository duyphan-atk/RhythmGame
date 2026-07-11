using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class MenuStartGame : MonoBehaviour
{
    private const string DefaultSongSelectSceneName = "SongSelect";

    public GameObject settingPanel;
    [SerializeField] private string gameplaySceneName = DefaultSongSelectSceneName;

    // START
    public void StartGame()
    {
        string sceneToLoad = string.IsNullOrWhiteSpace(gameplaySceneName)
            ? DefaultSongSelectSceneName
            : gameplaySceneName;

        Debug.Log("MenuStartGame: Loading scene '" + sceneToLoad + "'.");

        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

#if UNITY_EDITOR
        if (TryLoadSceneInEditor(sceneToLoad))
            return;
#endif

        Debug.LogError("MenuStartGame: Cannot load scene '" + sceneToLoad + "'.");
    }

   
    // EXIT
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
