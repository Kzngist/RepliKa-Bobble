using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUIHandler : MonoBehaviour
{
    [SerializeField] OptionsMenuUIHandler optionsMenu;
    [SerializeField] GameObject quitButton;
    [SerializeField] TMP_Text versionText;
    
    void Start()
    {
        if (InputManager.isTouchScreenSupported)
        {
            quitButton.SetActive(false);
        }

        versionText.text = "Version: " + Application.version;
    }

    public void OnStartButton()
    {
        LevelSelectionManager.Instance.currentGameMode = GameMode.Adventure;
        SceneLoadManager.Instance.LoadScene("LevelSelection");
    }
    
    public void OnArcadeButton()
    {
        LevelSelectionManager.Instance.InitializeArcadeMode();
        SceneLoadManager.Instance.LoadScene("ArcadeLevel");
    }

    public void OnOptionsButton()
    {
        optionsMenu.SetActive(true);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
