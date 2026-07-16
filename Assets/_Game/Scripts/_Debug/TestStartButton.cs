using UnityEngine;
public class TestStartButton : MonoBehaviour
{
	private const string SceneName = "chonnhaccuaban";

	public void StartGame()
	{
		SceneLoadUtility.LoadSceneByName(SceneName);
	}
}
