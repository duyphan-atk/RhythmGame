using UnityEngine;

public class BackButton : MonoBehaviour
{
    public string targetScene = "StartMenu";

    public void GoBack()
    {
        SceneLoadUtility.LoadSceneByName(targetScene);
    }
}
