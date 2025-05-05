using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUIHandler : MenuUIHandler
{
    [SerializeField] OptionsMenuUIHandler optionsMenu;

    void Awake()
    {
        SetActive(false);
    }

    public void OnResumeButton()
    {
        SetActive(false);
        GameManager.Instance.TogglePause(false);
    }
    
    public void OnRestartButton()
    {
        SceneLoadManager.Instance.ReloadScene();
    }
    
    public void OnOptionsButton()
    {
        SetActive(false);
        
        optionsMenu.SetActive(true);
        optionsMenu.previousMenu = this;
    }

    public void OnQuitButton()
    {
        SceneLoadManager.Instance.LoadScene("LevelSelection");
    }
    
    public void OnExitButton()
    {
        SceneLoadManager.Instance.LoadScene("MainMenu");
    }
}
