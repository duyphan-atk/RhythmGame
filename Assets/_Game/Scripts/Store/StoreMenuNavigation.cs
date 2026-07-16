using UnityEngine;
using UnityEngine.UI;

/// <summary>Binds StoreMenu navigation without relying on serialized button events.</summary>
public class StoreMenuNavigation : MonoBehaviour
{
    private void Start()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button != null && button.name == "Back")
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(GoToStartMenu);
                return;
            }
        }
    }

    private static void GoToStartMenu()
    {
        SceneLoadUtility.LoadSceneByName("StartMenu");
    }
}
