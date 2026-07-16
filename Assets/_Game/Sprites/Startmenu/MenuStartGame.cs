using UnityEngine;

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

        SceneLoadUtility.LoadSceneByName(sceneToLoad);
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

}
