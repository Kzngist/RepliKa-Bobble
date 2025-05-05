
using UnityEngine;

public class ResultScreenUIHandler : MonoBehaviour
{
    internal static ResultScreenUIHandler Instance;

    public void OnQuit()
    {
        SceneLoadManager.Instance.LoadScene("LevelSelection");
    }
    
    public void OnRetry()
    {
        SceneLoadManager.Instance.ReloadScene();
    }

    public void OnNext()
    {
        LevelSelectionManager.Instance.LoadNextLevel();
    }

    public void OnExit()
    {
        SceneLoadManager.Instance.LoadScene("MainMenu");
    }
}
